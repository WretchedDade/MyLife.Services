using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using MyLife.Services.Shared.Extensions;
using MyLife.Services.Shared.Services;
using RecordParser.Builders.Reader;
using RecordParser.Parsers;

namespace MyLife.Services.Functions.Functions
{
    public class DownloadAccountActivity
    {
        private readonly ILogger _logger;
        private readonly IBankKeywordConfigService _bankKeywordConfigService;

        private IVariableLengthReader<AccountActivityItem> _accountActivityItemReader;

        public DownloadAccountActivity(ILoggerFactory loggerFactory, IBankKeywordConfigService bankKeywordConfigService)
        {
            _logger = loggerFactory.CreateLogger<DownloadAccountActivity>();
            _bankKeywordConfigService = bankKeywordConfigService;

            _accountActivityItemReader = new VariableLengthReaderSequentialBuilder<AccountActivityItem>()
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
        }

        [Function("DownloadAccountActivity")]
        [CosmosDBOutput(AccountActivityService.DatabaseId, AccountActivityService.ContainerId, Connection = "COSMOS_CONNECTION_STRING", CreateIfNotExists = true, PartitionKey = AccountActivityService.PartitionKey)]
        public async Task<IEnumerable<AccountActivityItem>> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var options = await req.ReadFromJsonAsync<DownloadAccountActivityOptions>() ?? new();

            var keywords = await GetKeywords();

            Microsoft.Playwright.Program.Main(new[] { "install" });

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });

            var page = await browser.NewPageAsync();

            await Login(page);

            var checking = await DownloadAccountActivityCsv(page, "Dade's Checking", options.NumberOfMonths);
            var checkingItems = MapToItems(checking, AccountName.Checking, keywords).ToList();

            await page.GetByRole(AriaRole.Link).GetByText("Account Summary", new() {  Exact = true }).ClickAsync();

            var savings = await DownloadAccountActivityCsv(page, "Savings", options.NumberOfMonths);
            var savingsItems = MapToItems(savings, AccountName.Saving, keywords).ToList();

            await page.GetByRole(AriaRole.Link).GetByText("Account Summary", new() { Exact = true }).ClickAsync();

            var credit = await DownloadAccountActivityCsv(page, "My Credit Card", options.NumberOfMonths);
            var creditItems = MapToItems(credit, AccountName.CreditCard, keywords).ToList();

            return Concatenate(checkingItems, savingsItems, creditItems);            
        }

        private async Task Login(IPage page)
        {
            await page.GotoAsync("https://www.wellsfargo.com/");

            var username = Environment.GetEnvironmentVariable(EnvironmentVariables.WellsFargoUsername);
            var password = Environment.GetEnvironmentVariable(EnvironmentVariables.WellsFargoPassword);

            await page.GetByLabel("Username", new() { Exact = true }).FillAsync(username);

            var passwordField = page.GetByLabel("Password", new() { Exact = true });
            await passwordField.FillAsync(password);
            await passwordField.PressAsync("Enter");

            if (page.Url.Contains("present?origin=cob&error=yes"))
            {
                await page.GetByTestId("input-j_username").FillAsync(username);
                await page.GetByLabel("Password", new() { Exact = true }).FillAsync(password);
            }

            await page.WaitForURLAsync("**/accounts/start**");
        }

        private async Task<string> DownloadAccountActivityCsv(IPage page, string account, int numberOfMonths)
        {
            await page.GetByRole(AriaRole.Link).GetByText(account).ClickAsync();
            await page.GetByText("Download Account Activity").ClickAsync();

            var fromDateInput = page.GetByTestId("input-fromDate");
            await fromDateInput.FillAsync("");
            await fromDateInput.FillAsync(DateTime.Today.AddMonths(-numberOfMonths).ToString("MM/dd/yyyy"));

            await page.GetByTestId("radio-fileFormat-commaDelimited").ClickAsync();

            await page.GetByText("Download", new() { Exact = true }).ClickAsync();
            var download = await page.WaitForDownloadAsync();
            using var stream = await download.CreateReadStreamAsync();
            using var reader = new StreamReader(stream);
            var csv = await reader.ReadToEndAsync();

            return csv;
        }

        private IEnumerable<AccountActivityItem> MapToItems(string csv, AccountName accountName, Dictionary<string, (string Name, string Category)> keywords)
        {
            var rows = csv.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var items = rows.Select(row => _accountActivityItemReader.Parse(row));

            foreach(var item in items)
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

                Card? card = item.FullName switch
                {
                    string details when details.Contains("CARD5084") || details.Contains("CARD 5084") => Card.DebitDade,
                    string details when details.Contains("CARD8009") || details.Contains("CARD 8009") => Card.DebitCarla,
                    _ => null
                };

                yield return item with { Name = name, Category = category, AccountName = accountName, CardUsed = card };
            };
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

        private IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }
    }

    public record DownloadAccountActivityOptions(int NumberOfMonths = 3);
}
