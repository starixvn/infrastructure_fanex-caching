namespace Fanex.Caching.Redis.Tests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Xunit;

    public sealed class TimeExpirationTests : IClassFixture<RedisCacheFixture>, IDisposable
    {
        private const int NumberOfMillisecondsPassedForKeyToBeExpired = 50;

        private static readonly TimeSpan InitialExpiry = TimeSpan.FromMilliseconds(30);

        private readonly string _testingKey;
        private readonly object _testingValue;
        private readonly CacheItemOptions _cacheOptions;

        public TimeExpirationTests(RedisCacheFixture cacheFixture)
        {
            var fixture = new Fixture();

            _testingKey = fixture.Create<string>();
            _testingValue = fixture.Create<string>();
            _cacheOptions = new CacheItemOptions();

            Clock = cacheFixture.CacheOptions.Clock;
            Storage = cacheFixture.Cache.RedisStorage;
        }

        private RedisStorage Storage { get; }

        private ISystemClock Clock { get; }

        [Fact]
        public async Task RemoveKeyWhenAbsoluteExpirationMet()
        {
            // Arrange.
            var dateTimeToExpire = Clock.UtcNow + InitialExpiry;
            var absoluteExpirationPolicy = _cacheOptions.SetAbsoluteExpiration(dateTimeToExpire);

            await Storage.SetAsync(_testingKey, _testingValue, absoluteExpirationPolicy);

            // Act.
            await Task.Delay(NumberOfMillisecondsPassedForKeyToBeExpired);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_testingKey);

            Assert.False(found);
        }

        [Fact]
        public async Task RemoveKeyWhenRelativeExpirationMet()
        {
            // Arrange.
            var relativeExpirationPolicy = _cacheOptions.SetAbsoluteExpiration(InitialExpiry);

            await Storage.SetAsync(_testingKey, _testingValue, relativeExpirationPolicy);

            // Act.
            await Task.Delay(NumberOfMillisecondsPassedForKeyToBeExpired);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_testingKey);

            Assert.False(found);
        }

        [Fact]
        public async Task KeyWithSlidingExpirationExpiresIfNotTouched()
        {
            // Arrange.
            var sliddingExpirationPolicy = _cacheOptions.SetSlidingExpiration(InitialExpiry);

            await Storage.SetAsync(_testingKey, _testingValue, sliddingExpirationPolicy);

            // Act.
            await Task.Delay(NumberOfMillisecondsPassedForKeyToBeExpired);

            // Assert.
            var (found, _) = await Storage.TryGetAsync<string>(_testingKey);

            Assert.False(found);
        }

        [Fact]
        public async Task KeyWithSlidingExpirationRenewedOnceTouched()
        {
            // Arrange.
            var timeToSlide = TimeSpan.FromMilliseconds(500);
            var sliddingExpirationPolicy = _cacheOptions.SetSlidingExpiration(timeToSlide);

            await Storage.SetAsync(_testingKey, _testingValue, sliddingExpirationPolicy);

            // Act - after 200 milliseconds.
            const int numberOfMillisecondsPassedToMakeFirstTouch = 200;

            await Task.Delay(numberOfMillisecondsPassedToMakeFirstTouch);
            await Storage.TryGetAsync<string>(_testingKey);

            // Assert - after 110 milliseconds from last touch, 510 milliseconds since moment the key was stored.
            const int numberOfMillisecondsPassedToMakeSecondTouch = 310;

            await Task.Delay(numberOfMillisecondsPassedToMakeSecondTouch);

            var (found, _) = await Storage.TryGetAsync<string>(_testingKey);

            Assert.True(found);
        }

        [Fact]
        public async Task SlidingExpirationRenewedByTouchingUntilAbsoluteExpirationMet()
        {
            // Arrange.
            var timeToSlide = TimeSpan.FromMilliseconds(500);
            var timeToLive = TimeSpan.FromMilliseconds(800);
            var mixedExpirationPolicy = _cacheOptions
                .SetSlidingExpiration(timeToSlide)
                .SetAbsoluteExpiration(timeToLive);

            await Storage.SetAsync(_testingKey, _testingValue, mixedExpirationPolicy);

            // Act - touch the key when the sliding expiration has not met.
            const int numberOfMillisecondsWhenSlidingExpirationNearlyMet = 350;

            await Task.Delay(numberOfMillisecondsWhenSlidingExpirationNearlyMet);

            var (found, _) = await Storage.TryGetAsync<string>(_testingKey);

            Assert.True(found);

            // Act - touch the key when the absolute expiration has not met.
            const int numberOfMillisecondsWhenAbsoluteExpirationStillNotMet = 200;

            await Task.Delay(numberOfMillisecondsWhenAbsoluteExpirationStillNotMet);
            (found, _) = await Storage.TryGetAsync<string>(_testingKey);
            Assert.True(found);

            // Assert - after absolute timeout passed at 810 milliseconds since moment the key was stored.
            const int numberOfMillisecondsWhenAbsoluteExpirationOvers = 260;

            await Task.Delay(numberOfMillisecondsWhenAbsoluteExpirationOvers);
            (found, _) = await Storage.TryGetAsync<string>(_testingKey);
            Assert.False(found);
        }

        public void Dispose()
            => Storage.Remove(_testingKey);
    }
}