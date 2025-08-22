using HexMaster.Trunq.Core.Cqrs;

namespace HexMaster.Trunq.Resolver.Features.UpdateShortLink;

public record UpdateShortLinkCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public required Guid Id { get; init; }
    public required string TargetUrl { get; init; }
    public string? ShortCode { get; init; }
    public required string SubjectId { get; init; }
}