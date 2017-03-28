namespace Fanex.Caching.Redis.Tests
{
    using System;
    using System.Threading;
    using Caching.Tests.Mocks;
    using Xunit;

    public class RedisCacheTests
    {
        private readonly RedisCache _cache;
        private readonly CacheItemOptions _options;

        public RedisCacheTests()
        {
            _cache = new RedisCache(new RedisCacheOptions
            {
                ////ConnectionString = "10.70.70.249:8000,10.70.70.250:8000,10.70.70.251:8000",
                ConnectionString = "127.0.0.1:6379",
                ApplicationName = "Fanex.Caching.Redis.Tests"
            });
            _options = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
        }

        [Fact]
        public void TryGetMissingKeyReturnsNull()
        {
            // Arrange.
            string key = "non-existent-key";

            // Act.
            int? result;
            bool found = _cache.TryGet(key, out result);

            // Assert.
            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void SetAndTryGetReturnsObject()
        {
            // Arrange.
            string key = "testing-key";
            int? value = 1;

            // Act.
            int? result;

            _cache.Set(key, value, _options);
            _cache.TryGet(key, out result);

            // Assert.
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void SetAndGetWorkWithCaseSensitiveKeys()
        {
            // Arrange.
            string key = "testing-key";
            string caseSensitiveKey = "Testing-Key";
            int? value = 1;
            int? result;

            _cache.Set(key, value, _options);

            // Act.
            _cache.TryGet(key, out result);

            // Assert.
            Assert.Equal(value, result);

            // Act.
            _cache.TryGet(caseSensitiveKey, out result);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void SetAlwaysOverwritesExistingObject()
        {
            // Arrange.
            var key = "testing-key";
            int? oldValue = 1;
            int? newValue = 2;
            int? result;

            _cache.Set(key, oldValue, _options);

            // Ensure pre-condition.
            _cache.TryGet(key, out result);
            Assert.Equal(oldValue, result);

            // Act.
            _cache.Set(key, newValue, _options);

            // Assert.
            _cache.TryGet(key, out result);
            Assert.Equal(newValue, result);
        }

        [Fact]
        public void RemoveRemovesPassedKey()
        {
            // Arrange.
            var key = "testing-key";
            int? value = 1;
            int? result;
            bool found;

            _cache.Set(key, value, _options);

            // Ensure pre-condition.
            found = _cache.TryGet(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            // Act.
            _cache.Remove(key);

            // Assert.
            found = _cache.TryGet(key, out result);
            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void KeyExpiredWhenRedisServerSystemClockIsNotTheSameAsWebServer()
        {
            // Arrange.
            var key = "testing-key";
            int? value = 1;
            int? result;
            bool found;

            _options.SetAbsoluteExpiration(new SystemClockMock().UtcNow + TimeSpan.FromSeconds(50));
            _cache.Set(key, value, _options);

            // Act.
            found = _cache.TryGet(key, out result);
            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void KeyIsRemovedWhenAbsoluteExpirationExpires()
        {
            // Arrange.
            var key = "testing-key";
            int? value = 1;
            int? result;
            bool found;

            _options.SetAbsoluteExpiration(DateTimeOffset.Now + TimeSpan.FromSeconds(5));
            _cache.Set(key, value, _options);

            // Ensure pre-condition.
            found = _cache.TryGet(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            // Act.
            Thread.Sleep(TimeSpan.FromSeconds(5));
            found = _cache.TryGet(key, out result);

            // Assert.
            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void KeyIsRemovedWhenRelativeExpirationExpires()
        {
            // Arrange.
            var key = "testing-key";
            int? value = 1;
            int? result;
            bool found;

            _options.SetAbsoluteExpiration(TimeSpan.FromSeconds(5));
            _cache.Set(key, value, _options);

            // Ensure pre-condition.
            found = _cache.TryGet(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            // Act.
            Thread.Sleep(TimeSpan.FromSeconds(5));
            found = _cache.TryGet(key, out result);

            // Assert.
            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void SlidingExpirationExpiresIfNotAccessed()
        {
            // Arrange.
            var key = "testing-key";
            int? value = 1;
            int? result;
            bool found;

            _options.SetSlidingExpiration(TimeSpan.FromSeconds(1));
            _cache.Set(key, value, _options);

            // Ensure pre-condition.
            found = _cache.TryGet(key, out result);
            Assert.True(found);
            Assert.Equal(value, result);

            for (int index = 0; index < 5; index++)
            {
                // Act.
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                found = _cache.TryGet(key, out result);

                // Assert.
                Assert.True(found);
                Assert.Equal(value, result);
            }

            // Verify key expiration.
            Thread.Sleep(TimeSpan.FromSeconds(3));
            found = _cache.TryGet(key, out result);

            Assert.False(found);
            Assert.Null(result);
        }
    }
}