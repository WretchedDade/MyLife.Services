using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace MyLife.Services.Shared.Services;

public interface IBloodPressureService
{
    Task<BloodPressureReading> CreateReading(int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default);
    Task DeleteReading(string id);
    Task<BloodPressureReading> GetById(string id);
    Task<int> GetCount();
    Task<List<BloodPressureReading>> GetReadings(int skip, int take);
    Task<BloodPressureReading> UpdateReading(string id, int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default);
}

public class BloodPressureService : IBloodPressureService
{
    private const string DatabaseId = "MyLife";
    private const string ContainerId = "Blood Pressure Readings";

    private readonly MyLifeCosmosSettings _settings;

    private readonly CosmosClientOptions _cosmosClientOptions = new()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    public BloodPressureService(MyLifeCosmosSettings settings) => _settings = settings;

    public async Task<BloodPressureReading> CreateReading(int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        BloodPressureReading reading = await container.CreateItemAsync(
            item: new BloodPressureReading(
                Id: Guid.NewGuid().ToString(),
                Systolic: systolic,
                Diastolic: diastolic,
                HeartRate: heartRate,
                TimeAtReading: timeAtReading
            ),
            cancellationToken: cancellationToken
        );

        return reading;
    }

    public async Task DeleteReading(string id)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        await container.DeleteItemAsync<BloodPressureReading>(id, new(id));
    }

    public async Task<BloodPressureReading> GetById(string id)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        return await container.ReadItemAsync<BloodPressureReading>(id, new(id));
    }

    public async Task<int> GetCount()
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var count = await container.GetItemLinqQueryable<BloodPressureReading>().CountAsync();

        return count;
    }

    public async Task<List<BloodPressureReading>> GetReadings(int skip, int take)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var feed = container.GetItemLinqQueryable<BloodPressureReading>()
            .OrderByDescending(r => r.TimeAtReading)
            .Skip(skip)
            .Take(take)
            .ToFeedIterator();

        List<BloodPressureReading> readings = new();

        while (feed.HasMoreResults)
        {
            foreach (BloodPressureReading reading in await feed.ReadNextAsync())
            {
                readings.Add(reading);
            }
        }

        return readings;
    }

    public async Task<BloodPressureReading> UpdateReading(string id, int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        BloodPressureReading reading = await container.UpsertItemAsync(
            item: new BloodPressureReading(
                Id: id,
                Systolic: systolic,
                Diastolic: diastolic,
                HeartRate: heartRate,
                TimeAtReading: timeAtReading
            ),
            new(id),
            cancellationToken: cancellationToken
        );

        return reading;
    }
}

public record BloodPressureReading(
    string Id,
    int Systolic,
    int Diastolic,
    int? HeartRate,
    DateTime TimeAtReading
);
