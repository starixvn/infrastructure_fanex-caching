#if COREFX

namespace Fanex.Caching
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Redis;

    public static class RedisCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.TryAdd(ServiceDescriptor.Singleton<Cache, RedisCache>());
            services.TryAdd(ServiceDescriptor.Singleton<CacheService, CacheServiceImpl>());

            return services;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfigurationSection configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddOptions();
            services.Configure<RedisCacheOptions>(configuration);
            services.TryAdd(ServiceDescriptor.Singleton<Cache, RedisCache>());
            services.TryAdd(ServiceDescriptor.Singleton<CacheService, CacheServiceImpl>());

            return services;
        }
    }
}

#endif