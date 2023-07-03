using System;
using System.IO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MyLife.Services.Functions
{
    public class UploadAccountActivity
    {
        private readonly ILogger _logger;

        public UploadAccountActivity(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UploadAccountActivity>();
        }

        [Function("UploadAccountActivity")]
        public void Run([BlobTrigger("account-activity/{name}", Connection = "StorageAccountConnectionString")] string myBlob, string name)
        {
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {myBlob}");
        }

        private class Transaction
        {
            public Transaction(string csvRow)
            {
                var values = csvRow.Split(',');
                Date = DateOnly.Parse(values[0]);
                Amount = decimal.Parse(values[1]);
                Name = values[4];
            }

            public string Name { get; set; }
            public DateOnly Date { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
