using HexMaster.Trunq.Core.Cqrs;

namespace HexMaster.Trunq.Resolver.Features.GetUserShortLinks;

public record GetUserShortLinksQuery : IQuery
{
    public required string SubjectId { get; init; }
}