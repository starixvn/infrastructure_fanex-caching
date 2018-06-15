namespace Fanex.Caching.Tests
{
    using System;
    using Fanex.Caching.Memory;
    using NSubstitute;
    using Xunit;

    public class CacheServiceManagerTests
    {
        [Fact]
        public void MemoryCacheAsDefaultStorageIfNoConfigurationProvided()
        {
            // Arrange.
            CacheServiceManager.Reset();

            // Act
            ICache currentCacheStorage = CacheServiceManager.CacheStorage;

            // Assert.
            Assert.IsType<MemoryCache>(currentCacheStorage);
        }

        [Fact]
        public void ConfigureCacheStorageFactoryThrowsArgumentNullExceptionIfTakingNullAsParameter()
        {
            // Arrange.
            Func<ICache> cacheFactory = null;

            CacheServiceManager.Reset();

            // Act
            Exception exception = Record.Exception(() => CacheServiceManager.Use(cacheFactory));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void UseConfiguredCacheStorageFactoryToCreateStorage()
        {
            // Arrange.
            ICache fakeCacheStorage = Substitute.For<ICache>();

            CacheServiceManager.Reset();
            CacheServiceManager.Use(() => fakeCacheStorage);

            // Act
            ICache currentCacheStorage = CacheServiceManager.CacheStorage;

            // Assert.
            Assert.Same(fakeCacheStorage, currentCacheStorage);
        }

        [Fact]
        public void ConfigureCacheStorageThrowsArgumentNullExceptionIfTakingNullAsParameter()
        {
            // Arrange.
            ICache cache = null;

            CacheServiceManager.Reset();

            // Act
            Exception exception = Record.Exception(() => CacheServiceManager.Use(cache));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ConfigureCacheStorageDisposesCurrentCacheStorage()
        {
            // Arrange.
            var preConfiguredCacheStorage = new MemoryCache();

            CacheServiceManager.Reset();
            CacheServiceManager.Use(preConfiguredCacheStorage);

            // Act
            CacheServiceManager.Use(Substitute.For<ICache>());

            // Assert.
            Assert.Null(preConfiguredCacheStorage.MemoryStorage);
        }
    }
}