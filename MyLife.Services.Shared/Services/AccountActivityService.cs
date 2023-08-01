using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Graph.Models;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Services;

public interface IAccountActivityService
{
    Task<int> Count();
    Task<int> Count(int year, int month);
    Task<int> Count(int year, int month, string category);
    Task Delete(string id);
    Task<List<AccountActivityItem>> Get(int pageNumber = 0, int? pageSize = null, string? category = null);
    Task<AccountActivityItem> Get(string id);
    Task<List<AccountActivityItem>> Get(int year, int month, int pageNumber = 0, int? pageSize = null);
    Task<List<AccountActivityItem>> Get(int year, int month, string category, int pageNumber = 0, int? pageSize = null);
    Task<List<AccountActivityItem>> GetExpensesOnOrAfter(DateTime dateTime);
    Task<List<AccountActivityItem>> GetIncomeOnOrAfter(DateTime dateTime);
    Task<AccountActivityItem> Update(string id, string name, string category, CancellationToken cancellationToken = default);
}

public class AccountActivityService : CosmosService, IAccountActivityService
{
    public const string DatabaseId = "MyLife";
    public const string ContainerId = "Account Activity";
    public const string PartitionKey = "/id";

    public AccountActivityService(CosmosClient cosmosClient) : base(cosmosClient)
    {
    }

    public Task<int> Count()
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        QueryDefinition queryDefinition = new QueryDefinition("SELECT VALUE COUNT(item.id) FROM item");
        return Count(container.GetItemQueryIterator<int>(queryDefinition));
    }

    public Task<int> Count(int year, int month)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        QueryDefinition queryDefinition = new QueryDefinition("SELECT VALUE COUNT(item.id) FROM item WHERE DateTimePart(\"yyyy\", item.date) = @year AND DateTimePart(\"mm\", item.date) = @month")
                .WithParameter("@year", year)
                .WithParameter("@month", month);

        return Count(container.GetItemQueryIterator<int>(queryDefinition));
    }

    public Task<int> Count(int year, int month, string category)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        QueryDefinition queryDefinition = new QueryDefinition("SELECT VALUE COUNT(item.id) FROM item WHERE DateTimePart(\"yyyy\", item.date) = @year AND DateTimePart(\"mm\", item.date) = @month AND item.category = @category")
                .WithParameter("@year", year)
                .WithParameter("@month", month)
                .WithParameter("@category", category);

        return Count(container.GetItemQueryIterator<int>(queryDefinition));
    }


    public async Task Delete(string id)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        await container.DeleteItemAsync<AccountActivityItem>(id, new(id));
    }

    public async Task<AccountActivityItem> Get(string id)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        return await container.ReadItemAsync<AccountActivityItem>(id, new(id));
    }

    public Task<List<AccountActivityItem>> Get(int pageNumber = 0, int? pageSize = null, string? category = null)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        IQueryable<AccountActivityItem> query = container.GetItemLinqQueryable<AccountActivityItem>()
            .OrderByDescending(r => r.Date);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(r => r.Category == category);
        }

        if (pageSize.HasValue)
        {
            int skip = pageNumber * pageSize.Value;
            int take = pageSize.Value;

            query = query.Skip(skip).Take(take);
        }

        return ReadFeed(query.ToFeedIterator());
    }

    public Task<List<AccountActivityItem>> Get(int year, int month, int pageNumber = 0, int? pageSize = null)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var query = "SELECT * FROM item WHERE DateTimePart(\"yyyy\", item.date) = @year AND DateTimePart(\"mm\", item.date) = @month ORDER BY item.date DESC";

        QueryDefinition queryDefinition;

        if (pageSize.HasValue)
        {
            int skip = pageNumber * pageSize.Value;
            int take = pageSize.Value;

            query += " OFFSET @skip LIMIT @take";

            queryDefinition = new QueryDefinition(query)
                .WithParameter("@year", year)
                .WithParameter("@month", month)
                .WithParameter("@skip", skip)
                .WithParameter("@take", take);
        }
        else
        {
            queryDefinition = new QueryDefinition(query)
                .WithParameter("@year", year)
                .WithParameter("@month", month);
        }

        return ReadFeed(container.GetItemQueryIterator<AccountActivityItem>(queryDefinition));
    }

    public Task<List<AccountActivityItem>> Get(int year, int month, string category, int pageNumber = 0, int? pageSize = null)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var query = @"
            SELECT * 
            FROM item 
            WHERE DateTimePart(""yyyy"", item.date) = @year 
                AND DateTimePart(""mm"", item.date) = @month 
                AND item.category = @category 
            ORDER BY item.date DESC
        ";

        QueryDefinition queryDefinition;

        if (pageSize.HasValue)
        {
            int skip = pageNumber * pageSize.Value;
            int take = pageSize.Value;

            query += " OFFSET @skip LIMIT @take";

            queryDefinition = new QueryDefinition(query)
                .WithParameter("@year", year)
                .WithParameter("@month", month)
                .WithParameter("@category", category)
                .WithParameter("@skip", skip)
                .WithParameter("@take", take);
        }
        else
        {
            queryDefinition = new QueryDefinition(query)
                .WithParameter("@year", year)
                .WithParameter("@month", month)
                .WithParameter("@category", category);
        }

        return ReadFeed(container.GetItemQueryIterator<AccountActivityItem>(queryDefinition));
    }

    public Task<List<AccountActivityItem>> GetExpensesOnOrAfter(DateTime dateTime)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM item WHERE item.date >= @date and item.amount < 0")
            .WithParameter("@date", dateTime);

        return ReadFeed(container.GetItemQueryIterator<AccountActivityItem>(queryDefinition));
    }

    public Task<List<AccountActivityItem>> GetIncomeOnOrAfter(DateTime dateTime)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM item WHERE item.date >= @date and item.amount > 0")
            .WithParameter("@date", dateTime);

        return ReadFeed(container.GetItemQueryIterator<AccountActivityItem>(queryDefinition));
    }

    public async Task<AccountActivityItem> Update(string id, string name, string category, CancellationToken cancellationToken = default)
    {
        Database database = CosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        AccountActivityItem accountActivityItem = await container.ReadItemAsync<AccountActivityItem>(id, new(id), cancellationToken: cancellationToken);

        return await container.UpsertItemAsync(
            item: accountActivityItem with { Name = name, Category = category },
            new(id),
            cancellationToken: cancellationToken
        );
    }
}

public record AccountActivityItem
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("fullName")]
    public required string FullName { get; set; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("id")]
    public string Id => $"{Date:yyyyMMdd} {AccountName} ({Amount})";

    [JsonPropertyName("lastModifiedOn")]
    public DateTime LastModifiedOn => DateTime.UtcNow;

    [JsonPropertyName("hasShortName")]
    public bool HasShortName => FullName != Name;

    [JsonPropertyName("accountName")]
    public AccountName AccountName { get; set; } = AccountName.Checking;

    [JsonPropertyName("cardUsed")]
    public Card? CardUsed { get; set; }
}

public enum AccountName
{
    Checking, Saving, CreditCard
}

public enum Card
{
    DebitDade,
    DebitCarla,
    Credit
}