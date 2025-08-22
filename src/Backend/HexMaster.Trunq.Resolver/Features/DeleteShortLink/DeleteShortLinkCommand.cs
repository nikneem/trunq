using HexMaster.Trunq.Core.Cqrs;

namespace HexMaster.Trunq.Resolver.Features.DeleteShortLink;

public record DeleteShortLinkCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public required Guid Id { get; init; }
    public required string SubjectId { get; init; }
}