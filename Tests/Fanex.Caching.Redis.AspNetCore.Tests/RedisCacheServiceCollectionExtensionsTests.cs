namespace Fanex.Caching.Redis.AspNetCore.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Fanex.Caching.Redis;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class RedisCacheServiceCollectionExtensionsTests
    {
        private readonly IServiceCollection _services;

        public RedisCacheServiceCollectionExtensionsTests()
            => _services = new ServiceCollection();

        [Fact]
        public void CallAddRedisCacheOnNullServiceCollectionThrowsArgumentNullException()
        {
            // Act.
            Exception exception = Record.Exception(() => RedisCacheServiceCollectionExtensions.AddRedisCache(null));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void AddRedisCacheRegisterRedisCacheAsSingleton()
        {
            // Act.
            _services.AddRedisCache();

            // Assert.
            var memoryCache = _services.FirstOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ICache));

            Assert.Equal(ServiceLifetime.Singleton, memoryCache?.Lifetime);
        }

        [Fact]
        public void AddRedisCacheRegisterCacheServiceAsSingleton()
        {
            // Act.
            _services.AddRedisCache();

            // Assert.
            var cacheService = _services.FirstOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ICacheService));

            Assert.Equal(ServiceLifetime.Singleton, cacheService?.Lifetime);
        }

        [Fact]
        public void AddRedisCacheAllowsChaining()
        {
            // Act.
            var returnedServices = _services.AddRedisCache();

            // Assert.
            Assert.Same(_services, returnedServices);
        }

        [Fact]
        public void CallAddRedisCacheWithConfigurationSectionOnNullServiceCollectionThrowsArgumentNullException()
        {
            // Arrange.
            var configuration = ReadConfiguration();

            // Act.
            Exception exception = Record.Exception(() => RedisCacheServiceCollectionExtensions.AddRedisCache(null, configuration));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CallAddRedisCacheWithNullConfigurationSectionThrowsArgumentNullException()
        {
            // Arrange.
            IConfigurationSection configuration = null;

            // Act.
            Exception exception = Record.Exception(() => _services.AddRedisCache(configuration));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void AddRedisCacheWithConfigurationSectionAllowsChaining()
        {
            // Arrange.
            var configuration = ReadConfiguration();

            // Act.
            var returnedServices = _services.AddRedisCache(configuration);

            // Assert.
            Assert.Same(_services, returnedServices);
        }

        [Fact]
        public void AddRedisCacheReadsInformationFromConfigurationSection()
        {
            // Arrange.
            var configuration = ReadConfiguration();

            // Act.
            _services.AddRedisCache(configuration);

            var provider = _services.BuildServiceProvider();
            var options = provider.GetService<RedisCacheOptions>();

            // Assert.
            var expectedHost = "127.0.0.1";
            var expectedPort = 6379;
            var expectedApplicationName = "Fanex.Caching.Redis";

            Assert.NotNull(options);
            Assert.Equal(expectedHost, options.Host);
            Assert.Equal(expectedPort, options.Port);
            Assert.Equal(expectedApplicationName, options.ApplicationName);
        }

        private static IConfigurationSection ReadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath("./"))
                .AddJsonFile("appsettings.json", true, true);

            return builder.Build().GetSection("fanex.caching.redis");
        }

        [Fact]
        public void CallAddRedisCacheWithOptionAccessorOnNullServiceCollectionThrowsArgumentNullException()
        {
            // Act.
            Exception exception = Record.Exception(() => RedisCacheServiceCollectionExtensions.AddRedisCache(null, _ => { }));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CallAddRedisCacheWithNullOptionAccessorThrowsArgumentNullException()
        {
            // Arrange.
            Action<RedisCacheOptions> setupAction = null;

            // Act.
            Exception exception = Record.Exception(() => _services.AddRedisCache(setupAction));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void AddRedisCacheWithOptionAccessorAllowsChaining()
        {
            // Act.
            var returnedServices = _services.AddRedisCache(_ => { });

            // Assert.
            Assert.Same(_services, returnedServices);
        }
    }
}