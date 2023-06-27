using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MyLife.Services.Functions
{
    public class CreateBill
    {
        [FunctionName("CreateBill")]
        public void Run([TimerTrigger("0 0 6 * * *")]TimerInfo myTimer, ILogger log)
        {
            //https://www.notion.so/dadecook/93ffeab2ae6943acbf9e90c9df13dc03?v=3bf29229a8e1405d896b9878eea0f4a9&pvs=4
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            HttpClient httpClient = new()
            {
                BaseAddress = new Uri("https://api.notion.com/v1"),
            };

            //httpClient.DefaultRequestHeaders.Authorization = new(

            //var database = 
        }
    }
}
