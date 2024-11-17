namespace Fanex.Caching.Tests;

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
        var currentCacheStorage = CacheServiceManager.CacheStorage;

        // Assert.
        Assert.IsType<MemoryCache>(currentCacheStorage);
    }

    [Fact]
    public void ConfigureNullAsCacheStorageFactoryThrowsArgumentNullException()
    {
        // Arrange.

        CacheServiceManager.Reset();

        // Act
        var exception = Record.Exception(() => CacheServiceManager.Use((Func<ICache>)null));

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void UseConfiguredCacheStorageFactoryToCreateStorage()
    {
        // Arrange.
        var fakeCacheStorage = Substitute.For<ICache>();

        CacheServiceManager.Reset();
        CacheServiceManager.Use(() => fakeCacheStorage);

        // Act
        var currentCacheStorage = CacheServiceManager.CacheStorage;

        // Assert.
        Assert.Same(fakeCacheStorage, currentCacheStorage);
    }

    [Fact]
    public void ConfigureNullAsCacheStorageThrowsArgumentNullException()
    {
        // Arrange.

        CacheServiceManager.Reset();

        // Act
        var exception = Record.Exception(() => CacheServiceManager.Use((ICache)null));

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