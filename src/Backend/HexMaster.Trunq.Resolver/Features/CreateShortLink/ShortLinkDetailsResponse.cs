namespace HexMaster.Trunq.Resolver.Features.CreateShortLink;

public record ShortLinkDetailsResponse
{
    public required Guid Id { get; init; }
    public required string ShortCode { get; init; }
    public required string TargetUrl { get; init; }
    public required int Hits { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}