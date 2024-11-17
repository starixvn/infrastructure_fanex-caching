#if NET6_0_OR_GREATER
namespace Microsoft.Extensions.Configuration;

using System;
using DependencyInjection;
using DependencyInjection.Extensions;
using Fanex.Caching;
using Fanex.Caching.Memory;


public static class MemoryCacheServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryCache(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.TryAdd(ServiceDescriptor.Singleton<ICache, MemoryCache>());
        services.TryAdd(ServiceDescriptor.Singleton<ICacheService, CacheService>());

        return services;
    }

    public static IServiceCollection AddMemoryCache(this IServiceCollection services, IConfigurationSection configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var options = new MemoryCacheOptions();

        configuration.Bind(options);
        services.AddSingleton(options);
        services.AddMemoryCache();

        return services;
    }

    public static IServiceCollection AddMemoryCache(this IServiceCollection services, Action<MemoryCacheOptions> setupAction)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        services.Configure(setupAction);
        services.AddMemoryCache();

        return services;
    }
}
#endif