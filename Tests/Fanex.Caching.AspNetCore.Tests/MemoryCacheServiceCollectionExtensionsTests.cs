namespace Fanex.Caching.AspNetCore.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Fanex.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class MemoryCacheServiceCollectionExtensionsTests
    {
        private readonly IServiceCollection _services;

        public MemoryCacheServiceCollectionExtensionsTests()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        public void CallAddMemoryCacheOnNullServiceCollectionThrowsArgumentNullException()
        {
            // Act.
            Exception exception = Record.Exception(() => MemoryCacheServiceCollectionExtensions.AddMemoryCache(null));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void AddMemoryCacheRegisterMemoryCacheAsSingleton()
        {
            // Act.
            _services.AddMemoryCache();

            // Assert.
            var memoryCache = _services.FirstOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ICache));

            Assert.Equal(ServiceLifetime.Singleton, memoryCache?.Lifetime);
        }

        [Fact]
        public void AddMemoryCacheRegisterCacheServiceAsSingleton()
        {
            // Act.
            _services.AddMemoryCache();

            // Assert.
            var cacheService = _services.FirstOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ICacheService));

            Assert.Equal(ServiceLifetime.Singleton, cacheService?.Lifetime);
        }

        [Fact]
        public void AddMemoryCacheAllowsChaining()
        {
            // Act.
            var returnedServices = _services.AddMemoryCache();

            // Assert.
            Assert.Same(_services, returnedServices);
        }

        [Fact]
        public void CallAddMemoryCacheWithConfigurationSectionOnNullServiceCollectionThrowsArgumentNullException()
        {
            // Arrange.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath("./"))
                .AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build().GetSection("fanex.caching");

            // Act.
            Exception exception = Record.Exception(() => MemoryCacheServiceCollectionExtensions.AddMemoryCache(null, configuration));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CallAddMemoryCacheWithNullConfigurationSectionThrowsArgumentNullException()
        {
            // Arrange.
            IConfigurationSection configuration = null;

            // Act.
            Exception exception = Record.Exception(() => _services.AddMemoryCache(configuration));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void AddMemoryCacheWithConfigurationSectionAllowsChaining()
        {
            // Arrange.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath("./"))
                .AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build().GetSection("fanex.caching");

            // Act.
            var returnedServices = _services.AddMemoryCache(configuration);

            // Assert.
            Assert.Same(_services, returnedServices);
        }

        [Fact]
        public void AddMemoryCacheReadsInformationFromConfigurationSection()
        {
            // Arrange.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath("./"))
                .AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build().GetSection("fanex.caching");

            // Act.
            _services.AddMemoryCache(configuration);

            var provider = _services.BuildServiceProvider();
            var options = provider.GetService<MemoryCacheOptions>();

            // Assert.
            Assert.NotNull(options);
            Assert.True(options.CompactOnMemoryPressure);
            Assert.Equal(TimeSpan.FromSeconds(30), options.ExpirationScanFrequency);
        }

        [Fact]
        public void CallAddMemoryCacheWithOptionAccessorOnNullServiceCollectionThrowsArgumentNullException()
        {
            // Act.
            Exception exception = Record.Exception(() => MemoryCacheServiceCollectionExtensions.AddMemoryCache(null, _ => { }));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CallAddMemoryCacheWithNullOptionAccessorThrowsArgumentNullException()
        {
            // Arrange.
            Action<MemoryCacheOptions> setupAction = null;

            // Act.
            Exception exception = Record.Exception(() => _services.AddMemoryCache(setupAction));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void AddMemoryCacheWithOptionAccessorAllowsChaining()
        {
            // Act.
            var returnedServices = _services.AddMemoryCache(_ => { });

            // Assert.
            Assert.Same(_services, returnedServices);
        }
    }
}