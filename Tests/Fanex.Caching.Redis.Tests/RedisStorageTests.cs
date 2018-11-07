namespace Fanex.Caching.Redis.Tests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public sealed class RedisStorageTests : IClassFixture<RedisCacheFixture>, IDisposable
    {
        private readonly Fixture _fixture;
        private readonly string _existingKey;
        private readonly object _existingValue;

        public RedisStorageTests(RedisCacheFixture cacheFixture)
        {
            var cacheOptions = new CacheItemOptions();

            _fixture = new Fixture();
            _existingKey = _fixture.Create<string>();
            _existingValue = _fixture.Create<string>();

            Clock = cacheFixture.CacheOptions.Clock;
            Storage = cacheFixture.Cache.RedisStorage;

            Storage.Set(_existingKey, _existingValue, cacheOptions);
        }

        private RedisStorage Storage { get; }

        private ISystemClock Clock { get; }

        [Fact]
        public void ContainMissingKeyReturnsFalse()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            var found = Storage.Contain(missingKey);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void ContainExistingKeyReturnsTrue()
        {
            // Act.
            var found = Storage.Contain(_existingKey);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public async Task ContainAsyncMissingKeyReturnsFalse()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            var found = await Storage.ContainAsync(missingKey).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task ContainAsyncExistingKeyReturnsTrue()
        {
            // Act.
            var found = await Storage.ContainAsync(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public void TryGetMissingKeyReturnsFalse()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            var found = Storage.TryGet(missingKey, out object _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetMissingKeyReturnsNullAsOutput()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            Storage.TryGet(missingKey, out object actualValue);

            // Assert.
            Assert.Null(actualValue);
        }

        [Fact]
        public void TryGetExistingKeyReturnsTrue()
        {
            // Act.
            var found = Storage.TryGet(_existingKey, out string _);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public void TryGetExistingKeyReturnsCachedObjectAsOutput()
        {
            // Act.
            Storage.TryGet(_existingKey, out string actualValue);

            // Assert.
            Assert.Equal(_existingValue, actualValue);
        }

        [Fact]
        public async Task TryGetAsyncMissingKeyReturnsFalseAsFirstResult()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            var (found, _) = await Storage.TryGetAsync<object>(missingKey).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task TryGetAsyncMissingKeyReturnsNullAsSecondResult()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            var (_, actualValue) = await Storage.TryGetAsync<object>(missingKey).ConfigureAwait(false);

            // Assert.
            Assert.Null(actualValue);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyReturnsTrue()
        {
            // Act.
            var (found, _) = await Storage.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public async Task TryGetAsyncExistingKeyReturnsCachedObjectAsOutput()
        {
            // Act.
            var (_, actualValue) = await Storage.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            // Assert.
            Assert.Equal(_existingValue, actualValue);
        }

        [Fact]
        public void SetAndTryGetWorkWithCaseSensitiveKeys()
        {
            // Arrange.
            var caseSensitiveKey = _existingKey.ToUpperInvariant();

            // Act.
            var found = Storage.TryGet(caseSensitiveKey, out string _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void SetAlwaysOverwritesExistingKey()
        {
            // Arrange.
            var newValue = _fixture.Create<string>();
            var newCacheOptions = new CacheItemOptions();

            // Act.
            Storage.Set(_existingKey, newValue, newCacheOptions);

            // Assert.
            Storage.TryGet(_existingKey, out string actualValue);
            Assert.Equal(newValue, actualValue);
        }

        [Fact]
        public void SetExistingKeyWithValueBeingExpiredRemovesOldValue()
        {
            // Arrange.
            var newValue = _fixture.Create<string>();
            var expiredTime = Clock.UtcNow - TimeSpan.FromSeconds(1);
            var expiredPolicy = new CacheItemOptions().SetAbsoluteExpiration(expiredTime);

            // Act.
            Storage.Set(_existingKey, newValue, expiredPolicy);

            // Assert.
            var found = Storage.TryGet(_existingKey, out string _);

            Assert.False(found);
        }

        [Fact]
        public async Task SetAsyncAndTryGetAsyncWorkWithCaseSensitiveKeys()
        {
            // Arrange.
            var caseSensitiveKey = _existingKey.ToUpperInvariant();

            // Act.
            var (found, _) = await Storage.TryGetAsync<string>(caseSensitiveKey).ConfigureAwait(false);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public async Task SetAsyncAlwaysOverwritesExistingKey()
        {
            // Arrange.
            var newValue = _fixture.Create<string>();
            var newCacheOptions = new CacheItemOptions();

            // Act.
            await Storage.SetAsync(_existingKey, newValue, newCacheOptions).ConfigureAwait(false);

            // Assert.
            var (_, actualValue) = await Storage.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            Assert.Equal(newValue, actualValue);
        }

        [Fact]
        public async Task SetAsyncExistingKeyWithValueBeingExpiredRemovesOldValue()
        {
            // Arrange.
            var newValue = _fixture.Create<string>();
            var expiredTime = Clock.UtcNow - TimeSpan.FromSeconds(2);
            var expiredPolicy = new CacheItemOptions().SetAbsoluteExpiration(expiredTime);

            // Act.
            await Storage.SetAsync(_existingKey, newValue, expiredPolicy).ConfigureAwait(false);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            Assert.False(found);
        }

        [Fact]
        public void RemoveRemovesExistingKey()
        {
            // Act.
            Storage.Remove(_existingKey);

            // Assert.
            var found = Storage.TryGet(_existingKey, out string _);

            Assert.False(found);
        }

        [Fact]
        public async Task RemoveAsyncRemovesExistingKey()
        {
            // Act.
            await Storage.RemoveAsync(_existingKey).ConfigureAwait(false);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_existingKey).ConfigureAwait(false);

            Assert.False(found);
        }

        public void Dispose()
            => Storage.Remove(_existingKey);
    }
}