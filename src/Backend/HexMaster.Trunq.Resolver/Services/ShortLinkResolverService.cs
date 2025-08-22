using HexMaster.Trunq.Resolver.DomainModels;
using HexMaster.Trunq.Resolver.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HexMaster.Trunq.Resolver.Services;

public class ShortLinkResolverService : IShortLinkResolverService
{
    private readonly IShortLinkRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly IHitTrackingService _hitTrackingService;
    private readonly ILogger<ShortLinkResolverService> _logger;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public ShortLinkResolverService(
        IShortLinkRepository repository,
        IMemoryCache cache,
        IHitTrackingService hitTrackingService,
        ILogger<ShortLinkResolverService> logger)
    {
        _repository = repository;
        _cache = cache;
        _hitTrackingService = hitTrackingService;
        _logger = logger;
    }

    public async Task<ShortLink?> ResolveAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"shortlink:{shortCode}";
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out ShortLink? cachedShortLink))
        {
            _logger.LogDebug("Short link {ShortCode} retrieved from cache", shortCode);
            
            // Track hit in background
            _ = Task.Run(() => _hitTrackingService.TrackHitAsync(shortCode), CancellationToken.None);
            
            return cachedShortLink;
        }

        // Not in cache, get from repository
        var shortLink = await _repository.GetByShortCodeAsync(shortCode, cancellationToken);
        
        if (shortLink != null)
        {
            // Cache for 30 minutes
            _cache.Set(cacheKey, shortLink, CacheExpiration);
            _logger.LogDebug("Short link {ShortCode} cached for {ExpirationMinutes} minutes", shortCode, CacheExpiration.TotalMinutes);
            
            // Track hit in background
            _ = Task.Run(() => _hitTrackingService.TrackHitAsync(shortCode), CancellationToken.None);
        }

        return shortLink;
    }
}