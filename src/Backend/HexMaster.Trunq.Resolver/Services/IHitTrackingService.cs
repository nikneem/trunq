namespace HexMaster.Trunq.Resolver.Services;

public interface IHitTrackingService
{
    Task TrackHitAsync(string shortCode);
}