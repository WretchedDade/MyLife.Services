using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;
using System.Linq;
using System.Threading.Tasks;

namespace MyLife.Services.Functions
{
    public class GetBillPayments
    {
        private readonly ILogger _logger;
        private readonly INotionAPI _notionAPI;

        public GetBillPayments(ILoggerFactory loggerFactory, INotionAPI notionAPI)
        {
            _logger = loggerFactory.CreateLogger<GetBillPayments>();
            _notionAPI = notionAPI;
        }

        [Function("GetBillPayments")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bills")] HttpRequest req, [FromQuery] bool? unpaidOnly = null)
        {
            var billPaymentsDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBillPaymentsDatabaseId);

            NotionFilter? filter = null;

            if (unpaidOnly.HasValue)
                filter = new() { Property = "Bill Paid", Checkbox = NotionFilterCheckbox.ThatEquals(true) };

            var list = await _notionAPI.QueryDatabase<NotionPage>(billPaymentsDatabaseId, filter: filter);

            var billPayments = list.Results.Select(page => new BillPayment(page));

            return new OkObjectResult(billPayments);
        }
    }
}
