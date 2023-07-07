using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.Functions
{
    public class SyncBudgetWithBillConfiguration
    {
        private readonly INotionAPI _notionAPI;
        private readonly ILogger<SyncBudgetWithBillConfiguration> _logger;

        public SyncBudgetWithBillConfiguration(INotionAPI notionAPI, ILoggerFactory loggerFactory)
        {
            _notionAPI = notionAPI;

            _logger = loggerFactory.CreateLogger<SyncBudgetWithBillConfiguration>();
        }

        [Function("SyncBudgetWithBillConfiguration")]
        public async Task RunAsync([TimerTrigger("0 0 6 * * *")] TimerInfo timerInfo)
        {
            var databaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBillConfigurationDatabaseId);
            var pages = await _notionAPI.QueryDatabase<BillConfigurationPage>(databaseId);

            if (!pages.Any())
                _logger.LogWarning("No Bill Configurations Found");

            foreach (var billConfiguration in pages)
            {
                if (string.IsNullOrEmpty(billConfiguration.BudgetItemId))
                    await CreateBudgetItem(billConfiguration);
                else
                    await UpdateBudgetItem(billConfiguration.BudgetItemId, billConfiguration);
            }
        }

        private async Task CreateBudgetItem(BillConfigurationPage billConfiguration)
        {
            try
            {
                var budgetItemPage = MapFromBillConfiguration(billConfiguration);

                await _notionAPI.CreatePage(budgetItemPage);

                _logger.LogInformation($"Created Budget Item for {billConfiguration.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error was encountered when creating the budget item for {billConfiguration.Name}");
            }
        }

        private async Task UpdateBudgetItem(string budgetItemId, BillConfigurationPage billConfiguration)
        {
            try
            {
                var budgetItemPage = MapFromBillConfiguration(billConfiguration);

                await _notionAPI.UpdatePage(budgetItemId, budgetItemPage.Properties, icon: billConfiguration.Icon, cover: billConfiguration.Cover);

                _logger.LogInformation($"Updated Budget Item for {billConfiguration.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error was encountered when updating the budget item for {billConfiguration.Name}");
            }
        }

        private NotionPage MapFromBillConfiguration(BillConfigurationPage billConfiguration)
        {
            var notionBudgetDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBudgetDatabaseId);


            var dateType = billConfiguration.DayDueType switch
            {
                DayDueTypes.EndOfMonth => "End of Month",
                DayDueTypes.Fixed or _ => "Fixed"
            };

            var tags = billConfiguration.GetProperty("Tags")?.MultiSelect?.Select(option => option.Name).Where(option => option != "Auto-Pay") ?? Enumerable.Empty<string>();

            NotionPage page = new()
            {
                Icon = billConfiguration.Icon,
                Cover = billConfiguration.Cover,

                Parent = new()
                {
                    DatabaseId = notionBudgetDatabaseId
                },

                Properties = new()
                {
                    { "Name", NotionProperty.OfTitle(billConfiguration.Name) },

                    { "Category", NotionProperty.OfSelect("Bill") },
                    { "Tags", NotionProperty.OfMultiSelect(tags.ToArray()) },
                    { "Amount", NotionProperty.OfNumber(billConfiguration.Amount)},

                    { "Date Type", NotionProperty.OfSelect(dateType)},

                    { "Bill", NotionProperty.OfRelationship(billConfiguration.Id) },
                }
            };

            if (billConfiguration.DayDueType == DayDueTypes.Fixed)
                page.Properties.Add("Day", NotionProperty.OfNumber(billConfiguration.DayDue));

            return page;
        }
    }
}
