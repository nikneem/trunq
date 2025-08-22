using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Repositories;
using HexMaster.Trunq.Resolver.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Trunq.Resolver.Features.GetUserShortLinks;

public class GetUserShortLinksQueryHandler(
    IShortLinkRepository repository,
    IUrlBuilderService urlBuilder,
    ILogger<GetUserShortLinksQueryHandler> logger)
    : IQueryHandler<GetUserShortLinksQuery, GetUserShortLinksResponse>
{
    public async ValueTask<GetUserShortLinksResponse> HandleAsync(GetUserShortLinksQuery query, CancellationToken cancellationToken)
    {
        var shortLinks = await repository.GetBySubjectIdAsync(query.SubjectId, cancellationToken);
        
        logger.LogInformation("Retrieved {Count} short links for user {SubjectId}", shortLinks.Count(), query.SubjectId);

        var shortLinkDetails = shortLinks.Select(shortLink => new ShortLinkDetailsResponse
        {
            Id = shortLink.Id,
            ShortCode = shortLink.ShortCode,
            TargetUrl = shortLink.TargetUrl,
            ShortUrl = urlBuilder.BuildShortUrl(shortLink.ShortCode),
            Hits = shortLink.Hits ?? 0,
            CreatedAt = shortLink.CreatedAt
        }).OrderByDescending(x => x.CreatedAt);

        return new GetUserShortLinksResponse
        {
            ShortLinks = shortLinkDetails
        };
    }
}