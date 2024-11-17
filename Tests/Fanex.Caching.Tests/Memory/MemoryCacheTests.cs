namespace Fanex.Caching.Tests.Memory;

using System;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Caching.Memory;
using Xunit;

public sealed class MemoryCacheTests : IDisposable
{
    private readonly Fixture _fixture;
    private readonly MemoryCache _cache;
    private readonly string _existingKey;
    private readonly object _existingValue;

    public MemoryCacheTests()
    {
        var cacheOptions = new MemoryCacheItemOptions().UseDefaultExpiration();

        _fixture = new Fixture();
        _cache = new MemoryCache();
        _existingKey = _fixture.Create<string>();
        _existingValue = _fixture.Create<object>();

        _cache.Set(_existingKey, _existingValue, cacheOptions);
    }

    [Fact]
    public void NewInstanceWithNullOptionsThrowsArgumentNullException()
    {
        // Act.
        var exception = Record.Exception(() => new MemoryCache(null));

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ContainReturnsFalseForNullOrEmptyKey(string key)
    {
        // Act.
        var found = _cache.Contain(key);

        // Assert.
        Assert.False(found);
    }

    [Fact]
    public void ContainReturnsFalseForNotExistedKey()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();

        // Act.
        var found = _cache.Contain(notExistedKey);

        // Assert.
        Assert.False(found);
    }

    [Fact]
    public void ContainReturnsTrueForExistingKey()
    {
        // Act.
        var found = _cache.Contain(_existingKey);

        // Assert.
        Assert.True(found);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ContainAsyncReturnsFalseForNullOrEmptyKey(string key)
    {
        // Act.
        var found = await _cache.ContainAsync(key);

        // Assert.
        Assert.False(found);
    }

    [Fact]
    public async Task ContainAsyncReturnsFalseForNotExistedKey()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();

        // Act.
        var found = await _cache.ContainAsync(notExistedKey);

        // Assert.
        Assert.False(found);
    }

