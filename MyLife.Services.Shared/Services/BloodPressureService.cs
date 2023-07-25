using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace MyLife.Services.Shared.Services;

public interface IBloodPressureService
{
    Task<BloodPressureReading> CreateReading(int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default);
    Task DeleteReading(string id);
    Task<BloodPressureReading> GetById(string id);
    Task<int> Count();
    Task<List<BloodPressureReading>> GetReadings(int skip, int take);
    Task<BloodPressureReading> UpdateReading(string id, int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default);
}

public class BloodPressureService : CosmosService, IBloodPressureService
{
    private const string DatabaseId = "MyLife";
    private const string ContainerId = "Blood Pressure Readings";

    public BloodPressureService(CosmosClient cosmosClient) : base(cosmosClient)
    {
    }

    public async Task<BloodPressureReading> CreateReading(int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default)
    {
        Container container = CosmosClient.GetDatabase(DatabaseId).GetContainer(ContainerId);

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
        Container container = CosmosClient.GetDatabase(DatabaseId).GetContainer(ContainerId);

        await container.DeleteItemAsync<BloodPressureReading>(id, new(id));
    }

    public async Task<BloodPressureReading> GetById(string id)
    {
        Container container = CosmosClient.GetDatabase(DatabaseId).GetContainer(ContainerId);

        return await container.ReadItemAsync<BloodPressureReading>(id, new(id));
    }

    public Task<int> Count()
    {
        Container container = CosmosClient.GetDatabase(DatabaseId).GetContainer(ContainerId);

        QueryDefinition queryDefinition = new QueryDefinition("SELECT VALUE COUNT(item.id) FROM item");

        return Count(container.GetItemQueryIterator<int>(queryDefinition));
    }

    public Task<List<BloodPressureReading>> GetReadings(int skip, int take)
    {
        Container container = CosmosClient.GetDatabase(DatabaseId).GetContainer(ContainerId);

        return ReadFeed(
            container.GetItemLinqQueryable<BloodPressureReading>()
                .OrderByDescending(r => r.TimeAtReading)
                .Skip(skip)
                .Take(take)
                .ToFeedIterator()
        );
    }

    public async Task<BloodPressureReading> UpdateReading(string id, int systolic, int diastolic, int? heartRate, DateTime timeAtReading, CancellationToken cancellationToken = default)
    {
        Container container = CosmosClient.GetDatabase(DatabaseId).GetContainer(ContainerId);

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
