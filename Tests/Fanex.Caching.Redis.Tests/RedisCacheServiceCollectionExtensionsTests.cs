namespace Fanex.Caching.Redis.Tests;

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class RedisCacheServiceCollectionExtensionsTests
{
    private readonly IServiceCollection _services = new ServiceCollection();

    [Fact]
    public void CallAddRedisCacheOnNullServiceCollectionThrowsArgumentNullException()
    {
        // Act.
        var exception = Record.Exception(() => RedisCacheServiceCollectionExtensions.AddRedisCache(null));

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void AddRedisCacheRegistersRedisCacheAsSingleton()
    {
        // Act.
        _services.AddRedisCache();

        // Assert.
        var memoryCache = _services.FirstOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ICache));

        Assert.Equal(ServiceLifetime.Singleton, memoryCache?.Lifetime);
    }

    [Fact]
    public void AddRedisCacheRegistersCacheServiceAsSingleton()
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
        var exception = Record.Exception(() => RedisCacheServiceCollectionExtensions.AddRedisCache(null, configuration));

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void CallAddRedisCacheWithNullConfigurationSectionThrowsArgumentNullException()
    {
        // Arrange.

        // Act.
        var exception = Record.Exception(() => _services.AddRedisCache((IConfigurationSection)null));

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

        // Assert.
        const string expectedApplicationName = "Fanex.Caching.Redis.AspNetCore.Tests";

        var options = _services.BuildServiceProvider().GetService<RedisCacheOptions>();

        Assert.NotNull(options);
        Assert.NotEmpty(options.ConnectionOptions.EndPoints);
        Assert.Equal(expectedApplicationName, options.ApplicationName);
    }

    [Fact]
    public void AddRedisCacheWithConfigurationSectionDoesNotOverwriteClock()
    {
        // Arrange.
        var configuration = ReadConfiguration();

        // Act.
        _services.AddRedisCache(configuration);

        // Assert.
        var options = _services.BuildServiceProvider().GetService<RedisCacheOptions>();

        Assert.NotNull(options.Clock);
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
        var exception = Record.Exception(() => RedisCacheServiceCollectionExtensions.AddRedisCache(null, _ => { }));

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void CallAddRedisCacheWithNullOptionAccessorThrowsArgumentNullException()
    {
        // Arrange.

        // Act.
        var exception = Record.Exception(() => _services.AddRedisCache((Action<RedisCacheOptions>)null));

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