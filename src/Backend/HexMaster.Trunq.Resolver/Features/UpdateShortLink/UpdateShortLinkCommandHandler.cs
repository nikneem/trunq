using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Trunq.Resolver.Features.UpdateShortLink;

public class UpdateShortLinkCommandHandler(
    IShortLinkRepository repository,
    ILogger<UpdateShortLinkCommandHandler> logger)
    : ICommandHandler<UpdateShortLinkCommand, ShortLinkDetailsResponse>
{
    public async ValueTask<ShortLinkDetailsResponse> HandleAsync(UpdateShortLinkCommand command, CancellationToken cancellationToken)
    {
        // Get the existing short link
        var existingShortLink = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (existingShortLink == null)
        {
            throw new KeyNotFoundException($"Short link with ID '{command.Id}' not found.");
        }

        // Verify ownership
        if (existingShortLink.SubjectId != command.SubjectId)
        {
            throw new UnauthorizedAccessException("You can only update your own short links.");
        }

        // Update the target URL (validation happens in domain model)
        existingShortLink.UpdateTargetUrl(command.TargetUrl);
        
        // Update short code if provided
        if (!string.IsNullOrWhiteSpace(command.ShortCode))
        {
            // Check if the new short code is already taken by another link
            var existingWithSameCode = await repository.GetByShortCodeAsync(command.ShortCode, cancellationToken);
            if (existingWithSameCode != null && existingWithSameCode.Id != command.Id)
            {
                throw new InvalidOperationException($"Short code '{command.ShortCode}' is already taken.");
            }
            
            existingShortLink.SetShortCode(command.ShortCode);
        }

        // Save the updated short link
        var updatedShortLink = await repository.UpdateAsync(existingShortLink, cancellationToken);
        
        logger.LogInformation("Short link {Id} updated by user {SubjectId}", command.Id, command.SubjectId);

        return new ShortLinkDetailsResponse
        {
            Id = updatedShortLink.Id,
            ShortCode = updatedShortLink.ShortCode,
            TargetUrl = updatedShortLink.TargetUrl,
            Hits = updatedShortLink.Hits ?? 0,
            CreatedAt = updatedShortLink.CreatedAt
        };
    }
}