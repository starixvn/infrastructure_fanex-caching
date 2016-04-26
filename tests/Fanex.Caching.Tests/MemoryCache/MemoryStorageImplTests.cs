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
        private SystemClock _clock;
        private MemoryStorage _storage;
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
            var obj = new object();
            _storage.Set(key, obj, _cachingOptions);

            // Act.
            object result = null;
            _storage.TryGet(key, out result);

            // Assert.
            Assert.Same(obj, result);
        }

        [Fact]
        public void SetAndTryGetWorksWithCaseSensitiveKeys()
        {
            // Arrange.
            var key = "cachingObject";
            var caseSensitiveKey = "cachingobject";
            var obj = new object();

            // Act.
            _storage.Set(key, obj, _cachingOptions);
            object result = null;
            bool found = _storage.TryGet(caseSensitiveKey, out result);

            // Assert.
            Assert.False(found);
            Assert.Null(result);
        }

        [Fact]
        public void SetAlwaysOverwritesExistingObject()
        {
            // Arrange.
            var key = "cachingObject";
            var firstObj = new object();
            var secondObj = new object();

            // Act.
            _storage.Set(key, firstObj, _cachingOptions);
            _storage.Set(key, secondObj, _cachingOptions);

            // Assert.
            object result = null;
            _storage.TryGet(key, out result);
            Assert.Same(secondObj, result);
        }

        [Fact]
        public void SetOverwritesAndInvokesCallback()
        {
            // Arrange.
            string key = "cachingKey";
            var value1 = new object();
            var callback1Invoked = new ManualResetEvent(false);
            var cachingOptions1 = new MemoryCacheItemOptions();
            cachingOptions1.RegisterPostEvictionCallback(
                (subkey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    Assert.Equal(key, subkey);
                    Assert.Same(subValue, value1);
                    Assert.Equal(EvictionReason.Replaced, reason);

                    localCallbackInvoked.Set();
                },
                callback1Invoked);
            _storage.Set(key, value1, cachingOptions1);

            // Act.
            var value2 = new object();
            var callback2Invoked = new ManualResetEvent(false);
            var cachingOptions2 = new MemoryCacheItemOptions();
            cachingOptions2.RegisterPostEvictionCallback(
                (subkey, subValue, reason, state) =>
                {
                    // Shouldn't be invoked.
                    var localCallbackInvoked = (ManualResetEvent)state;

                    localCallbackInvoked.Set();
                },
                callback2Invoked);
            _storage.Set(key, value2, cachingOptions2);

            // Assert.
            Assert.True(callback1Invoked.WaitOne(TimeSpan.FromSeconds(30)), "Callback1");
            Assert.False(callback2Invoked.WaitOne(TimeSpan.FromSeconds(1)), "Callback2");

            object result = null;
            bool found = _storage.TryGet(key, out result);
            Assert.Same(value2, result);
            Assert.False(callback2Invoked.WaitOne(TimeSpan.FromSeconds(1)), "Callback2");
        }

        [Fact]
        public void SetOverwritesWithReplacedReason()
        {
            // Arrange.
            string key = "cachingKey";
            var value1 = new object();
            var value2 = new object();
            var callback1Invoked = new ManualResetEvent(false);
            EvictionReason actualReason = EvictionReason.None;

            _cachingOptions.RegisterPostEvictionCallback(
                (subkey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    actualReason = reason;
                    localCallbackInvoked.Set();
                },
                callback1Invoked);
            _storage.Set(key, value1, _cachingOptions);

            // Act.
            _storage.Set(key, value1, new MemoryCacheItemOptions());

            Assert.True(callback1Invoked.WaitOne(TimeSpan.FromSeconds(3)), "Callback1");
            Assert.Equal(EvictionReason.Replaced, actualReason);
        }

        [Fact]
        public void RemoveRemoves()
        {
            // Arrange.
            string key = "cachingKey";
            var obj = new object();
            _storage.Set(key, obj, _cachingOptions);

            // Act.
            _storage.Remove(key);

            // Assert.
            object result = null;
            bool found = _storage.TryGet(key, out result);
            Assert.False(found);
        }

        [Fact]
        public void RemoveRemovesAndInvokesCallback()
        {
            // Arrange.
            string key = "cachingKey";
            var value = new object();
            var callbackInvoked = new ManualResetEvent(false);
            _cachingOptions.RegisterPostEvictionCallback(
                (subkey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    Assert.Equal(key, subkey);
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
        }

        [Fact]
        public void RemoveAndReAddFromCallbackWorks()
        {
            // Arrange.
            string key = "cachingKey";
            var value = new object();
            var obj2 = new object();
            var callbackInvoked = new ManualResetEvent(false);
            _cachingOptions.RegisterPostEvictionCallback(
                (subkey, subValue, reason, state) =>
                {
                    var localCallbackInvoked = (ManualResetEvent)state;

                    Assert.Equal(key, subkey);
                    Assert.Same(subValue, value);
                    Assert.Equal(EvictionReason.Removed, reason);

                    _storage.Set(key, obj2, new MemoryCacheItemOptions());
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
            Assert.Same(obj2, result);
        }
    }
}