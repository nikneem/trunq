using HexMaster.Trunq.Core.Cqrs;

namespace HexMaster.Trunq.Resolver.Features.GetShortLinkById;

public record GetShortLinkByIdQuery : IQuery
{
    public required Guid Id { get; init; }
    public required string SubjectId { get; init; }
}