using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Extensions;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.Functions
{
    public class CreateBillPayments
    {
        private readonly INotionAPI _notionAPI;
        private readonly INotionService _notionService;
        private readonly ILogger<CreateBillPayments> _logger;

        public CreateBillPayments(INotionAPI notionAPI, INotionService notionService, ILoggerFactory loggerFactory)
        {
            _notionAPI = notionAPI;
            _notionService = notionService;

            _logger = loggerFactory.CreateLogger<CreateBillPayments>();
        }

        [Function("CreateBillPayments")]
        public async Task RunAsync([TimerTrigger("0 0 6 * * *")] TimerInfo timerInfo)
        {
            var billConfigurations = await GetBillConfigurations();

            if (!billConfigurations.Any())
                _logger.LogWarning("No Bill Configurations Found");

            foreach (var billConfiguration in billConfigurations)
            {
                await CreateBillPaymentIfNotExists(billConfiguration);
            }
        }

        private async Task<IEnumerable<BillConfigurationPage>> GetBillConfigurations()
        {
            var databaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBillConfigurationDatabaseId);
            var pages = await _notionAPI.QueryDatabase<BillConfigurationPage>(databaseId);

            return pages;
        }

        private async Task CreateBillPaymentIfNotExists(BillConfigurationPage billConfiguration)
        {
            var billPaymentsDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBillPaymentsDatabaseId);

            DateTime? nextPaymentDate = GetNextPaymentDate(billConfiguration);

            if (!nextPaymentDate.HasValue)
                // Next Payment Date could not be determined, unable to create Bill Payment
                return;

            if (nextPaymentDate.Value.Date.Subtract(DateTime.Today).TotalDays > 14)
                // Next Payment Date is more than 14 days away, do not create Bill Payment
                return;

            NotionFilter filter = new()
            {
                And = new()
                {
                    new()
                    {
                        Property = "Bill Configuration",
                        Relation = new() { Contains = billConfiguration.Id }
                    },
                    new()
                    {
                        Property = "Date",
                        Date = new(){ Equals = nextPaymentDate.Value.Date },
                    }
                }
            };

            var results = await _notionAPI.QueryDatabase<NotionPage>(billPaymentsDatabaseId, filter: filter);

            if (results.Any())
            {
                if (billConfiguration.IsAutoPay && DateTime.Today == nextPaymentDate.Value.Date)
                    await AutoPayBillPayment(results.First());

                // There is already a bill payment for this payment date, do not create a bill payment
                return;
            }

            await CreateBillPayment(nextPaymentDate.Value, billConfiguration, billPaymentsDatabaseId);
        }

        private DateTime? GetNextPaymentDate(BillConfigurationPage billConfiguration)
        {
            if (billConfiguration.DayDueType == DayDueTypes.Fixed)
            {
                if (!billConfiguration.DayDue.HasValue)
                {
                    _logger.LogError("Bill Configuration DayDue is null but the DayDueType is Fixed");
                    return null;
                }

                if (DateTime.Today.Day <= billConfiguration.DayDue.Value)
                    // Bill is due in current month
                    return new DateTime(DateTime.Today.Year, DateTime.Today.Month, billConfiguration.DayDue.Value, 0,0,0, DateTimeKind.Utc);
                else
                {
                    // Bill isn't due till next month
                    return new DateTime(DateTime.Today.Year, DateTime.Today.Month + 1, billConfiguration.DayDue.Value, 0, 0, 0, DateTimeKind.Utc);
                }
            }
            else if (billConfiguration.DayDueType == DayDueTypes.EndOfMonth)
            {
                DateTime endOfCurrentMonth = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc).ToEndOfMonth();

                if (DateTime.Today.Day < endOfCurrentMonth.Day)
                    // Bill is due in current month
                    return endOfCurrentMonth;
                else
                {
                    // Bill isn't due till next month
                    return endOfCurrentMonth.AddMonths(1);
                }
            }
            else
            {
                throw new NotImplementedException($"DayDueType {billConfiguration.DayDueType} is not implemented");
            }
        }

        private async Task CreateBillPayment(DateTime paymentDate, BillConfigurationPage billConfiguration, string billPaymentsDatabaseId)
        {
            try
            {
                await _notionAPI.CreatePage(new()
                {
                    Icon = billConfiguration.Icon,
                    Cover = billConfiguration.Cover,

                    Parent = new()
                    {
                        DatabaseId = billPaymentsDatabaseId
                    },

                    Properties = new()
                    {
                        { "Date", NotionProperty.OfDate(paymentDate.ToString("yyyy-MM-dd")) },
                        { "Name", NotionProperty.OfTitle($"{billConfiguration.Name} - {paymentDate.ToShortDateString()}") },
                        { "Bill Configuration", NotionProperty.OfRelationship(billConfiguration.Id) },
                        { "Is Auto-Pay", NotionProperty.OfCheckbox(billConfiguration.IsAutoPay) }
                    }
                });

                _logger.LogInformation($"Created Bill Payment for {billConfiguration} due on {paymentDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Bill Payment");
            }
        }

        private async Task AutoPayBillPayment(NotionPage billPayment)
        {
            if (billPayment.GetProperty("Bill Paid")?.IsChecked == true)
                return;

            var name = billPayment.Properties["Name"].RichText?[0].PlainText;

            try
            {
                await _notionService.MarkBillAsPaid(billPayment.Id);

                _logger.LogInformation($"{name} Auto-Paid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error auto-paying bill {name}");

            }
        }
    }
}
