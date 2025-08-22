using HexMaster.Trunq.Resolver.DomainModels;

namespace HexMaster.Trunq.Resolver.Services;

public interface IShortLinkResolverService
{
    Task<ShortLink?> ResolveAsync(string shortCode, CancellationToken cancellationToken = default);
}