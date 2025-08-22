using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Trunq.Resolver.Features.GetShortLinkById;

public class GetShortLinkByIdQueryHandler(
    IShortLinkRepository repository,
    ILogger<GetShortLinkByIdQueryHandler> logger)
    : IQueryHandler<GetShortLinkByIdQuery, ShortLinkDetailsResponse>
{
    public async ValueTask<ShortLinkDetailsResponse> HandleAsync(GetShortLinkByIdQuery query, CancellationToken cancellationToken)
    {
        var shortLink = await repository.GetByIdAsync(query.Id, cancellationToken);
        
        if (shortLink == null)
        {
            throw new KeyNotFoundException($"Short link with ID '{query.Id}' not found.");
        }

        // Verify ownership
        if (shortLink.SubjectId != query.SubjectId)
        {
            throw new UnauthorizedAccessException("You can only access your own short links.");
        }

        logger.LogInformation("Retrieved short link {Id} for user {SubjectId}", query.Id, query.SubjectId);

        return new ShortLinkDetailsResponse
        {
            Id = shortLink.Id,
            ShortCode = shortLink.ShortCode,
            TargetUrl = shortLink.TargetUrl,
            Hits = shortLink.Hits ?? 0,
            CreatedAt = shortLink.CreatedAt
        };
    }
}