namespace Fanex.Caching.Tests.Memory
{
    using System;
    using System.Threading;
    using AutoFixture;
    using Fanex.Caching.Memory;
    using Xunit;

    public sealed class TimeExpirationTests : IDisposable
    {
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
            var absoluteExpiration = _clock.UtcNow + TimeSpan.FromMinutes(1);

            _cacheOptions.SetAbsoluteExpiration(absoluteExpiration);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Assert.
            _clock.Add(TimeSpan.FromMinutes(2));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void RemoveObjectWhenRelativeExpirationMet()
        {
            // Arrange.
            var relativeExpiration = TimeSpan.FromMinutes(1);

            _cacheOptions.SetAbsoluteExpiration(relativeExpiration);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Assert.
            _clock.Add(TimeSpan.FromMinutes(2));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeysExpiresInBackground()
        {
            // Arrange.
            var relativeExpiration = TimeSpan.FromMinutes(1);

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
                .SetAbsoluteExpiration(relativeExpiration);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Ensure testing key was added successfully.
            Assert.True(_storage.TryGet(_testingKey, out _));

            // Act - Background expiration checks are triggered when TryGet is called no matter what the key is.
            var otherKey = _fixture.Create<string>();

            _clock.Add(TimeSpan.FromMinutes(2));
            _storage.TryGet(otherKey, out _);

            // Assert.
            Assert.True(_callbackInvoked.Wait(TimeSpan.FromSeconds(30)));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void ExpirationScanHideExceptionThrownInCallback()
        {
            // Arrange.
            var relativeExpiration = TimeSpan.FromMinutes(2);

            _cacheOptions
                .RegisterPostEvictionCallback(
                    (key, value, reason, state) =>
                    {
                        var localCallbackInvoked = (ManualResetEventSlim)state;

                        localCallbackInvoked.Set();
                        throw new InvalidOperationException();
                    },
                    _callbackInvoked)
                .SetAbsoluteExpiration(relativeExpiration);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Act.
            var otherKey = _fixture.Create<string>();

            _clock.Add(TimeSpan.FromMinutes(3));
            _storage.TryGet(otherKey, out _);

            // Assert.
            Assert.True(_callbackInvoked.Wait(TimeSpan.FromSeconds(30)));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeyWithSlidingExpirationExpiresIfNotTouched()
        {
            // Arrange.
            var slidingExpiration = TimeSpan.FromMinutes(1);

            _cacheOptions.SetSlidingExpiration(slidingExpiration);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Assert.
            _clock.Add(TimeSpan.FromMinutes(2));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeyWithSlidingExpirationRenewedOnceTouched()
        {
            // Arrange.
            var slidingExpiration = TimeSpan.FromMinutes(1);

            _cacheOptions.SetSlidingExpiration(slidingExpiration);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Act - after 10 seconds.
            _clock.Add(TimeSpan.FromSeconds(10));
            _storage.TryGet(_testingKey, out _);

            // Assert - after 1 minute from last touch.
            _clock.Add(TimeSpan.FromSeconds(59));
            Assert.True(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void SlidingExpirationRenewedByTouchingUntilAbsoluteExpirationMet()
        {
            // Arrange.
            var slidingExpiration = TimeSpan.FromMinutes(1);
            var relativeExpiration = TimeSpan.FromMinutes(2);

            _cacheOptions
                .SetSlidingExpiration(slidingExpiration)
                .SetAbsoluteExpiration(relativeExpiration);
            _storage.Set(_testingKey, _testingValue, _cacheOptions);

            // Act - touch the key when the absolute expiration has not met.
            _clock.Add(TimeSpan.FromSeconds(59));
            Assert.True(_storage.TryGet(_testingKey, out _));

            // Assert - after absolute timeout passed at 2 minutes and 1 second.
            _clock.Add(TimeSpan.FromSeconds(61));
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