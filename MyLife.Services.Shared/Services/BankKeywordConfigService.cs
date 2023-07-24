using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Services;

public interface IBankKeywordConfigService
{
    Task<int> Count();
    Task<BankKeyword> Create(string keyword, string name, string category, CancellationToken cancellationToken = default);
    Task Delete(string keyword);
    Task<List<BankKeyword>> Get(int pageNumber = 0, int? pageSize = null);
    Task<BankKeyword> Get(string keyword);
    Task<BankKeyword> Update(string keyword, string name, string category, CancellationToken cancellationToken = default);
}

public class BankKeywordConfigService : IBankKeywordConfigService
{
    public const string DatabaseId = "MyLife";
    public const string ContainerId = "Bank Keyword Config";
    public const string PartitionKey = "/id";

    private readonly MyLifeCosmosSettings _settings;

    private readonly CosmosClientOptions _cosmosClientOptions = new()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    public BankKeywordConfigService(MyLifeCosmosSettings settings) => _settings = settings;

    public async Task<int> Count()
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var count = await container.GetItemLinqQueryable<BankKeyword>().CountAsync();

        return count;
    }

    public async Task<BankKeyword> Create(string keyword, string name, string category, CancellationToken cancellationToken = default)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        BankKeyword reading = await container.CreateItemAsync(
            item: new BankKeyword(keyword, name, category),
            cancellationToken: cancellationToken
        );

        return reading;
    }

    public async Task Delete(string keyword)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        await container.DeleteItemAsync<BankKeyword>(keyword, new(keyword));
    }

    public async Task<BankKeyword> Get(string keyword)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        return await container.ReadItemAsync<BankKeyword>(keyword, new(keyword));
    }

    public async Task<List<BankKeyword>> Get(int pageNumber = 0, int? pageSize = null)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        IQueryable<BankKeyword> query = container.GetItemLinqQueryable<BankKeyword>()
            .OrderByDescending(r => r.LastModifiedOn);

        if (pageSize.HasValue)
        {
            int skip = pageNumber * pageSize.Value;
            int take = pageSize.Value;

            query = query.Skip(skip).Take(take);
        }

        var feed = query.ToFeedIterator();

        List<BankKeyword> results = new();

        while (feed.HasMoreResults)
        {
            foreach (BankKeyword reading in await feed.ReadNextAsync())
            {
                results.Add(reading);
            }
        }

        return results;
    }

    public async Task<BankKeyword> Update(string keyword, string name, string category, CancellationToken cancellationToken = default)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        return await container.UpsertItemAsync(
            item: new BankKeyword(keyword, name, category),
            new(keyword),
            cancellationToken: cancellationToken
        );
    }
}



public record BankKeyword
{
    public BankKeyword(string keyword, string name, string category)
    {
        Keyword = keyword;
        Name = name;
        Category = category;
    }

    [JsonPropertyName("keyword")]
    public string Keyword { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("id")]
    public string Id => Keyword;

    public DateTimeOffset LastModifiedOn => DateTimeOffset.UtcNow;
}