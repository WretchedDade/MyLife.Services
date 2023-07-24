using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Graph.Models;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Services;

public interface IAccountActivityService
{
    Task<int> Count();
    Task<int> Count(int year, int month);
    Task Delete(string id);
    Task<List<AccountActivityItem>> Get(int pageNumber = 0, int? pageSize = null, string? category = null);
    Task<AccountActivityItem> Get(string id);
    Task<List<AccountActivityItem>> Get(int year, int month, int pageNumber = 0, int? pageSize = null);
    Task<AccountActivityItem> Update(string id, string name, string category, CancellationToken cancellationToken = default);
}

public class AccountActivityService : IAccountActivityService
{
    public const string DatabaseId = "MyLife";
    public const string ContainerId = "Account Activity";
    public const string PartitionKey = "/id";

    private readonly MyLifeCosmosSettings _settings;

    private readonly CosmosClientOptions _cosmosClientOptions = new()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    public AccountActivityService(MyLifeCosmosSettings settings) => _settings = settings;

    public async Task<int> Count()
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var count = await container.GetItemLinqQueryable<AccountActivityItem>().CountAsync();

        return count;
    }

    public async Task<int> Count(int year, int month)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        QueryDefinition queryDefinition = new QueryDefinition("SELECT VALUE COUNT(item.id) FROM item WHERE DateTimePart(\"yyyy\", item.date) = @year AND DateTimePart(\"mm\", item.date) = @month")
                .WithParameter("@year", year)
                .WithParameter("@month", month);


        var iterator = container.GetItemQueryIterator<int>(queryDefinition);

        var count = 0;
        while (iterator.HasMoreResults)
        {
            var currentResultSet = await iterator.ReadNextAsync();
            foreach (var res in currentResultSet)
            {
                count += res;
            }
        }

        return count;
    }


    public async Task Delete(string id)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        await container.DeleteItemAsync<AccountActivityItem>(id, new(id));
    }

    public async Task<AccountActivityItem> Get(string id)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        return await container.ReadItemAsync<AccountActivityItem>(id, new(id));
    }

    public async Task<List<AccountActivityItem>> Get(int pageNumber = 0, int? pageSize = null, string? category = null)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
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

        var feed = query.ToFeedIterator();

        List<AccountActivityItem> results = new();

        while (feed.HasMoreResults)
        {
            foreach (AccountActivityItem reading in await feed.ReadNextAsync())
            {
                results.Add(reading);
            }
        }

        return results;
    }

    public async Task<List<AccountActivityItem>> Get(int year, int month, int pageNumber = 0, int? pageSize = null)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var query = "SELECT * FROM item WHERE DateTimePart(\"yyyy\", item.date) = @year AND DateTimePart(\"mm\", item.date) = @month";

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

        var feed = container.GetItemQueryIterator<AccountActivityItem>(queryDefinition);

        List<AccountActivityItem> results = new();

        while (feed.HasMoreResults)
        {
            foreach (AccountActivityItem reading in await feed.ReadNextAsync())
            {
                results.Add(reading);
            }
        }

        return results;
    }

    public async Task<AccountActivityItem> Update(string id, string name, string category, CancellationToken cancellationToken = default)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
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
    public string Id => $"{Date:yyyyMMdd} {Name} ({Amount})";

    [JsonPropertyName("lastModifiedOn")]
    public DateTime LastModifiedOn => DateTime.UtcNow;

    [JsonPropertyName("hasShortName")]
    public bool HasShortName => FullName != Name;
}