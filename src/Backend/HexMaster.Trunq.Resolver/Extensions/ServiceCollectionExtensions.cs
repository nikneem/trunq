using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Configuration;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Features.DeleteShortLink;
using HexMaster.Trunq.Resolver.Features.GetShortLinkById;
using HexMaster.Trunq.Resolver.Features.GetUserShortLinks;
using HexMaster.Trunq.Resolver.Features.UpdateShortLink;
using HexMaster.Trunq.Resolver.Repositories;
using HexMaster.Trunq.Resolver.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Trunq.Resolver.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShortLinkServices(this IServiceCollection services)
    {
        // Register configuration
        services.AddOptions<TrunqOptions>()
            .BindConfiguration(TrunqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
            
        // Add memory cache
        services.AddMemoryCache();
        
        // Register repositories
        services.AddScoped<IShortLinkRepository, AzureTableShortLinkRepository>();
        
        // Register services
        services.AddScoped<IUrlBuilderService, UrlBuilderService>();
        services.AddScoped<IShortLinkResolverService, ShortLinkResolverService>();
        services.AddSingleton<IHitTrackingService, HitTrackingService>();
        
        // Register the hit tracking service as a hosted service
        services.AddHostedService<HitTrackingService>(serviceProvider =>
            (HitTrackingService)serviceProvider.GetRequiredService<IHitTrackingService>());
        
        // Register command handlers
        services.AddScoped<ICommandHandler<CreateShortLinkCommand, ShortLinkDetailsResponse>, CreateShortLinkCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateShortLinkCommand, ShortLinkDetailsResponse>, UpdateShortLinkCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteShortLinkCommand, bool>, DeleteShortLinkCommandHandler>();
        
        // Register query handlers
        services.AddScoped<IQueryHandler<GetShortLinkByIdQuery, ShortLinkDetailsResponse>, GetShortLinkByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserShortLinksQuery, GetUserShortLinksResponse>, GetUserShortLinksQueryHandler>();
        
        return services;
    }
}