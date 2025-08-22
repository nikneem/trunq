using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Trunq.Resolver.Features.DeleteShortLink;

public class DeleteShortLinkCommandHandler(
    IShortLinkRepository repository,
    ILogger<DeleteShortLinkCommandHandler> logger)
    : ICommandHandler<DeleteShortLinkCommand, bool>
{
    public async ValueTask<bool> HandleAsync(DeleteShortLinkCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await repository.DeleteAsync(command.Id, command.SubjectId, cancellationToken);
            
            if (deleted)
            {
                logger.LogInformation("Short link {Id} deleted by user {SubjectId}", command.Id, command.SubjectId);
            }
            else
            {
                logger.LogWarning("Short link {Id} not found for deletion by user {SubjectId}", command.Id, command.SubjectId);
            }
            
            return deleted;
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("User {SubjectId} attempted to delete short link {Id} they don't own", command.SubjectId, command.Id);
            throw;
        }
    }
}