using Azure;
using Azure.Data.Tables;
using HexMaster.Trunq.Resolver.DomainModels;

namespace HexMaster.Trunq.Resolver.Repositories;

public class AzureTableShortLinkRepository : IShortLinkRepository
{
    private readonly TableClient _tableClient;

    public AzureTableShortLinkRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient("shortlinks");
        // Create table if it doesn't exist (Aspire/Azurite handles this gracefully)
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task<ShortLink> CreateAsync(ShortLink shortLink, CancellationToken cancellationToken = default)
    {
        var entity = new ShortLinkTableEntity(shortLink);
        
        try
        {
            await _tableClient.AddEntityAsync(entity, cancellationToken);
            return shortLink;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            // Entity already exists - handle collision
            throw new InvalidOperationException($"Short code '{shortLink.ShortCode}' already exists.");
        }
    }

    public async Task<ShortLink?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<ShortLinkTableEntity>("shortlink", shortCode, cancellationToken: cancellationToken);
            return response.Value.ToDomainModel();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<ShortLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = $"Id eq guid'{id}'";
        await foreach (var entity in _tableClient.QueryAsync<ShortLinkTableEntity>(filter, cancellationToken: cancellationToken))
        {
            return entity.ToDomainModel();
        }
        return null;
    }
}