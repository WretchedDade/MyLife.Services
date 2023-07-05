using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion;
using MyLife.Services.Shared.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.HttpFunctions
{
    public class BillConfigurations
    {
        private readonly ILogger _logger;
        private readonly INotionAPI _notionAPI;

        public BillConfigurations(ILoggerFactory loggerFactory, INotionAPI notionAPI)
        {
            _logger = loggerFactory.CreateLogger<BillConfigurations>();
            _notionAPI = notionAPI;
        }

        [Function("GetBillConfiguration")]
        public async Task<IActionResult> GetById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "billconfigurations/{id:guid}")] HttpRequest request, FunctionContext context, Guid id)
        {
            var billConfigurationPage = await _notionAPI.GetPage<BillConfigurationPage>(id.ToString());

            if (billConfigurationPage is BillConfigurationPage page)
                return new OkObjectResult(new BillConfiguration(page));
            else
                return new NotFoundResult();
        }

        [Function("GetBillConfigurations")]
        public async Task<IActionResult> GetAll([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "billconfigurations")] HttpRequest request, FunctionContext context)
        {
            var billPaymentsDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBillConfigurationDatabaseId);

            var list = await _notionAPI.QueryDatabase<BillConfigurationPage>(billPaymentsDatabaseId);

            var billConfigurations = list.Results.Select(page => new BillConfiguration(page));

            return new OkObjectResult(billConfigurations);
        }
    }
}
