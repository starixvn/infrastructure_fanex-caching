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
        private readonly MemoryCacheItemOptions _cacheOptions;

        public MemoryCacheTests()
        {
            _fixture = new Fixture();
            _cache = new MemoryCache();
            _existingKey = _fixture.Create<string>();
            _existingValue = _fixture.Create<object>();
            _cacheOptions = new MemoryCacheItemOptions();

            _cache.Set(_existingKey, _existingValue, _cacheOptions);
        }

        [Fact]
        public void NewInstanceWithNullOptionsThrowsArgumentNullException()
        {
            // Act.
            Exception exception = Record.Exception(() => new MemoryCache(null));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ContainReturnsFalseIfPassingKeyIsNullOrEmpty(string key)
        {
            // Act.
            bool found = _cache.Contain(key);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void ContainReturnsFalseForNonexistingKey()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();

            // Act.
            bool found = _cache.Contain(nonexistingKey);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void ContainReturnsTrueForExistingKey()
        {
            // Act.
            bool found = _cache.Contain(_existingKey);

            // Assert.
            Assert.True(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ContainAsyncReturnsFalseIfPassingKeyIsNullOrEmpty(string key)
        {
            // Act.
            bool found = await _cache.ContainAsync(key);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task ContainAsyncReturnsFalseForNonexistingKey()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();

            // Act.
            bool found = await _cache.ContainAsync(nonexistingKey);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task ContainAsyncReturnsTrueForExistingKey()
        {
            // Act.
            bool found = await _cache.ContainAsync(_existingKey);

            // Assert.
            Assert.True(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TryGetNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            Exception exception = Record.Exception(() => _cache.TryGet<object>(key, out _));

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void TryGetUnexistingKeyReturnsFalse()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();

            // Act.
            bool found = _cache.TryGet(nonexistingKey, out object _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetUnexistingKeyTakesDefaultValueOfTypeAsOutputParameter()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();

            // Act.
            _cache.TryGet(nonexistingKey, out object value);

            // Assert.
            var expectedValue = default(object);

            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void TryGetExistingKeyReturnsTrue()
        {
            // Act.
            bool found = _cache.TryGet(_existingKey, out object _);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public void TryGetExistingKeyWithCorrectValueTypeHasCachedObjectAsOutputParameter()
        {
            // Act.
            _cache.TryGet(_existingKey, out object value);

            // Assert.
            Assert.Same(_existingValue, value);
        }

        [Fact]
        public void TryGetExistingKeyWithIncorrectValueTypeTakesDefaultValueOfTypeAsOutputParameter()
        {
            // Act.
            _cache.TryGet(_existingKey, out int value);

            // Assert.
            var expectedValue = default(int);

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task TryGetAsyncNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            Exception exception = await Record.ExceptionAsync(() => _cache.TryGetAsync<object>(key));

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task TryGetAsyncUnexistingKeyReturnsFalseAsFirstResult()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();

            // Act.
            Tuple<bool, object> result = await _cache.TryGetAsync<object>(nonexistingKey);

            // Assert.
            Assert.False(result.Item1);
        }

        [Fact]
        public async Task TryGetAsyncUnexistingKeyReturnsDefaultValueOfTypeAsSecondResult()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();

            // Act.
            Tuple<bool, object> result = await _cache.TryGetAsync<object>(nonexistingKey);

            // Assert.
            var expectedValue = default(object);

            Assert.Equal(expectedValue, result.Item2);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyReturnsTrueAsFirstResult()
        {
            // Act.
            Tuple<bool, object> result = await _cache.TryGetAsync<object>(_existingKey);

            // Assert.
            Assert.True(result.Item1);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyWithCorrectValueTypeReturnsCachedObjectAsSecondResult()
        {
            // Act.
            Tuple<bool, object> result = await _cache.TryGetAsync<object>(_existingKey);

            // Assert.
            Assert.Same(_existingValue, result.Item2);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyWithIncorrectValueTypeReturnsDefaultValueOfTypeAsSecondResult()
        {
            // Act.
            Tuple<bool, int> result = await _cache.TryGetAsync<int>(_existingKey);

            // Assert.
            var expectedValue = default(int);

            Assert.Equal(expectedValue, result.Item2);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void RemoveNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            Exception exception = Record.Exception(() => _cache.Remove(key));

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void RemoveUnexistingKeyDoesNoChangeCacheStorage()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();
            var numberOfExistingKeys = _cache.MemoryStorage.Count;

            // Act.
            _cache.Remove(nonexistingKey);

            // Assert.
            Assert.Equal(numberOfExistingKeys, _cache.MemoryStorage.Count);
        }

        [Fact]
        public void RemoveExistingKeyRemoveItOutOfCacheStroage()
        {
            // Act.
            _cache.Remove(_existingKey);

            // Assert.
            Assert.False(_cache.TryGet<object>(_existingKey, out _));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task RemoveAsyncNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            Exception exception = await Record.ExceptionAsync(() => _cache.RemoveAsync(key));

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task RemoveAsyncUnexistingKeyDoesNoChangeCacheStorage()
        {
            // Arrange.
            var nonexistingKey = _fixture.Create<string>();
            var numberOfExistingKeys = _cache.MemoryStorage.Count;

            // Act.
            await _cache.RemoveAsync(nonexistingKey);

            // Assert.
            Assert.Equal(numberOfExistingKeys, _cache.MemoryStorage.Count);
        }

        [Fact]
        public async Task RemoveAsyncExistingKeyRemoveItOutOfCacheStroage()
        {
            // Act.
            await _cache.RemoveAsync(_existingKey);

            // Assert.
            Assert.False(_cache.TryGet<object>(_existingKey, out _));
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
            Exception exception = Record.Exception(() => _cache.Set(key, value, cacheOptions));

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
            Exception exception = Record.Exception(() => _cache.Set<object>(key, null, cacheOptions));

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
            Exception exception = Record.Exception(() => _cache.Set(key, value, cacheOptions));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void SetTakesCacheItemOptions()
        {
            // Arrange.
            var key = _fixture.Create<string>();
            var value = _fixture.Create<object>();
            var cacheOptions = new CacheItemOptions();

            // Act.
            _cache.Set(key, value, cacheOptions);

            // Assert.
            Assert.True(_cache.TryGet<object>(key, out _));
        }

        [Fact]
        public void SetTakesMemoryCacheItemOptions()
        {
            // Arrange.
            var key = _fixture.Create<string>();
            var value = _fixture.Create<object>();
            var cacheOptions = new MemoryCacheItemOptions();

            // Act.
            _cache.Set(key, value, cacheOptions);

            // Assert.
            Assert.True(_cache.TryGet<object>(key, out _));
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
            Exception exception = await Record.ExceptionAsync(() => _cache.SetAsync(key, value, cacheOptions));

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
            Exception exception = await Record.ExceptionAsync(() => _cache.SetAsync<object>(key, null, cacheOptions));

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
            Exception exception = await Record.ExceptionAsync(() => _cache.SetAsync(key, value, cacheOptions));

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
            Assert.True(_cache.TryGet<object>(key, out _));
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
            Assert.True(_cache.TryGet<object>(key, out _));
        }

        public void Dispose()
            => _cache.Dispose();
    }
}