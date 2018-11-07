namespace Fanex.Caching.Redis.Tests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public sealed class TimeExpirationTests : IClassFixture<RedisCacheFixture>, IDisposable
    {
        private readonly string _testingKey;
        private readonly object _testingValue;

        public TimeExpirationTests(RedisCacheFixture cacheFixture)
        {
            var fixture = new Fixture();

            _testingKey = fixture.Create<string>();
            _testingValue = fixture.Create<string>();

            Clock = cacheFixture.CacheOptions.Clock;
            Storage = cacheFixture.Cache.RedisStorage;
        }

        private RedisStorage Storage { get; }

        private ISystemClock Clock { get; }

        [Fact]
        public async Task RemoveKeyWhenAbsoluteExpirationMet()
        {
            // Arrange.
            var dateTimeToExpire = Clock.UtcNow + TimeSpan.FromMilliseconds(30);
            var absoluteExpirationPolicy = new CacheItemOptions().SetAbsoluteExpiration(dateTimeToExpire);

            await Storage.SetAsync(_testingKey, _testingValue, absoluteExpirationPolicy).ConfigureAwait(false);

            // Act.
            await Task.Delay(50).ConfigureAwait(false);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);

            Assert.False(found);
        }

        [Fact]
        public async Task RemoveKeyWhenRelativeExpirationMet()
        {
            // Arrange.
            var timeToLive = TimeSpan.FromMilliseconds(30);
            var relativeExpirationPolicy = new CacheItemOptions().SetAbsoluteExpiration(timeToLive);

            await Storage.SetAsync(_testingKey, _testingValue, relativeExpirationPolicy).ConfigureAwait(false);

            // Act.
            await Task.Delay(50).ConfigureAwait(false);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);

            Assert.False(found);
        }

        [Fact]
        public async Task KeyWithSlidingExpirationExpiresIfNotTouched()
        {
            // Arrange.
            var timeToSlide = TimeSpan.FromMilliseconds(30);
            var sliddingExpirationPolicy = new CacheItemOptions().SetSlidingExpiration(timeToSlide);

            await Storage.SetAsync(_testingKey, _testingValue, sliddingExpirationPolicy).ConfigureAwait(false);

            // Act.
            await Task.Delay(50).ConfigureAwait(false);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);

            Assert.False(found);
        }

        [Fact]
        public async Task KeyWithSlidingExpirationRenewedOnceTouched()
        {
            // Arrange.
            var timeToSlide = TimeSpan.FromMilliseconds(200);
            var sliddingExpirationPolicy = new CacheItemOptions().SetSlidingExpiration(timeToSlide);

            await Storage.SetAsync(_testingKey, _testingValue, sliddingExpirationPolicy).ConfigureAwait(false);

            // Act - after 100 milliseconds.
            await Task.Delay(100).ConfigureAwait(false);
            await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);

            // Assert - after 110 milliseconds from last touch, 210 milliseconds since moment the key was stored.
            await Task.Delay(110).ConfigureAwait(false);

            var (found, _) = await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);

            Assert.True(found);
        }

        [Fact]
        public async Task SlidingExpirationRenewedByTouchingUntilAbsoluteExpirationMet()
        {
            // Arrange.
            var timeToSlide = TimeSpan.FromMilliseconds(200);
            var timeToLive = TimeSpan.FromMilliseconds(300);
            var mixedExpirationPolicy = new CacheItemOptions()
                .SetSlidingExpiration(timeToSlide)
                .SetAbsoluteExpiration(timeToLive);

            await Storage.SetAsync(_testingKey, _testingValue, mixedExpirationPolicy).ConfigureAwait(false);

            // Act - touch the key when the sliding expiration has not met.
            await Task.Delay(100).ConfigureAwait(false);

            var (found, _) = await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);

            Assert.True(found);

            // Act - touch the key when the absolute expiration has not met.
            await Task.Delay(110).ConfigureAwait(false);
            (found, _) = await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);
            Assert.True(found);

            // Assert - after absolute timeout passed at 310 milliseconds since moment the key was stored.
            await Task.Delay(100).ConfigureAwait(false);
            (found, _) = await Storage.TryGetAsync<string>(_testingKey).ConfigureAwait(false);
            Assert.False(found);
        }

        public void Dispose()
            => Storage.Remove(_testingKey);
    }
}