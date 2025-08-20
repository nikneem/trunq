using System.Security.Claims;
using Azure.Data.Tables;
using HexMaster.Trunq.Api.Models;
using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/trunq";
        options.Audience = "account";
        options.RequireHttpsMetadata = false; // Only for development
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, // Keycloak may not always include audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// Add Azure Table Storage
builder.Services.AddSingleton<TableServiceClient>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureTableStorage") 
                          ?? "UseDevelopmentStorage=true"; // Default to Azurite for local development
    return new TableServiceClient(connectionString);
});

// Register repositories and handlers
builder.Services.AddScoped<IShortLinkRepository, AzureTableShortLinkRepository>();
builder.Services.AddScoped<ICommandHandler<CreateShortLinkCommand, ShortLinkDetailsResponse>, CreateShortLinkCommandHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// API endpoints
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

app.Run();
