namespace Fanex.Caching.Tests.Memory
{
    using System;
    using System.Threading;
    using AutoFixture;
    using Fanex.Caching.Memory;
    using Xunit;

    public sealed class TimeExpirationTests : IDisposable
    {
        private static readonly TimeSpan InitialExpiry = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan TimeToExpire = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan CallbackTimeout = TimeSpan.FromSeconds(30);

        private readonly Fixture _fixture;
        private readonly SystemClockMock _clock;
        private readonly MemoryCacheItemOptions _cacheOptions;
        private readonly string _testingKey;
        private readonly object _testingValue;

        private MemoryStorage _storage;
        private ManualResetEventSlim _callbackInvoked;

        public TimeExpirationTests()
        {
            _fixture = new Fixture();
            _clock = new SystemClockMock();
            _cacheOptions = new MemoryCacheItemOptions();
            _storage = new MemoryStorage(new MemoryCacheOptions
            {
                Clock = _clock
            });
            _testingKey = _fixture.Create<string>();
            _testingValue = _fixture.Create<object>();
            _callbackInvoked = new ManualResetEventSlim(false);
        }

        [Fact]
        public void RemoveObjectWhenAbsoluteExpirationMet()
        {
            // Arrange.
            var absoluteExpiration = _clock.UtcNow + InitialExpiry;
            var absoluteExpirationPolicy = _cacheOptions.SetAbsoluteExpiration(absoluteExpiration);

            _storage.Set(_testingKey, _testingValue, absoluteExpirationPolicy);

            // Act.
            _clock.Add(TimeToExpire);

            // Assert.
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void RemoveObjectWhenRelativeExpirationMet()
        {
            // Arrange.
            var relativeExpirationPolicy = _cacheOptions.SetAbsoluteExpiration(InitialExpiry);

            _storage.Set(_testingKey, _testingValue, relativeExpirationPolicy);

            // Act.
            _clock.Add(TimeToExpire);

            // Assert.
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeysExpiresInBackground()
        {
            // Arrange.
            _cacheOptions
                .RegisterPostEvictionCallback(
                    (key, value, reason, state) =>
                    {
                        var localCallbackInvoked = (ManualResetEventSlim)state;

                        // Verify whether received parameters of callback are correct or not.
                        Assert.Equal(_testingKey, key);
                        Assert.Same(_testingValue, value);
                        Assert.Equal(EvictionReason.Expired, reason);

                        localCallbackInvoked.Set();
                    },
                    _callbackInvoked)
                .SetAbsoluteExpiration(InitialExpiry);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Ensure testing key was added successfully.
            Assert.True(_storage.TryGet(_testingKey, out _));

            // Act - Background expiration checks are triggered when TryGet is called no matter what the key is.
            var otherKey = _fixture.Create<string>();

            _clock.Add(TimeToExpire);
            _storage.TryGet(otherKey, out _);

            // Assert.
            Assert.True(_callbackInvoked.Wait(CallbackTimeout));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void ExpirationScanHideExceptionThrownInCallback()
        {
            // Arrange.
            _cacheOptions
                .RegisterPostEvictionCallback(
                    (key, value, reason, state) =>
                    {
                        var localCallbackInvoked = (ManualResetEventSlim)state;

                        localCallbackInvoked.Set();
                        throw new InvalidOperationException();
                    },
                    _callbackInvoked)
                .SetAbsoluteExpiration(InitialExpiry);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Act.
            var otherKey = _fixture.Create<string>();

            _clock.Add(TimeToExpire);
            _storage.TryGet(otherKey, out _);

            // Assert.
            Assert.True(_callbackInvoked.Wait(CallbackTimeout));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeyWithSlidingExpirationExpiresIfNotTouched()
        {
            // Arrange.
            var sliddingExpirationPolicy = _cacheOptions.SetSlidingExpiration(InitialExpiry);

            _storage.Set(_testingKey, _testingValue, sliddingExpirationPolicy);

            // Act.
            _clock.Add(TimeToExpire);

            // Assert.
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeyWithSlidingExpirationRenewedOnceTouched()
        {
            // Arrange.
            var sliddingExpirationPolicy = _cacheOptions.SetSlidingExpiration(InitialExpiry);

            _storage.Set(_testingKey, _testingValue, sliddingExpirationPolicy);

            // Act - after 10 seconds.
            var firstTouchWhenKeyLivesWell = TimeSpan.FromSeconds(10);

            _clock.Add(firstTouchWhenKeyLivesWell);
            _storage.TryGet(_testingKey, out _);

            // Assert - after nearly 1 minute from last touch.
            var secondTouchWhenKeyNearlyExpired = TimeSpan.FromSeconds(59);

            _clock.Add(secondTouchWhenKeyNearlyExpired);
            Assert.True(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void SlidingExpirationRenewedByTouchingUntilAbsoluteExpirationMet()
        {
            // Arrange.
            var slidingExpiration = InitialExpiry;
            var relativeExpiration = InitialExpiry + TimeSpan.FromMinutes(1);
            var mixedExpirationPolicy = _cacheOptions
                .SetSlidingExpiration(slidingExpiration)
                .SetAbsoluteExpiration(relativeExpiration);

            _storage.Set(_testingKey, _testingValue, mixedExpirationPolicy);

            // Act - touch the key when the absolute expiration has not met.
            var firstTouchWhenSlidingExpirationNearlyMet = TimeSpan.FromSeconds(59);

            _clock.Add(firstTouchWhenSlidingExpirationNearlyMet);
            Assert.True(_storage.TryGet(_testingKey, out _));

            // Assert - after absolute timeout passed at 2 minutes and 1 second.
            var secondTouchWhenAbsoluteExpirationOvers = TimeSpan.FromSeconds(62);

            _clock.Add(secondTouchWhenAbsoluteExpirationOvers);
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        public void Dispose()
        {
            _storage.Dispose();
            _storage = null;
            _callbackInvoked.Dispose();
            _callbackInvoked = null;
        }
    }
}