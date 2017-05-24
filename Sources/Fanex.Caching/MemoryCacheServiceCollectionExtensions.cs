#if COREFX

namespace Fanex.Caching
{
    using System;
    using Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class MemoryCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryCache(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<Cache, MemoryCache>());
            services.TryAdd(ServiceDescriptor.Singleton<CacheService, CacheServiceImpl>());

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

            services.AddMemoryCache();
            services.Configure<MemoryCacheOptions>(configuration);

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

            services.AddMemoryCache();
            services.Configure(setupAction);

            return services;
        }
    }
}

#endif