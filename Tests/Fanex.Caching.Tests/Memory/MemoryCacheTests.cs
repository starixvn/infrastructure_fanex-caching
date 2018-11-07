namespace Fanex.Caching.Tests.Memory
{
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
        public void ContainReturnsFalseForUnexistingKey()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var found = _cache.Contain(unexistingKey);

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
            var found = await _cache.ContainAsync(key).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task ContainAsyncReturnsFalseForUnexistingKey()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var found = await _cache.ContainAsync(unexistingKey).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task ContainAsyncReturnsTrueForExistingKey()
        {
            // Act.
            var found = await _cache.ContainAsync(_existingKey).ConfigureAwait(false);

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
        public void TryGetUnexistingKeyReturnsFalse()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var found = _cache.TryGet(unexistingKey, out object _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetUnexistingKeyTakesDefaultValueAsOutputParameter()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            _cache.TryGet(unexistingKey, out object actualValue);

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
            var expectedValue = default(int);

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
                .ConfigureAwait(false);

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task TryGetAsyncUnexistingKeyReturnsFalseAsFirstResult()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var (found, _) = await _cache.TryGetAsync<object>(unexistingKey).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task TryGetAsyncUnexistingKeyReturnsDefaultValueAsSecondResult()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var (_, actualValue) = await _cache.TryGetAsync<object>(unexistingKey).ConfigureAwait(false);

            // Assert.
            var expectedValue = default(object);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyReturnsTrueAsFirstResult()
        {
            // Act.
            var (found, _) = await _cache.TryGetAsync<object>(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyReturnsCachedObjectAsSecondResult()
        {
            // Act.
            var (_, actualValue) = await _cache.TryGetAsync<object>(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.Same(_existingValue, actualValue);
        }

        [Fact]
        public async Task TryGetAsyncWithIncorrectValueTypeReturnsDefaultValueAsSecondResult()
        {
            // Act.
            var (_, actualValue) = await _cache.TryGetAsync<int>(_existingKey).ConfigureAwait(false);

            // Assert.
            var expectedValue = default(int);

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
            CacheItemOptions cacheOptions = null;

            // Act.
            var exception = Record.Exception(() => _cache.Set(key, value, cacheOptions));

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
                .ConfigureAwait(false);

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
                .ConfigureAwait(false);

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task SetAsyncWithNullCacheItemOptionsThrowsArgumentNullException()
        {
            // Arrange.
            var key = _fixture.Create<string>();
            var value = _fixture.Create<object>();
            CacheItemOptions cacheOptions = null;

            // Act.
            var exception = await Record
                .ExceptionAsync(() => _cache.SetAsync(key, value, cacheOptions))
                .ConfigureAwait(false);

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
            await _cache.SetAsync(key, value, cacheOptions).ConfigureAwait(false);

            // Assert.
            var (found, _) = await _cache.TryGetAsync<object>(key).ConfigureAwait(false);

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
            await _cache.SetAsync(key, value, cacheOptions).ConfigureAwait(false);

            // Assert.
            var (found, _) = await _cache.TryGetAsync<object>(key).ConfigureAwait(false);

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
        public void RemoveUnexistingKeyDoesNoChangeCacheStorage()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();
            var numberOfExistingKeys = _cache.MemoryStorage.Count;

            // Act.
            _cache.Remove(unexistingKey);

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
            var exception = await Record.ExceptionAsync(() => _cache.RemoveAsync(key)).ConfigureAwait(false);

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task RemoveAsyncUnexistingKeyDoesNoChangeCacheStorage()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();
            var numberOfExistingKeys = _cache.MemoryStorage.Count;

            // Act.
            await _cache.RemoveAsync(unexistingKey).ConfigureAwait(false);

            // Assert.
            Assert.Equal(numberOfExistingKeys, _cache.MemoryStorage.Count);
        }

        [Fact]
        public async Task RemoveAsyncExistingKeyRemoveItOutOfCacheStorage()
        {
            // Act.
            await _cache.RemoveAsync(_existingKey).ConfigureAwait(false);

            // Assert.
            var (found, _) = await _cache.TryGetAsync<object>(_existingKey).ConfigureAwait(false);

            Assert.False(found);
        }

        public void Dispose()
            => _cache.Dispose();
    }
}