using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Graph.Models;

namespace MyLife.Services.Shared.Services;

public interface IBloodPressureService
{
    Task<BloodPressureReading> CreateReading(int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default);
    Task<BloodPressureReading> GetById(string id);
    Task<List<BloodPressureReading>> GetRecentReadings(int count);
}

public class BloodPressureService : IBloodPressureService
{
    private const string DatabaseId = "MyLife";
    private const string ContainerId = "Blood Pressure Readings";

    private readonly BloodPressureSettings _settings;

    private readonly CosmosClientOptions _cosmosClientOptions = new()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    public BloodPressureService(BloodPressureSettings settings) => _settings = settings;

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

    public async Task<BloodPressureReading> GetById(string id)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        return await container.ReadItemAsync<BloodPressureReading>(id, new(id));
    }

    public async Task<List<BloodPressureReading>> GetRecentReadings(int count)
    {
        using CosmosClient cosmosClient = new(_settings.Endpoint, _settings.Key, _cosmosClientOptions);

        Database database = cosmosClient.GetDatabase(DatabaseId);
        Container container = database.GetContainer(ContainerId);

        var feed = container.GetItemLinqQueryable<BloodPressureReading>(allowSynchronousQueryExecution: true, requestOptions: new() { MaxItemCount = 1 })
            .OrderByDescending(r => r.TimeAtReading)
            .Take(count)
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
}

public record BloodPressureReading(
    string Id,
    int Systolic,
    int Diastolic,
    int? HeartRate,
    DateTime TimeAtReading
);

public class BloodPressureSettings
{
    public required string Key { get; set; }
    public required string Endpoint { get; set; }
}