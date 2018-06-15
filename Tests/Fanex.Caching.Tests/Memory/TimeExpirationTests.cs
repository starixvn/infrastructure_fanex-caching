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
        private readonly MemoryStorage _storage;
        private readonly MemoryCacheItemOptions _cachingOptions;

        private readonly string _testingKey;
        private readonly object _testingValue;
        private readonly ManualResetEventSlim _callbackInvoked;

        public TimeExpirationTests()
        {
            _fixture = new Fixture();
            _clock = new SystemClockMock();
            _cachingOptions = new MemoryCacheItemOptions();
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

            _cachingOptions.SetAbsoluteExpiration(absoluteExpiration);
            _storage.Set(_testingKey, _testingValue, _cachingOptions);

            // Assert.
            _clock.Add(TimeSpan.FromMinutes(2));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void RemoveObjectWhenRelativeExpirationMet()
        {
            // Arrange.
            var relativeExpiration = TimeSpan.FromMinutes(1);

            _cachingOptions.SetAbsoluteExpiration(relativeExpiration);
            _storage.Set(_testingKey, _testingValue, _cachingOptions);

            // Assert.
            _clock.Add(TimeSpan.FromMinutes(2));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeysExpiresInBackground()
        {
            // Arrange.
            var relativeExpiration = TimeSpan.FromMinutes(1);

            _cachingOptions
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
            _storage.Set(_testingKey, _testingValue, _cachingOptions);

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
            var relativeExpiration = TimeSpan.FromMinutes(1);

            _cachingOptions
                .RegisterPostEvictionCallback(
                    (key, value, reason, state) =>
                    {
                        var localCallbackInvoked = (ManualResetEventSlim)state;

                        localCallbackInvoked.Set();
                        throw new InvalidOperationException();
                    },
                    _callbackInvoked)
                .SetAbsoluteExpiration(relativeExpiration);
            _storage.Set(_testingKey, _testingValue, _cachingOptions);

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
        public void KeyWithSlidingExpirationExpiresIfNotTouched()
        {
            // Arrange.
            var slidingExpiration = TimeSpan.FromMinutes(1);

            _cachingOptions.SetSlidingExpiration(slidingExpiration);
            _storage.Set(_testingKey, _testingValue, _cachingOptions);

            // Assert.
            _clock.Add(TimeSpan.FromMinutes(2));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        [Fact]
        public void KeyWithSlidingExpirationRenewedOnceTouched()
        {
            // Arrange.
            var slidingExpiration = TimeSpan.FromMinutes(1);

            _cachingOptions.SetSlidingExpiration(slidingExpiration);
            _storage.Set(_testingKey, _testingValue, _cachingOptions);

            // Act - after 10 second.
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

            _cachingOptions
                .SetSlidingExpiration(slidingExpiration)
                .SetAbsoluteExpiration(relativeExpiration);
            _storage.Set(_testingKey, _testingValue, _cachingOptions);

            // Act - touch the key when the absolute expiration has not met.
            _clock.Add(TimeSpan.FromSeconds(59));
            Assert.True(_storage.TryGet(_testingKey, out _));

            // Assert - after absolute timeout passed at 2 minutes and 1 second.
            _clock.Add(TimeSpan.FromSeconds(61));
            Assert.False(_storage.TryGet(_testingKey, out _));
        }

        public void Dispose()
            => _storage.Dispose();
    }
}