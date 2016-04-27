namespace Fanex.Caching.Tests.MemoryCache
{
    using System;
    using System.Threading;
    using Memory;
    using Xunit;

    public partial class MemoryStorageImplTests
    {
        [Fact]
        public void RemoveObjectWhenAbsoluteExpirationExpires()
        {
            // Arrange.
            var key = "myKey";
            var value = new object();
            bool found = false;
            object addedValue = null;

            _cachingOptions.SetAbsoluteExpiration(_clock.UtcNow + TimeSpan.FromMinutes(1));
            _storage.Set(key, value, _cachingOptions);
            found = _storage.TryGet(key, out addedValue);

            Assert.True(found);
            Assert.Same(value, addedValue);

            // Act.
            _clock.Add(TimeSpan.FromMinutes(2));

            // Assert.
            found = _storage.TryGet(key, out addedValue);
            Assert.False(found);
            Assert.Null(addedValue);
        }

        [Fact]
        public void RemoveObjectWhenRelativeExpirationExpires()
        {
            // Arrange.
            var key = "myKey";
            var value = new object();
            bool found = false;
            object addedValue = null;

            _cachingOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            _storage.Set(key, value, _cachingOptions);
            found = _storage.TryGet(key, out addedValue);

            Assert.True(found);
            Assert.Same(value, addedValue);

            // Act.
            _clock.Add(TimeSpan.FromMinutes(2));

            // Assert.
            found = _storage.TryGet(key, out addedValue);
            Assert.False(found);
            Assert.Null(addedValue);
        }

        [Fact]
        public void ExpirationExpiresInBackground()
        {
            // Arrange.
            var key = "myKey";
            var value = new object();
            var callbackInvoked = new ManualResetEvent(false);
            bool found = false;
            object addedValue = null;

            _cachingOptions
                .RegisterPostEvictionCallback(
                    (subkey, subValue, reason, state) =>
                    {
                        var localCallbackInvoked = (ManualResetEvent)state;

                        Assert.Equal(key, subkey);
                        Assert.Same(subValue, value);
                        Assert.Equal(EvictionReason.Expired, reason);

                        localCallbackInvoked.Set();
                    },
                    callbackInvoked)
                .SetAbsoluteExpiration(_clock.UtcNow + TimeSpan.FromMinutes(1));

            _storage.Set(key, value, _cachingOptions);
            found = _storage.TryGet(key, out addedValue);
            Assert.True(found);
            Assert.Same(value, addedValue);

            // Act.
            // Background expiration checks are triggered when TryGet is called no matter what the key is.
            object otherValue = null;

            _clock.Add(TimeSpan.FromMinutes(2));
            _storage.TryGet("otherKey", out otherValue);

            // Assert.
            Assert.True(callbackInvoked.WaitOne(TimeSpan.FromSeconds(30)), "Callback");

            found = _storage.TryGet(key, out addedValue);

            Assert.False(found);
            Assert.Null(addedValue);
        }

        [Fact]
        public void KeyWithSlidingExpirationExpiresIfNotTouched()
        {
            // Arrange.
            var key = "myKey";
            var value = new object();
            bool found = false;
            object addedValue = null;

            _cachingOptions.SetSlidingExpiration(TimeSpan.FromMinutes(1));
            _storage.Set(key, value, _cachingOptions);
            found = _storage.TryGet(key, out addedValue);

            Assert.True(found);
            Assert.Same(value, addedValue);

            // Act.
            _clock.Add(TimeSpan.FromMinutes(2));

            // Assert.
            found = _storage.TryGet(key, out addedValue);

            Assert.False(found);
            Assert.Null(addedValue);
        }

        [Fact]
        public void KeyWithSlidingExpirationRenewOnceTouched()
        {
            // Arrange.
            var key = "myKey";
            var value = new object();
            bool found = false;
            object addedValue = null;

            _cachingOptions.SetSlidingExpiration(TimeSpan.FromMinutes(1));
            _storage.Set(key, value, _cachingOptions);
            found = _storage.TryGet(key, out addedValue);

            Assert.True(found);
            Assert.Same(value, addedValue);

            // Act - after 10 second.
            _clock.Add(TimeSpan.FromSeconds(10));

            // Assert.
            found = _storage.TryGet(key, out addedValue);

            Assert.True(found);
            Assert.Equal(value, addedValue);

            // Act - after 1 minute from last touch.
            _clock.Add(TimeSpan.FromSeconds(59));

            // Assert.
            found = _storage.TryGet(key, out addedValue);

            Assert.True(found);
            Assert.Equal(value, addedValue);
        }

        [Fact]
        public void SlidingExpirationRenewedByThouchUntilAbsoluteExpirationMet()
        {
            // Arrange.
            var key = "myKey";
            var value = new object();
            object result = null;
            bool found = false;

            _cachingOptions.SetSlidingExpiration(TimeSpan.FromMinutes(1))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            _storage.Set(key, value, _cachingOptions);

            // Act - touch the key when the absolute expiration has not met.
            _clock.Add(TimeSpan.FromSeconds(59));
            found = _storage.TryGet(key, out result);

            // Assert.
            Assert.True(found);
            Assert.Same(value, result);

            // Act - after 2 minutes from the time the key was stored.
            _clock.Add(TimeSpan.FromSeconds(61));
            found = _storage.TryGet(key, out result);

            // Assert.
            Assert.False(found);
            Assert.Null(result);
        }
    }
}