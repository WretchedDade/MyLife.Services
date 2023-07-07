using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Services.Shared.Extensions;

namespace MyLife.Services.Functions.Functions
{
    public class SyncBudgetWithIncomeConfiguration
    {
        private readonly INotionAPI _notionAPI;
        private readonly ILogger<SyncBudgetWithIncomeConfiguration> _logger;

        public SyncBudgetWithIncomeConfiguration(INotionAPI notionAPI, ILoggerFactory loggerFactory)
        {
            _notionAPI = notionAPI;

            _logger = loggerFactory.CreateLogger<SyncBudgetWithIncomeConfiguration>();
        }

        [Function("SyncBudgetWithIncomeConfiguration")]
        public async Task RunAsync([TimerTrigger("0 0 6 * * *")] TimerInfo timerInfo)
        {
            var databaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionIncomeConfigurationDatabaseId);
            var pages = await _notionAPI.QueryDatabase<NotionPage>(databaseId);

            if (!pages.Any())
                _logger.LogWarning("No Income Configurations Found");

            foreach (var page in pages)
            {
                await SyncIncomeConfiguration(page);
            }
        }

        private async Task SyncIncomeConfiguration(NotionPage page)
        {
            if (page.GetProperty("Frequency")?.Select?.Name == "Bi-Weekly")
            {
                List<string> budgetItemIds = page.GetProperty("Budget Items")?.Relationships?.Select(relationship => relationship.Id).ToList() ?? new();

                if (budgetItemIds.Count > 0)
                {
                    if (budgetItemIds.Count > 2)
                    {
                        await _notionAPI.DeletePages(budgetItemIds.Skip(2).ToArray());
                    }

                    await UpdateBudgetItem(budgetItemIds[0], 15, page);
                    await UpdateBudgetItem(budgetItemIds[1], int.MaxValue, page);
                }
                else
                {
                    await CreateBudgetItem(15, page);
                    await CreateBudgetItem(int.MaxValue, page);
                }
            }
        }

        private async Task CreateBudgetItem(int day, NotionPage page)
        {
            try
            {
                var budgetItemPage = MapFromBillConfiguration(day, page);

                await _notionAPI.CreatePage(budgetItemPage);

                _logger.LogInformation($"Created Budget Item for {page.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error was encountered when creating the budget item for {page.Name}");
            }
        }

        private async Task UpdateBudgetItem(string budgetItemId, int day, NotionPage page)
        {
            try
            {
                var budgetItemPage = MapFromBillConfiguration(day, page);

                await _notionAPI.UpdatePage(budgetItemId, budgetItemPage.Properties, icon: page.Icon, cover: page.Cover);

                _logger.LogInformation($"Updated Budget Item for {page.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error was encountered when updating the budget item for {page.Name}");
            }
        }

        private NotionPage MapFromBillConfiguration(int day, NotionPage page)
        {
            var notionBudgetDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBudgetDatabaseId);

            NotionPage budgetItemPage = new()
            {
                Icon = page.Icon,
                Cover = page.Cover,

                Parent = new()
                {
                    DatabaseId = notionBudgetDatabaseId
                },

                Properties = new()
                {
                    { "Name", NotionProperty.OfTitle(page.Name) },

                    { "Category", NotionProperty.OfSelect("Income") },
                    { "Tags", NotionProperty.OfMultiSelect() },
                    { "Amount", NotionProperty.OfNumber(page.GetProperty("Amount")?.Number.GetValueOrDefault())},

                    { "Date Type", NotionProperty.OfSelect(day == int.MaxValue ? "End of Month" : "Fixed")},

                    { "Income", NotionProperty.OfRelationship(page.Id) },
                }
            };

            if (day != int.MaxValue)
                budgetItemPage.Properties.Add("Day", NotionProperty.OfNumber(day));

            return budgetItemPage;
        }
    }
}
