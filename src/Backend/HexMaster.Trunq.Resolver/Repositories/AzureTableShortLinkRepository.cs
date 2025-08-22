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

    public async Task<IEnumerable<ShortLink>> GetBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        var filter = $"SubjectId eq '{subjectId}'";
        var shortLinks = new List<ShortLink>();
        
        await foreach (var entity in _tableClient.QueryAsync<ShortLinkTableEntity>(filter, cancellationToken: cancellationToken))
        {
            shortLinks.Add(entity.ToDomainModel());
        }
        
        return shortLinks;
    }

    public async Task<ShortLink> UpdateAsync(ShortLink shortLink, CancellationToken cancellationToken = default)
    {
        // First, get the existing entity to get the current row key and ETag
        var existingEntity = await GetEntityByIdAsync(shortLink.Id, cancellationToken);
        if (existingEntity == null)
        {
            throw new InvalidOperationException($"Short link with ID '{shortLink.Id}' not found.");
        }

        var newEntity = new ShortLinkTableEntity(shortLink);
        
        // Check if the short code (row key) has changed
        if (existingEntity.RowKey != newEntity.RowKey)
        {
            // Short code changed - need to delete old entity and create new one
            await DeleteEntityAsync(existingEntity, cancellationToken);
            
            try
            {
                await _tableClient.AddEntityAsync(newEntity, cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                // New short code already exists, restore the old entity
                try
                {
                    await _tableClient.AddEntityAsync(existingEntity, cancellationToken);
                }
                catch
                {
                    // Best effort to restore, but don't mask the original error
                }
                throw new InvalidOperationException($"Short code '{shortLink.ShortCode}' already exists.");
            }
        }
        else
        {
            // Short code unchanged - simple update
            newEntity.ETag = existingEntity.ETag; // Preserve ETag for optimistic concurrency
            try
            {
                await _tableClient.UpdateEntityAsync(newEntity, newEntity.ETag, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new InvalidOperationException($"Short link with ID '{shortLink.Id}' not found.");
            }
            catch (RequestFailedException ex) when (ex.Status == 412)
            {
                throw new InvalidOperationException($"Short link with ID '{shortLink.Id}' was modified by another process. Please reload and try again.");
            }
        }

        return shortLink;
    }

    public async Task<bool> DeleteAsync(Guid id, string subjectId, CancellationToken cancellationToken = default)
    {
        // First, find the entity by ID and verify ownership
        var existingShortLink = await GetByIdAsync(id, cancellationToken);
        if (existingShortLink == null)
        {
            return false;
        }

        if (existingShortLink.SubjectId != subjectId)
        {
            throw new UnauthorizedAccessException("You can only delete your own short links.");
        }

        try
        {
            await _tableClient.DeleteEntityAsync("shortlink", existingShortLink.ShortCode, cancellationToken: cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    private async Task<ShortLinkTableEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = $"Id eq guid'{id}'";
        await foreach (var entity in _tableClient.QueryAsync<ShortLinkTableEntity>(filter, cancellationToken: cancellationToken))
        {
            return entity;
        }
        return null;
    }

    private async Task DeleteEntityAsync(ShortLinkTableEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, entity.ETag, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Entity already deleted, that's fine
        }
    }
}