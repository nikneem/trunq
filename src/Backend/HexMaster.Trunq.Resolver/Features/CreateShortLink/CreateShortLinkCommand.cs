using HexMaster.Trunq.Core.Cqrs;

namespace HexMaster.Trunq.Resolver.Features.CreateShortLink;


public record CreateShortLinkCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public required string TargetUrl { get; init; }
    public required string SubjectId { get; init; }
}