namespace Fanex.Caching.Redis.Tests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public sealed class RedisCacheTests : IClassFixture<RedisCacheFixture>, IDisposable
    {
        private readonly Fixture _fixture;
        private readonly string _existingKey;
        private readonly string _existingValue;

        public RedisCacheTests(RedisCacheFixture cacheFixture)
        {
            var cacheOptions = new CacheItemOptions().UseDefaultExpiration();

            _fixture = new Fixture();
            _existingKey = _fixture.Create<string>();
            _existingValue = _fixture.Create<string>();

            Cache = cacheFixture.Cache;
            Cache.Set(_existingKey, _existingValue, cacheOptions);
        }

        private RedisCache Cache { get; }

        [Fact]
        public void NewInstanceWithNullOptionsThrowsArgumentNullException()
        {
            // Act.
            var exception = Record.Exception(() => new RedisCache(null));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ContainReturnsFalseForNullOrEmptyKey(string key)
        {
            // Act.
            var found = Cache.Contain(key);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void ContainReturnsFalseForUnexistingKey()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var found = Cache.Contain(unexistingKey);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void ContainReturnsTrueForExistingKey()
        {
            // Act.
            var found = Cache.Contain(_existingKey);

            // Assert.
            Assert.True(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ContainAsyncReturnsFalseForNullOrEmptyKey(string key)
        {
            // Act.
            var found = await Cache.ContainAsync(key).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task ContainAsyncReturnsFalseForUnexistingKey()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var found = await Cache.ContainAsync(unexistingKey).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task ContainAsyncReturnsTrueForExistingKey()
        {
            // Act.
            var found = await Cache.ContainAsync(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.True(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TryGetNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            var exception = Record.Exception(() => Cache.TryGet(key, out string _));

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void TryGetUnexistingKeyReturnsFalse()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var found = Cache.TryGet(unexistingKey, out string _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetUnexistingKeyTakesDefaultValueAsOutputParameter()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            Cache.TryGet(unexistingKey, out string actualValue);

            // Assert.
            Assert.Equal(default, actualValue);
        }

        [Fact]
        public void TryGetExistingKeyReturnsTrue()
        {
            // Act.
            var found = Cache.TryGet(_existingKey, out string _);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public void TryGetExistingKeySetsCachedObjectAsOutputParameter()
        {
            // Act.
            Cache.TryGet(_existingKey, out string actualValue);

            // Assert.
            Assert.Equal(_existingValue, actualValue);
        }

        [Fact]
        public void TryGetWithIncorrectValueTypeTakesDefaultValueAsOutputParameter()
        {
            // Act.
            Cache.TryGet(_existingKey, out int actualValue);

            // Assert.
            Assert.Equal(default, actualValue);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task TryGetAsyncNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            var exception = await Record
                .ExceptionAsync(() => Cache.TryGetAsync<string>(key))
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
            var (found, _) = await Cache.TryGetAsync<string>(unexistingKey).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task TryGetAsyncUnexistingKeyReturnsDefaultValueAsSecondResult()
        {
            // Arrange.
            var unexistingKey = _fixture.Create<string>();

            // Act.
            var (_, actualValue) = await Cache.TryGetAsync<string>(unexistingKey).ConfigureAwait(false);

            // Assert.
            Assert.Equal(default, actualValue);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyReturnsTrueAsFirstResult()
        {
            // Act.
            var (found, _) = await Cache.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyReturnsCachedObjectAsSecondResult()
        {
            // Act.
            var (_, actualValue) = await Cache.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.Equal(_existingValue, actualValue);
        }

        [Fact]
        public async Task TryGetAsyncWithIncorrectValueTypeReturnsDefaultValueAsSecondResult()
        {
            // Act.
            var (_, actualValue) = await Cache.TryGetAsync<int>(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.Equal(default, actualValue);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void SetNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Arrange.
            var value = _fixture.Create<string>();
            var cacheOptions = new CacheItemOptions();

            // Act.
            var exception = Record.Exception(() => Cache.Set(key, value, cacheOptions));

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
            var exception = Record.Exception(() => Cache.Set<string>(key, null, cacheOptions));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void SetWithNullCacheItemOptionsThrowsArgumentNullException()
        {
            // Arrange.
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            CacheItemOptions cacheOptions = null;

            // Act.
            var exception = Record.Exception(() => Cache.Set(key, value, cacheOptions));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void SetSavesObjectToStorage()
        {
            // Arrange.
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            var cacheOptions = new CacheItemOptions();

            // Act.
            Cache.Set(key, value, cacheOptions);

            // Assert.
            var found = Cache.TryGet(key, out string _);

            Assert.True(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SetAsyncNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Arrange.
            var value = _fixture.Create<string>();
            var cacheOptions = new CacheItemOptions();

            // Act.
            var exception = await Record
                .ExceptionAsync(() => Cache.SetAsync(key, value, cacheOptions))
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
                .ExceptionAsync(() => Cache.SetAsync<string>(key, null, cacheOptions))
                .ConfigureAwait(false);

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task SetAsyncWithNullCacheItemOptionsThrowsArgumentNullException()
        {
            // Arrange.
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            CacheItemOptions cacheOptions = null;

            // Act.
            var exception = await Record
                .ExceptionAsync(() => Cache.SetAsync(key, value, cacheOptions))
                .ConfigureAwait(false);

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public async Task SetAsyncSavesObjectToStorage()
        {
            // Arrange.
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            var cacheOptions = new CacheItemOptions();

            // Act.
            await Cache.SetAsync(key, value, cacheOptions).ConfigureAwait(false);

            // Assert.
            var (found, _) = await Cache.TryGetAsync<string>(key).ConfigureAwait(false);

            Assert.True(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void RemoveNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            var exception = Record.Exception(() => Cache.Remove(key));

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void RemoveExistingKeyRemoveItOutOfCacheStorage()
        {
            // Act.
            Cache.Remove(_existingKey);

            // Assert.
            var found = Cache.TryGet(_existingKey, out string _);

            Assert.False(found);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task RemoveAsyncNullOrEmptyKeyThrowsArgumentException(string key)
        {
            // Act.
            var exception = await Record.ExceptionAsync(() => Cache.RemoveAsync(key)).ConfigureAwait(false);

            // Assert.
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public async Task RemoveAsyncExistingKeyRemoveItOutOfCacheStroage()
        {
            // Act.
            await Cache.RemoveAsync(_existingKey).ConfigureAwait(false);

            // Assert.
            var (found, _) = await Cache.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            Assert.False(found);
        }

        public void Dispose()
            => Cache.RedisStorage.Remove(_existingKey);
    }
}