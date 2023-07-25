using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.Functions
{
    public class UploadManualKeywords
    {
        private readonly ILogger _logger;
        private readonly INotionAPI _notionAPI;

        public UploadManualKeywords(ILoggerFactory loggerFactory, INotionAPI notionAPI)
        {
            _logger = loggerFactory.CreateLogger<UploadManualKeywords>();
            _notionAPI = notionAPI;
        }

        [Function("UploadManualKeywords")]
        [CosmosDBOutput(BankKeywordConfigService.DatabaseId, BankKeywordConfigService.ContainerId, Connection = "COSMOS_CONNECTION_STRING", CreateIfNotExists = true, PartitionKey = BankKeywordConfigService.PartitionKey)]
        public async Task<IEnumerable<BankKeyword>> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            var keywords = await GetKeywords();
            return keywords.Select(keyword => new BankKeyword(keyword.Key, keyword.Value.Name, keyword.Value.Category));
        }

        private Dictionary<string, (string Name, string Category)> ManualKeywords = new()
        {
            { "McDonalds", ("McDonald's", "Eating Out") },
            { "McDonald's", ("McDonald's", "Eating Out") },
            { "Dunkin", ("Dunkin Donuts", "Eating Out") },
            { "FIREHOUSE", ("Firehouse Subs", "Eating Out") },
            { "Sonic Drive In", ("Sonic Drive-In", "Eating Out") },
            { "Chili's", ("Chili's", "Eating Out") },
            { "Starbucks", ("Starbucks Coffee", "Eating Out") },
            { "Chick-Fil-A", ("Chick-Fil-A", "Eating Out") },
            { "Diablos", ("Diablo's Southwest", "Eating Out") },
            { "THAT FLIPPIN EGG", ("That Flippin' Egg", "Eating Out") },
            { "SHANES RIB", ("Shane's Rib Shack", "Eating Out") },
            { "SHANE'S RIB", ("Shane's Rib Shack", "Eating Out") },
            { "OCHARLEYS", ("O'Charleys", "Eating Out") },
            { "WINGSTOP", ("Wing Stop", "Eating Out") },
            { "MI PUEBLO", ("Mi Pueblo", "Eating Out") },
            { "Chicken Finge", ("Chicken Fingers", "Eating Out") },
            { "Taco Bell", ("Taco Bell", "Eating Out") },
            { "Papa John's", ("Papa John's", "Eating Out") },
            { "Wendys", ("Wendy's", "Eating Out") },
            { "Wendy's", ("Wendy's", "Eating Out") },
            { "ZAXBYS", ("Zaxby's", "Eating Out") },
            { "GENGHIS GRILL", ("Genghis Grill", "Eating Out") },
            { "MARCOS PIZZA", ("Marco's Pizza", "Eating Out") },
            { "Metro Diner", ("Metro Diner", "Eating Out") },
            { "Texas Roadhouse", ("Texas Roadhouse", "Eating Out") },
            { "BRUSTERS", ("Bruster's Ice Cream", "Eating Out") },
            { "HOME DEPOT", ("The Home Depot", "Eating Out") },
            { "PANERA BREAD", ("Panera Bread", "Eating Out") },
            { "SLIM CHICKENS", ("Slim Chicken's", "Eating Out") },
            { "DAIRY QUEEN", ("Dairy Queen", "Eating Out") },
            { "Wing Place", ("Wing Place", "Eating Out") },
            { "WINGPLACE", ("Wing Place", "Eating Out") },
            { "RUSHS", ("Rush's", "Eating Out") },
            { "RUSH'S", ("Rush's", "Eating Out") },
            { "CRACKER BARREL", ("Cracker Barrel", "Eating Out") },
            { "JERSEY MIKES", ("Jersey Mike's", "Eating Out") },
            { "SMALLCAKES", ("Small Cake's", "Eating Out") },
            { "BOJANGLES", ("Bojangles", "Eating Out") },
            { "MONTERREY", ("Monterrey's", "Eating Out") },
            { "FIVE GUYS", ("Five Guy's", "Eating Out") },
            { "5GUYS", ("Five Guy's", "Eating Out") },
            { "CHICK-N-SNACK", ("Chick N' Snack", "Eating Out") },
            { "ARBY'S", ("Arby's", "Eating Out") },
            { "COLDSTONE", ("Coldstone Creamery", "Eating Out") },
            { "CRUMBL", ("Crumbl Cookie", "Eating Out") },
            { "MELLOW MUSHROOM", ("Mellow Mushroom", "Eating Out") },
            { "NOTHING BUNDT", ("Nothing Bundt Cakes", "Eating Out") },
            { "SUKIYA", ("Sukiya Japanese", "Eating Out") },
            { "HARDEES", ("Hardee's", "Eating Out") },
            { "Olive Garden", ("Olive Garden", "Eating Out") },
            { "Groucho's Deli", ("Groucho's Deli", "Eating Out") },
            { "LONGHORN", ("Longhorn Steakhouse", "Eating Out") },

            { "Bowlero", ("Bowlero", "Entertainment") },
            { "B&N Membership", ("Barnes and Noble Membership", "Entertainment") },
            { "ABCMOUSE.COM", ("ABC Mouse", "Entertainment") },
            { "RIVERWATCH CINEMAS", ("Riverwatch Cinemas", "Entertainment") },
            { "D&B", ("Dave & Buster's", "Entertainment") },
            { "DAVE & BUSTER'S", ("Dave & Buster's", "Entertainment") },
            { "RIVERBANKS ZOO", ("Riverbank's Zoo", "Entertainment") },

            { "Target", ("Target", "Shopping") },
            { "Hobby", ("Hobby Lobby", "Shopping") },
            { "GOODWILL", ("Goodwill", "Shopping") },
            { "Amazon.com", ("Amazon", "Shopping") },
            { "AMZN Mktp", ("Amazon", "Shopping") },
            { "PUBLIX", ("Publix", "Shopping") },
            { "TEMU.COM", ("Temu", "Shopping") },
            { "DOLLAR TR", ("Dollar Tree", "Shopping") },
            { "DOLLARTRE", ("Dollar Tree", "Shopping") },
            { "MICHAELS", ("Michaels", "Shopping") },
            { "LOWE'S", ("Lowe's", "Shopping") },
            { "Etsy.com", ("Etsy", "Shopping") },
            { "SARAS FRESH MARKET", ("Sara's Fresh Market", "Shopping") },
            { "VENMO PAYMENT", ("Venmo Payment", "Shopping") },
            { "DOLLAR-GENERAL", ("Dollar General", "Shopping") },
            { "OLD NAVY US", ("Old Navy", "Shopping") },
            { "BATH & BODY", ("Bath & Body Works", "Shopping") },
            { "RACK ROOM SHOES", ("Rack Room Shoes", "Shopping") },
            { "ShopDisney.com", ("Shop Disney", "Shopping") },
            { "EBAY", ("eBay", "Shopping") },
            { "2ND AND CHARLES", ("2nd & Charles", "Shopping") },

            { "CVS", ("CVS Pharmacy", "Medical") },
            { "PATIENTPMT", ("Patient Payment (Carla)", "Medical") },

            { "MICROSOFT EDIPAYMENT", ("Microsoft Paycheck", "Income") },
            { "PANGOBOOK", ("Pango Book", "Income") },
            { "VENMO CASHOUT", ("Venmo Cashout", "Income") },

            { "QT 1196", ("Quick Trip", "Gas") },
            { "PILOT", ("Pilot", "Gas") },

            { "WALMART.COM", ("Walmart", "Groceries") },
            { "ALDI", ("ALDI", "Groceries") },

            { "ATM WITHDRAWAL", ("ATM Withdrawal", "Cash Out") },

            { "CASH APP", ("Cash App", "Misc") },
            { "MONEY TRANSFER", ("Money Transfer", "Misc") },
            { "ONLINE TRANSFER", ("Online Transfer", "Misc") },
            { "WELLS FARGO REWARDS", ("Wells Fargo Rewards", "Misc") },
            { "MOBILE DEPOSIT", ("Mobile Check Deposit", "Misc") },
            { "APPLE.COM/BILL", ("Apple ID Subscription", "Misc") },
            { "MICROSOFT", ("Microsoft", "Misc") },
            { "SUNDAY LAWN", ("Sunday Lawn & Garden", "Misc") },
            { "TAX", ("Taxes", "Misc") },

            { "WDW", ("Walt Disney World", "Vacation") }
        };

        private async Task<Dictionary<string, (string Name, string Category)>> GetKeywords()
        {
            var billConfigurationDatabaseId = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionBillConfigurationDatabaseId);

            Dictionary<string, (string Name, string Category)> keywords = ManualKeywords;

            var billConfigurationPages = await _notionAPI.QueryDatabase<NotionPage>(billConfigurationDatabaseId);

            foreach (var billConfigurationPage in billConfigurationPages)
            {
                var billKeywords = billConfigurationPage.GetProperty("Account Activity Keywords")?.RichText?.FirstOrDefault()?.PlainText ?? "";

                foreach (var keyword in billKeywords.Split(", "))
                {
                    var trimmedKeyword = keyword.Trim();

                    if (!string.IsNullOrEmpty(trimmedKeyword) && !keywords.ContainsKey(trimmedKeyword))
                        keywords.Add(trimmedKeyword, (billConfigurationPage.Name, "Bill"));
                }
            }

            return keywords;
        }
    }
}
