using HexMaster.Trunq.Core.Cqrs;
using HexMaster.Trunq.Resolver.Features.CreateShortLink;
using HexMaster.Trunq.Resolver.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Trunq.Resolver.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShortLinkServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IShortLinkRepository, AzureTableShortLinkRepository>();
        
        // Register command handlers
        services.AddScoped<ICommandHandler<CreateShortLinkCommand, ShortLinkDetailsResponse>, CreateShortLinkCommandHandler>();
        
        return services;
    }
}