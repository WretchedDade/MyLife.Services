using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Extensions;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.Functions
{
    public class SyncBudgetWithExpenseConfiguration
    {
        private readonly INotionAPI _notionAPI;
        private readonly ILogger<SyncBudgetWithExpenseConfiguration> _logger;

        public SyncBudgetWithExpenseConfiguration(INotionAPI notionAPI, ILoggerFactory loggerFactory)
        {
            _notionAPI = notionAPI;

            _logger = loggerFactory.CreateLogger<SyncBudgetWithExpenseConfiguration>();
        }

        [Function("SyncBudgetWithExpenseConfiguration")]
        public async Task RunAsync([TimerTrigger("0 0 6 * * *")] TimerInfo timerInfo)
        {
            var databaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionExpenseConfigurationDatabaseId);
            var pages = await _notionAPI.QueryDatabase<NotionPage>(databaseId);

            if (!pages.Any())
                _logger.LogWarning("No Expense Configurations Found");

            foreach (var page in pages)
            {
                await SyncIncomeConfiguration(page);
            }
        }

        private async Task SyncIncomeConfiguration(NotionPage page)
        {
            await (page.GetProperty("Frequency")?.Select?.Name switch
            {
                "Bi-Weekly" => SyncBiWeeklyBudgetItem(page),
                "Weekly" => SyncWeeklyBudgetItem(page),
                "Week-Daily" => SyncWeekDailyBudgetItem(page),
                "Monthly" => SyncMonthlyBudgetItem(page),
                _ => Task.CompletedTask,
            });
        }

        private async Task SyncBiWeeklyBudgetItem(NotionPage page)
        {
            List<string> budgetItemIds = page.GetProperty("Budget Items")?.Relationships?.Select(relationship => relationship.Id).ToList() ?? new();

            if (budgetItemIds.Count > 0)
            {
                if (budgetItemIds.Count > 2)
                {
                    await _notionAPI.DeletePages(budgetItemIds.Skip(2).ToArray());
                }

                await UpdateBudgetItem(budgetItemIds[0], 15, page.Name, page);
                await UpdateBudgetItem(budgetItemIds[1], int.MaxValue, page.Name, page);
            }
            else
            {
                await CreateBudgetItem(15, page.Name, page);
                await CreateBudgetItem(int.MaxValue, page.Name, page);
            }
        }

        private async Task SyncWeeklyBudgetItem(NotionPage page)
        {
            List<string> budgetItemIds = page.GetProperty("Budget Items")?.Relationships?.Select(relationship => relationship.Id).ToList() ?? new();

            if (budgetItemIds.Count > 0)
            {
                if (budgetItemIds.Count > 4)
                {
                    await _notionAPI.DeletePages(budgetItemIds.Skip(4).ToArray());
                }

                await UpdateBudgetItem(budgetItemIds[0], 7, page.Name, page);
                await UpdateBudgetItem(budgetItemIds[1], 14, page.Name, page);
                await UpdateBudgetItem(budgetItemIds[2], 21, page.Name, page);
                await UpdateBudgetItem(budgetItemIds[3], int.MaxValue, page.Name, page);
            }
            else
            {
                await CreateBudgetItem(7, page.Name, page);
                await CreateBudgetItem(14, page.Name, page);
                await CreateBudgetItem(21, page.Name, page);
                await CreateBudgetItem(int.MaxValue, page.Name, page);
            }
        }

        private async Task SyncWeekDailyBudgetItem(NotionPage page)
        {
            List<string> budgetItemIds = page.GetProperty("Budget Items")?.Relationships?.Select(relationship => relationship.Id).ToList() ?? new();

            page.Properties["Amount"].Number = page.Properties["Amount"].Number.GetValueOrDefault() * 5;

            if (budgetItemIds.Count > 0)
            {
                if (budgetItemIds.Count > 4)
                {
                    await _notionAPI.DeletePages(budgetItemIds.Skip(4).ToArray());
                }

                await UpdateBudgetItem(budgetItemIds[0], 7, page.Name, page);
                await UpdateBudgetItem(budgetItemIds[1], 14, page.Name, page);
                await UpdateBudgetItem(budgetItemIds[2], 21, page.Name, page);
                await UpdateBudgetItem(budgetItemIds[3], int.MaxValue, page.Name, page);
            }
            else
            {
                await CreateBudgetItem(7, page.Name, page);
                await CreateBudgetItem(14, page.Name, page);
                await CreateBudgetItem(21, page.Name, page);
                await CreateBudgetItem(int.MaxValue, page.Name, page);
            }
        }

        private async Task SyncMonthlyBudgetItem(NotionPage page)
        {
            await Task.CompletedTask;
        }

        private async Task CreateBudgetItem(int day, string name, NotionPage page)
        {
            try
            {
                var budgetItemPage = MapFromBillConfiguration(day, name, page);

                await _notionAPI.CreatePage(budgetItemPage);

                _logger.LogInformation($"Created Budget Item for {page.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error was encountered when creating the budget item for {page.Name}");
            }
        }

        private async Task UpdateBudgetItem(string budgetItemId, int day, string name, NotionPage page)
        {
            try
            {
                var budgetItemPage = MapFromBillConfiguration(day, name, page);

                await _notionAPI.UpdatePage(budgetItemId, budgetItemPage.Properties, icon: page.Icon, cover: page.Cover);

                _logger.LogInformation($"Updated Budget Item for {page.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error was encountered when updating the budget item for {page.Name}");
            }
        }

        private NotionPage MapFromBillConfiguration(int day, string name, NotionPage page)
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
                    { "Name", NotionProperty.OfTitle(name) },

                    { "Category", NotionProperty.OfSelect("Expense") },
                    { "Tags", NotionProperty.OfMultiSelect() },
                    { "Amount", NotionProperty.OfNumber(page.GetProperty("Amount")?.Number.GetValueOrDefault())},

                    { "Date Type", NotionProperty.OfSelect(day == int.MaxValue ? "End of Month" : "Fixed")},

                    { "Expense", NotionProperty.OfRelationship(page.Id) },
                }
            };

            if (day != int.MaxValue)
                budgetItemPage.Properties.Add("Day", NotionProperty.OfNumber(day));

            return budgetItemPage;
        }
    }
}
