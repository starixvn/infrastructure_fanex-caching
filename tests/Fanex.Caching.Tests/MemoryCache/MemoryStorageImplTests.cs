namespace Fanex.Caching.Tests.MemoryCache
{
    using System;
    using System.Threading;
    using Memory;
    using Mocks;
    using Xunit;

    public partial class MemoryStorageImplTests
    {
        private MemoryCacheOptions _options;
        private SystemClockMock _clock;
        private MemoryStorageImpl _storage;
        private MemoryCacheItemOptions _cachingOptions;

        public MemoryStorageImplTests()
        {
            _options = new MemoryCacheOptions();
            _clock = new SystemClockMock();
            _storage = new MemoryStorageImpl(_options, _clock);
            _cachingOptions = new MemoryCacheItemOptions();
        }

        [Fact]
        public void TryGetMissingKeyReturnsFalse()
        {
            // Arrange.
            var key = "cachingObject";
            object result = null;

            // Act.
            bool found = _storage.TryGet(key, out result);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetMissingKeyReturnsNullAsOutput()
        {
            // Arrange.
            var key = "cachingObject";
            object result = null;

            // Act.
            bool found = _storage.TryGet(key, out result);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void TryGetCanPickObjectStoredBySet()
        {
            // Arrange.
            var key = "cachingObject";
            var value = new object();

            _storage.Set(key, value, _cachingOptions);

            // Act.
            object result = null;
            bool found = _storage.TryGet(key, out result);

            // Assert.
            Assert.True(found);
            Assert.Same(value, result);
        }

        [Fact]
        public void SetAndTryGetWorkWithCaseSensitiveKeys()
        {
            // Arrange.
            var key = "cachingObject";
            var caseSensitiveKey = "cachingobject";
            var value = new object();

            // Act.
            object result = null;
            bool found = false;

            _storage.Set(key, value, _cachingOptions);
            found = _storage.TryGet(caseSensitiveKey, out result);

            // Assert.
            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void SetAlwaysOverwritesExistingObject()
        {
            // Arrange.
            var key = "cachingObject";
            var oldValue = new object();
            var newValue = new object();

            // Act.
            _storage.Set(key, oldValue, _cachingOptions);
            _storage.Set(key, newValue, _cachingOptions);

            // Assert.
            object result = null;

            _storage.TryGet(key, out result);
            Assert.Same(newValue, result);
        }

        [Fact]
        public void SetOverwritesKeyAndInvokesCallback()
        {
            // Arrange.
            string key = "cachingKey";
            var oldValue = new object();
            var oldValueCallbackInvoked = new ManualResetEvent(false);
            var oldValueCachingOptions = new MemoryCacheItemOptions();
            var newValue = new object();
            var newValueCallbackInvoked = new ManualResetEvent(false);
            var newValueCachingOptions = new MemoryCacheItemOptions();

            oldValueCachingOptions.RegisterPostEvictionCallback(
                (subKey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    Assert.Equal(key, subKey);
                    Assert.Same(subValue, oldValue);
                    Assert.Equal(EvictionReason.Replaced, reason);

                    localCallbackInvoked.Set();
                },
                oldValueCallbackInvoked);
            newValueCachingOptions.RegisterPostEvictionCallback(
                (subKey, subValue, reason, state) =>
                {
                    // Shouldn't be invoked.
                    var localCallbackInvoked = (ManualResetEvent)state;

                    localCallbackInvoked.Set();
                },
                newValueCallbackInvoked);

            // Act.
            _storage.Set(key, oldValue, oldValueCachingOptions);
            _storage.Set(key, newValue, newValueCachingOptions);

            // Assert.
            Assert.True(oldValueCallbackInvoked.WaitOne(TimeSpan.FromSeconds(30)), "Callback1");
            Assert.False(newValueCallbackInvoked.WaitOne(TimeSpan.FromSeconds(1)), "Callback2");

            object result = null;
            bool found = _storage.TryGet(key, out result);

            Assert.Same(newValue, result);
        }

        [Fact]
        public void SetOverwritesKeyWithReplacedReason()
        {
            // Arrange.
            string key = "cachingKey";
            var oldValue = new object();
            var newValue = new object();
            var callbackInvoked = new ManualResetEvent(false);
            EvictionReason actualReason = EvictionReason.None;

            _cachingOptions.RegisterPostEvictionCallback(
                (subKey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    actualReason = reason;
                    localCallbackInvoked.Set();
                },
                callbackInvoked);
            _storage.Set(key, oldValue, _cachingOptions);

            // Act.
            _storage.Set(key, newValue, new MemoryCacheItemOptions());

            // Assert.
            Assert.True(callbackInvoked.WaitOne(TimeSpan.FromSeconds(3)), "Callback");
            Assert.Equal(EvictionReason.Replaced, actualReason);
        }

        [Fact]
        public void RemoveRemovesPassedKey()
        {
            // Arrange.
            string key = "cachingKey";
            var value = new object();

            _storage.Set(key, value, _cachingOptions);

            // Act.
            _storage.Remove(key);

            // Assert.
            object result = null;
            bool found = _storage.TryGet(key, out result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void RemoveRemovesObjectAndInvokesCallback()
        {
            // Arrange.
            string key = "cachingKey";
            var value = new object();
            var callbackInvoked = new ManualResetEvent(false);

            _cachingOptions.RegisterPostEvictionCallback(
                (subKey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    Assert.Equal(key, subKey);
                    Assert.Same(value, subValue);
                    Assert.Equal(EvictionReason.Removed, reason);

                    localCallbackInvoked.Set();
                },
                callbackInvoked);
            _storage.Set(key, value, _cachingOptions);

            // Act.
            _storage.Remove(key);

            // Assert.
            Assert.True(callbackInvoked.WaitOne(TimeSpan.FromSeconds(30)), "Callback");

            object result = null;
            bool found = _storage.TryGet(key, out result);

            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void RemoveRemovesOldObjectAndCallbackReAddsNewObject()
        {
            // Arrange.
            string key = "cachingKey";
            var oldValue = new object();
            var newValue = new object();
            var callbackInvoked = new ManualResetEvent(false);

            _cachingOptions.RegisterPostEvictionCallback(
                (subKey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    Assert.Equal(key, subKey);
                    Assert.Same(subValue, oldValue);
                    Assert.Equal(EvictionReason.Removed, reason);

                    _storage.Set(key, newValue, new MemoryCacheItemOptions());
                    localCallbackInvoked.Set();
                },
                callbackInvoked);
            _storage.Set(key, oldValue, _cachingOptions);

            // Act.
            _storage.Remove(key);

            // Assert.
            Assert.True(callbackInvoked.WaitOne(TimeSpan.FromSeconds(30)), "Callback");

            object result = null;
            bool found = _storage.TryGet(key, out result);

            Assert.True(found);
            Assert.Same(newValue, result);
        }
    }
}