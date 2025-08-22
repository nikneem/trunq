using HexMaster.Trunq.Resolver.Features.CreateShortLink;

namespace HexMaster.Trunq.Resolver.Features.GetUserShortLinks;

public record GetUserShortLinksResponse
{
    public required IEnumerable<ShortLinkDetailsResponse> ShortLinks { get; init; }
}