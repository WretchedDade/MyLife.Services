using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;

namespace MyLife.Services.Functions.Functions
{
    public class UploadBloodPressureReadings
    {
        private readonly ILogger _logger;
        private readonly INotionAPI _notionAPI;
        private readonly IBloodPressureService _bloodPressureService;

        public UploadBloodPressureReadings(ILoggerFactory loggerFactory, INotionAPI notionAPI, IBloodPressureService bloodPressureService)
        {
            _logger = loggerFactory.CreateLogger<UploadBloodPressureReadings>();
            _notionAPI = notionAPI;
            _bloodPressureService = bloodPressureService;
        }

        [Function("UploadBloodPressureReadings")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var bloodPressureDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBloodPressureDatabaseId);

            var bloodPressureReadings = await _notionAPI.QueryDatabase<NotionPage>(bloodPressureDatabaseId);

            foreach(var bloodPressureReading in bloodPressureReadings)
            {
                var systolic = bloodPressureReading.GetProperty("SYS (Top)")?.Number;
                var diastolic = bloodPressureReading.GetProperty("DIA (Bottom)")?.Number;
                var date = bloodPressureReading.GetProperty("Date")?.Date?.StartDate;

                if(systolic == null || diastolic == null || date == null)
                {
                    _logger.LogWarning("Blood Pressure Reading Missing Required Properties");
                    continue;
                }

                await _bloodPressureService.CreateReading((int)systolic.Value, (int)diastolic.Value, null, date.Value);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