    [Fact]
    public async Task ContainAsyncReturnsTrueForExistingKey()
    {
        // Act.
        var found = await _cache.ContainAsync(_existingKey);

        // Assert.
        Assert.True(found);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TryGetNullOrEmptyKeyThrowsArgumentException(string key)
    {
        // Act.
        var exception = Record.Exception(() => _cache.TryGet(key, out object _));

        // Assert.
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void TryGetNotExistedKeyReturnsFalse()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();

        // Act.
        var found = _cache.TryGet(notExistedKey, out object _);

        // Assert.
        Assert.False(found);
    }

    [Fact]
    public void TryGetNotExistedKeyTakesDefaultValueAsOutputParameter()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();

        // Act.
        _cache.TryGet(notExistedKey, out object actualValue);

        // Assert.
        var expectedValue = default(object);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TryGetExistingKeyReturnsTrue()
    {
        // Act.
        var found = _cache.TryGet(_existingKey, out object _);

        // Assert.
        Assert.True(found);
    }

    [Fact]
    public void TryGetExistingKeySetsCachedObjectAsOutputParameter()
    {
        // Act.
        _cache.TryGet(_existingKey, out object actualValue);

        // Assert.
        Assert.Same(_existingValue, actualValue);
    }

    [Fact]
    public void TryGetWithIncorrectValueTypeTakesDefaultValueAsOutputParameter()
    {
        // Act.
        _cache.TryGet(_existingKey, out int actualValue);

        // Assert.
        const int expectedValue = default;

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task TryGetAsyncNullOrEmptyKeyThrowsArgumentException(string key)
    {
        // Act.
        var exception = await Record
            .ExceptionAsync(() => _cache.TryGetAsync<object>(key))
            ;

        // Assert.
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public async Task TryGetAsyncNotExistedKeyReturnsFalseAsFirstResult()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();

        // Act.
        var (found, _) = await _cache.TryGetAsync<object>(notExistedKey);

        // Assert.
        Assert.False(found);
    }

    [Fact]
    public async Task TryGetAsyncNotExistedKeyReturnsDefaultValueAsSecondResult()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();

        // Act.
        var (_, actualValue) = await _cache.TryGetAsync<object>(notExistedKey);

        // Assert.
        var expectedValue = default(object);

        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public async Task TryGetAsyncExistingKeyReturnsTrueAsFirstResult()
    {
        // Act.
        var (found, _) = await _cache.TryGetAsync<object>(_existingKey);

        // Assert.
        Assert.True(found);
    }

    [Fact]
    public async Task TryGetAsyncExistingKeyReturnsCachedObjectAsSecondResult()
    {
        // Act.
        var (_, actualValue) = await _cache.TryGetAsync<object>(_existingKey);

        // Assert.
        Assert.Same(_existingValue, actualValue);
    }

    [Fact]
    public async Task TryGetAsyncWithIncorrectValueTypeReturnsDefaultValueAsSecondResult()
    {
        // Act.
        var (_, actualValue) = await _cache.TryGetAsync<int>(_existingKey);

        // Assert.
        const int expectedValue = default;

        Assert.Equal(expectedValue, actualValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void SetNullOrEmptyKeyThrowsArgumentException(string key)
    {
        // Arrange.
        var value = _fixture.Create<object>();
        var cacheOptions = new CacheItemOptions();

        // Act.
        var exception = Record.Exception(() => _cache.Set(key, value, cacheOptions));

        // Assert.
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void SetNullValueThrowsArgumentNullException()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var cacheOptions = new CacheItemOptions();

        // Act.
        var exception = Record.Exception(() => _cache.Set<object>(key, null, cacheOptions));

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void SetWithNullCacheItemOptionsThrowsArgumentNullException()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var value = _fixture.Create<object>();

        // Act.
        var exception = Record.Exception(() => _cache.Set(key, value, (CacheItemOptions)null));

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void SetWithCacheItemOptionsSavesObjectToStorage()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var value = _fixture.Create<object>();
        var cacheOptions = new CacheItemOptions();

        // Act.
        _cache.Set(key, value, cacheOptions);

        // Assert.
        var found = _cache.TryGet(key, out object _);

        Assert.True(found);
    }

    [Fact]
    public void SetWithMemoryCacheItemOptionsSavesObjectToStorage()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var value = _fixture.Create<object>();
        var cacheOptions = new MemoryCacheItemOptions();

        // Act.
        _cache.Set(key, value, cacheOptions);

        // Assert.
        var found = _cache.TryGet(key, out object _);

        Assert.True(found);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task SetAsyncNullOrEmptyKeyThrowsArgumentException(string key)
    {
        // Arrange.
        var value = _fixture.Create<object>();
        var cacheOptions = new CacheItemOptions();

        // Act.
        var exception = await Record
            .ExceptionAsync(() => _cache.SetAsync(key, value, cacheOptions))
            ;

        // Assert.
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public async Task SetAsyncNullValueThrowsArgumentNullException()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var cacheOptions = new CacheItemOptions();

        // Act.
        var exception = await Record
            .ExceptionAsync(() => _cache.SetAsync<object>(key, null, cacheOptions))
            ;

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public async Task SetAsyncWithNullCacheItemOptionsThrowsArgumentNullException()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var value = _fixture.Create<object>();

        // Act.
        var exception = await Record
            .ExceptionAsync(() => _cache.SetAsync(key, value, (CacheItemOptions)null))
            ;

        // Assert.
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public async Task SetAsyncTakesCacheItemOptions()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var value = _fixture.Create<object>();
        var cacheOptions = new CacheItemOptions();

        // Act.
        await _cache.SetAsync(key, value, cacheOptions);

        // Assert.
        var (found, _) = await _cache.TryGetAsync<object>(key);

        Assert.True(found);
    }

    [Fact]
    public async Task SetAsyncTakesMemoryCacheItemOptions()
    {
        // Arrange.
        var key = _fixture.Create<string>();
        var value = _fixture.Create<object>();
        var cacheOptions = new MemoryCacheItemOptions();

        // Act.
        await _cache.SetAsync(key, value, cacheOptions);

        // Assert.
        var (found, _) = await _cache.TryGetAsync<object>(key);

        Assert.True(found);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void RemoveNullOrEmptyKeyThrowsArgumentException(string key)
    {
        // Act.
        var exception = Record.Exception(() => _cache.Remove(key));

        // Assert.
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void RemoveNotExistedKeyDoesNoChangeCacheStorage()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();
        var numberOfExistingKeys = _cache.MemoryStorage.Count;

        // Act.
        _cache.Remove(notExistedKey);

        // Assert.
        Assert.Equal(numberOfExistingKeys, _cache.MemoryStorage.Count);
    }

    [Fact]
    public void RemoveExistingKeyRemoveItOutOfCacheStorage()
    {
        // Act.
        _cache.Remove(_existingKey);

        // Assert.
        var found = _cache.TryGet(_existingKey, out object _);

        Assert.False(found);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RemoveAsyncNullOrEmptyKeyThrowsArgumentException(string key)
    {
        // Act.
        var exception = await Record.ExceptionAsync(() => _cache.RemoveAsync(key));

        // Assert.
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public async Task RemoveAsyncNotExistedKeyDoesNoChangeCacheStorage()
    {
        // Arrange.
        var notExistedKey = _fixture.Create<string>();
        var numberOfExistingKeys = _cache.MemoryStorage.Count;

        // Act.
        await _cache.RemoveAsync(notExistedKey);

        // Assert.
        Assert.Equal(numberOfExistingKeys, _cache.MemoryStorage.Count);
    }

    [Fact]
    public async Task RemoveAsyncExistingKeyRemoveItOutOfCacheStorage()
    {
        // Act.
        await _cache.RemoveAsync(_existingKey);

        // Assert.
        var (found, _) = await _cache.TryGetAsync<object>(_existingKey);

        Assert.False(found);
    }

    public void Dispose()
        => _cache.Dispose();
}