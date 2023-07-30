using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Services;
using RecordParser.Builders.Reader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace MyLife.Services.Functions
{
    public class ProcessAccountActivity
    {
        private readonly ILogger _logger;
        private readonly IBankKeywordConfigService _bankKeywordConfigService;

        public ProcessAccountActivity(ILoggerFactory loggerFactory, IBankKeywordConfigService bankKeywordConfigService)
        {
            _logger = loggerFactory.CreateLogger<ProcessAccountActivity>();
            _bankKeywordConfigService = bankKeywordConfigService;
        }

        [Function("ProcessAccountActivity")]
        [CosmosDBOutput(AccountActivityService.DatabaseId, AccountActivityService.ContainerId, Connection = "COSMOS_CONNECTION_STRING", CreateIfNotExists = true, PartitionKey = AccountActivityService.PartitionKey)]
        public async Task<IEnumerable<AccountActivityItem>> Run([BlobTrigger("account-activity/{name}", Connection = "MyLifeStorage")] string accountActivity)
        {
            var rows = accountActivity.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var reader = new VariableLengthReaderSequentialBuilder<AccountActivityItem>()
                .Map(x => x.Date, span =>
                {
                    var date = DateTime.Parse(span, null, DateTimeStyles.AllowWhiteSpaces);

                    TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    TimeSpan utcOffset = timeZone.GetUtcOffset(date);

                    return new DateTimeOffset(date, utcOffset).ToUniversalTime().DateTime;
                })
                .Map(x => x.Amount)
                .Skip(2)
                .Map(x => x.FullName)
                .Build(",");

            var items = rows.Select(row => reader.Parse(row));

            var keywords = await GetKeywords();

            items = items.Select(item =>
            {
                var (name, category) = GetNameAndCategory(item, keywords) ?? (string.Empty, "Misc (Unmapped)");

                if (string.IsNullOrEmpty(name))
                {
                    if (item.FullName.StartsWith("Purchase Authorized On", StringComparison.OrdinalIgnoreCase))
                    {
                        name = string.Join(' ', item.FullName[29..].Split(" ").Take(3));
                    }
                    else
                    {
                        name = item.FullName;
                    }
                }

                return item with { Name = name, Category = category };
            });

            return items;
        }

        private async Task<Dictionary<string, (string Name, string Category)>> GetKeywords()
        {
            var keywords = await _bankKeywordConfigService.Get();

            return keywords?.ToDictionary(x => x.Keyword, x => (x.Name, x.Category)) ?? new();
        }

        private (string Name, string Category)? GetNameAndCategory(AccountActivityItem item, Dictionary<string, (string Name, string Category)> keywords)
        {
            foreach (var (key, (name, category)) in keywords)
            {
                if (item.FullName.Contains(key, StringComparison.OrdinalIgnoreCase))
                    return (name, category);
            }

            return null;
        }
    }
}
