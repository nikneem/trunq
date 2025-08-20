using HexMaster.Trunq.Resolver.DomainModels;

namespace HexMaster.Trunq.Resolver.Repositories;

public interface IShortLinkRepository
{
    Task<ShortLink> CreateAsync(ShortLink shortLink, CancellationToken cancellationToken = default);
    Task<ShortLink?> GetByShortCodeAsync(string shortCode, CancellationToken cancellationToken = default);
    Task<ShortLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}