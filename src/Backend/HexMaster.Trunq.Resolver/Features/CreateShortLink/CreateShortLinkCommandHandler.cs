using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.DomainModels;
using HexMaster.Trunq.Resolver.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Trunq.Resolver.Features.CreateShortLink;

public class CreateShortLinkCommandHandler(
    IShortLinkRepository repository,
    ILogger<CreateShortLinkCommandHandler> logger)
    : ICommandHandler<CreateShortLinkCommand, ShortLinkDetailsResponse>
{
    public async ValueTask<ShortLinkDetailsResponse> HandleAsync(CreateShortLinkCommand command, CancellationToken cancellationToken)
    {
        // Validate the target URL
        if (!Uri.TryCreate(command.TargetUrl, UriKind.Absolute, out var uri) || 
            (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            throw new ArgumentException("Target URL must be a valid HTTP or HTTPS URL.", nameof(command.TargetUrl));
        }

        // Attempt to create unique short link with collision detection
        const int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // Create a new short link (generates new short code each time)
            var shortLink = ShortLink.Create(command.TargetUrl, command.SubjectId);

            // Check if this short code already exists
            var existingShortLink = await repository.GetByShortCodeAsync(shortLink.ShortCode, cancellationToken);
            
            if (existingShortLink == null)
            {
                // Short code is unique, create and store it
                var createdShortLink = await repository.CreateAsync(shortLink, cancellationToken);
                logger.LogInformation("Short link {ShortCode} created for target URL", createdShortLink.ShortCode);
                
                // Map domain model to response record
                return MapToResponse(createdShortLink);
            }

            // Short code collision detected, will retry with a new domain model on next iteration
            logger.LogDebug("Short code collision detected for {ShortCode}, retrying... (attempt {Attempt})", 
                shortLink.ShortCode, attempt + 1);
        }

        throw new InvalidOperationException("Failed to generate unique short code after multiple attempts.");
    }

    private static ShortLinkDetailsResponse MapToResponse(ShortLink shortLink)
    {
        return new ShortLinkDetailsResponse
        {
            Id = shortLink.Id,
            ShortCode = shortLink.ShortCode,
            TargetUrl = shortLink.TargetUrl,
            Hits = shortLink.Hits ?? 0, // Assuming Hits is nullable, default to 0
            CreatedAt = shortLink.CreatedAt
        };
    }
}