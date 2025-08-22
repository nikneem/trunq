using System.Security.Claims;
using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Features.GetShortLinkById;
using HexMaster.Trunq.Resolver.Features.GetUserShortLinks;
using HexMaster.Trunq.Resolver.Features.UpdateShortLink;
using HexMaster.Trunq.Resolver.Features.DeleteShortLink;
using HexMaster.Trunq.Resolver.Models;
using HexMaster.Trunq.Resolver.Services;
using HexMaster.Trunq.Resolver.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HexMaster.Trunq.Resolver.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapShortLinkEndpoints(this WebApplication app)
    {
        // PUBLIC REDIRECT - GET /{shortCode} (no authentication required)
        app.MapGet("/{shortCode}", async (
            string shortCode,
            IShortLinkResolverService resolverService,
            CancellationToken cancellationToken) =>
        {
            // Validate short code format using centralized validator
            if (!ShortCodeValidator.IsValid(shortCode))
            {
                return Results.BadRequest(new { error = ShortCodeValidator.ErrorMessage });
            }

            var shortLink = await resolverService.ResolveAsync(shortCode, cancellationToken);
            
            if (shortLink == null)
            {
                return Results.NotFound();
            }

            return Results.Redirect(shortLink.TargetUrl, permanent: false);
        })
        .WithName("ResolveShortLink")
        .WithOpenApi();

        var linksGroup = app.MapGroup("/api/links")
            .RequireAuthorization()
            .WithOpenApi();

        // CREATE - POST /api/links
        linksGroup.MapPost("", async (
            CreateLinkRequest request, 
            ICommandHandler<CreateShortLinkCommand, ShortLinkDetailsResponse> handler,
            IUrlBuilderService urlBuilder,
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
                    ShortUrl = urlBuilder.BuildShortUrl(shortLinkDetails.ShortCode),
                    CreatedAt = shortLinkDetails.CreatedAt
                };

                return Results.Created($"/api/links/{shortLinkDetails.Id}", response);
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
        .WithName("CreateShortLink");

        // READ - GET /api/links (get all user's links)
        linksGroup.MapGet("", async (
            IQueryHandler<GetUserShortLinksQuery, GetUserShortLinksResponse> handler,
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

            var query = new GetUserShortLinksQuery { SubjectId = subjectId };
            var result = await handler.HandleAsync(query, cancellationToken);

            return Results.Ok(result.ShortLinks);
        })
        .WithName("GetUserShortLinks");

        // READ - GET /api/links/{id} (get single link by ID)
        linksGroup.MapGet("{id:guid}", async (
            Guid id,
            IQueryHandler<GetShortLinkByIdQuery, ShortLinkDetailsResponse> handler,
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

            try
            {
                var query = new GetShortLinkByIdQuery { Id = id, SubjectId = subjectId };
                var result = await handler.HandleAsync(query, cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = $"Short link with ID '{id}' not found." });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithName("GetShortLinkById");

        // UPDATE - PUT /api/links/{id}
        linksGroup.MapPut("{id:guid}", async (
            Guid id,
            UpdateLinkRequest request,
            ICommandHandler<UpdateShortLinkCommand, ShortLinkDetailsResponse> handler,
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

            var command = new UpdateShortLinkCommand
            {
                Id = id,
                TargetUrl = request.TargetUrl,
                ShortCode = request.ShortCode,
                SubjectId = subjectId
            };

            try
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = $"Short link with ID '{id}' not found." });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateShortLink");

        // DELETE - DELETE /api/links/{id}
        linksGroup.MapDelete("{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteShortLinkCommand, bool> handler,
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

            var command = new DeleteShortLinkCommand
            {
                Id = id,
                SubjectId = subjectId
            };

            try
            {
                var deleted = await handler.HandleAsync(command, cancellationToken);
                if (deleted)
                {
                    return Results.NoContent();
                }
                else
                {
                    return Results.NotFound(new { error = $"Short link with ID '{id}' not found." });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithName("DeleteShortLink");

        return app;
    }
}