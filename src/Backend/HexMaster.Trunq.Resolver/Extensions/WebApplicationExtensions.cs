using System.Security.Claims;
using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HexMaster.Trunq.Resolver.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapShortLinkEndpoints(this WebApplication app)
    {
        app.MapPost("/api/links", async (
            CreateLinkRequest request, 
            ICommandHandler<CreateShortLinkCommand, ShortLinkDetailsResponse> handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            // Get the subject ID from the JWT token
            var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? user.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(subjectId))
            {
                return Results.Unauthorized();
            }

            // Create command
            var command = new CreateShortLinkCommand
            {
                TargetUrl = request.TargetUrl,
                SubjectId = subjectId
            };

            try
            {
                // Handle command
                var shortLinkDetails = await handler.HandleAsync(command, cancellationToken);

                // Return response
                var response = new CreateLinkResponse
                {
                    Id = shortLinkDetails.Id,
                    ShortCode = shortLinkDetails.ShortCode,
                    TargetUrl = shortLinkDetails.TargetUrl,
                    CreatedAt = shortLinkDetails.CreatedAt
                };

                return Results.Created($"/api/links/{shortLinkDetails.ShortCode}", response);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            }
        })
        .RequireAuthorization()
        .WithName("CreateShortLink")
        .WithOpenApi();

        return app;
    }
}