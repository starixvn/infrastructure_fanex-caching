namespace Fanex.Caching.Redis
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class RedisCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAdd(ServiceDescriptor.Singleton<ICache, RedisCache>());
            services.TryAdd(ServiceDescriptor.Singleton<ICacheService, CacheService>());

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

            var options = new RedisCacheOptions();

            configuration.Bind(options);
            services.AddSingleton(options);
            services.AddRedisCache();

            return services;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services, Action<RedisCacheOptions> setupAction)
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
            services.AddRedisCache();

            return services;
        }
    }
}