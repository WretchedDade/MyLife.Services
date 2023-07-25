using Microsoft.Azure.Cosmos;


namespace MyLife.Services.Shared.Services;
public class CosmosService
{
    protected CosmosClient CosmosClient;

    public CosmosService(CosmosClient cosmosClient) => CosmosClient = cosmosClient;

    protected async Task<List<T>> ReadFeed<T>(FeedIterator<T>? feedIterator)
    {
        ArgumentNullException.ThrowIfNull(feedIterator);

        List<T> items = new();

        while (feedIterator.HasMoreResults)
        {
            foreach (var item in await feedIterator.ReadNextAsync())
            {
                items.Add(item);
            }
        }

        return items;
    }

    protected async Task<int> Count(FeedIterator<int>? feedIterator)
    {
        ArgumentNullException.ThrowIfNull(feedIterator);

        var results = await ReadFeed(feedIterator);

        return results.Sum();
    }
}
