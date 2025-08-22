using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using HexMaster.Trunq.Resolver.Repositories;

namespace HexMaster.Trunq.Resolver.Services;

public class HitTrackingService : BackgroundService, IHitTrackingService
{
    private readonly ConcurrentQueue<string> _hitQueue = new();
    private readonly ConcurrentDictionary<string, int> _hitCounts = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HitTrackingService> _logger;
    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromSeconds(30);

    public HitTrackingService(IServiceProvider serviceProvider, ILogger<HitTrackingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task TrackHitAsync(string shortCode)
    {
        _hitQueue.Enqueue(shortCode);
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hit tracking service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessHitsAsync(stoppingToken);
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Hit tracking service stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing hits in background service");
                await Task.Delay(ProcessingInterval, stoppingToken);
            }
        }
    }

    private async Task ProcessHitsAsync(CancellationToken cancellationToken)
    {
        // Process all queued hits
        while (_hitQueue.TryDequeue(out var shortCode))
        {
            _hitCounts.AddOrUpdate(shortCode, 1, (key, count) => count + 1);
        }

        if (_hitCounts.IsEmpty)
            return;

        // Create a copy and clear the dictionary atomically
        var hitsToProcess = new Dictionary<string, int>();
        foreach (var kvp in _hitCounts)
        {
            hitsToProcess[kvp.Key] = kvp.Value;
        }
        _hitCounts.Clear();

        // Update hit counts in batches
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IShortLinkRepository>();

        foreach (var hit in hitsToProcess)
        {
            try
            {
                await UpdateHitCountAsync(repository, hit.Key, hit.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update hit count for short code {ShortCode}", hit.Key);
                // Re-queue failed hits (with a simple retry mechanism)
                for (int i = 0; i < hit.Value; i++)
                {
                    _hitQueue.Enqueue(hit.Key);
                }
            }
        }

        if (hitsToProcess.Count > 0)
        {
            _logger.LogDebug("Processed {HitCount} hit updates for {ShortCodeCount} short codes", 
                hitsToProcess.Values.Sum(), hitsToProcess.Count);
        }
    }

    private async Task UpdateHitCountAsync(IShortLinkRepository repository, string shortCode, int hitCount, CancellationToken cancellationToken)
    {
        var shortLink = await repository.GetByShortCodeAsync(shortCode, cancellationToken);
        if (shortLink != null)
        {
            // Update hits using the domain method
            shortLink.IncrementHits(hitCount);
            
            await repository.UpdateAsync(shortLink, cancellationToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hit tracking service stopping, processing remaining hits");
        
        // Process any remaining hits before stopping
        await ProcessHitsAsync(cancellationToken);
        
        await base.StopAsync(cancellationToken);
    }
}