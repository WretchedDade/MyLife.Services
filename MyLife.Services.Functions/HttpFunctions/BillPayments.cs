using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.HttpFunctions
{
    public class BillPayments
    {
        private readonly ILogger _logger;
        private readonly INotionAPI _notionAPI;
        private readonly INotionService _notionService;

        public BillPayments(ILoggerFactory loggerFactory, INotionAPI notionAPI, INotionService notionService)
        {
            _logger = loggerFactory.CreateLogger<BillPayments>();

            _notionAPI = notionAPI;
            _notionService = notionService;
        }

        [Function("GetBillPayments")]
        public async Task<IActionResult> GetAll([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bills")] HttpRequest request, FunctionContext context, [FromQuery] bool? unpaidOnly = null)
        {
            var billPaymentsDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBillPaymentsDatabaseId);

            NotionFilter? filter = null;

            if (unpaidOnly.HasValue)
                filter = new() { Property = "Bill Paid", Checkbox = NotionFilterCheckbox.ThatEquals(false) };

            var list = await _notionAPI.QueryDatabase<NotionPage>(billPaymentsDatabaseId, filter: filter);

            var billPayments = list.Results.Select(page => new BillPayment(page)).OrderBy(bill => bill.DateDue);

            return new OkObjectResult(billPayments);
        }

        [Function("MarkBillAsPaid")]
        public async Task<IActionResult> MarkBillAsPaid([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bills/{id:guid}/pay")] HttpRequest request, FunctionContext context, Guid id)
        {
            var billPayment = await _notionService.MarkBillAsPaid(id.ToString());

            return new OkObjectResult(billPayment);
        }
    }
}
