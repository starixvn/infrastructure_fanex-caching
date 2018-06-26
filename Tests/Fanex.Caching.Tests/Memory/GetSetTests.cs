namespace Fanex.Caching.Tests.Memory
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Fanex.Caching.Memory;
    using Xunit;

    public sealed class GetSetTests : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly SystemClockMock _clock;
        private readonly MemoryStorage _storage;
        private readonly MemoryCacheItemOptions _cachingOptions;
        private readonly ManualResetEventSlim _callbackInvoked;
        private readonly TimeSpan _callbackWaitingTime;
        private readonly string _existingKey;
        private readonly object _existingValue;

        private EvictionReason _callbackReason;

        public GetSetTests()
        {
            _fixture = new Fixture();
            _clock = new SystemClockMock();
            _cachingOptions = new MemoryCacheItemOptions();
            _storage = new MemoryStorage(new MemoryCacheOptions
            {
                Clock = _clock
            });
            _callbackInvoked = new ManualResetEventSlim(false);
            _callbackWaitingTime = TimeSpan.FromSeconds(3);
            _existingKey = _fixture.Create<string>();
            _existingValue = _fixture.Create<object>();

            _fixture.Customizations.Add(new RandomNumericSequenceGenerator(0, 10));
            _cachingOptions.RegisterPostEvictionCallback(
                (key, value, reason, state) =>
                {
                    var callbackInvoked = (ManualResetEventSlim)state;

                    Assert.Equal(_existingKey, key);
                    Assert.Same(_existingValue, value);

                    _callbackReason = reason;
                    callbackInvoked.Set();
                },
                _callbackInvoked);
            _storage.Set(_existingKey, _existingValue, _cachingOptions);
        }

        [Fact]
        public void ContainReturnsTrueForExistingKey()
        {
            // Act.
            bool found = _storage.Contain(_existingKey);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public void ContainReturnsFalseForMissingKey()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            bool found = _storage.Contain(missingKey);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetMissingKeyReturnsFalse()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            bool found = _storage.TryGet(missingKey, out _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetMissingKeyReturnsNullAsOutput()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            _storage.TryGet(missingKey, out object value);

            // Assert.
            Assert.Null(value);
        }

        [Fact]
        public void TryGetReturnsObjectStoredBySet()
        {
            // Act.
            bool found = _storage.TryGet(_existingKey, out object value);

            // Assert.
            Assert.True(found);
            Assert.Same(_existingValue, value);
        }

        [Fact]
        public void SetAndTryGetWorkWithCaseSensitiveKeys()
        {
            // Arrange.
            var caseSensitiveKey = _existingKey.ToUpperInvariant();

            // Act.
            bool found = _storage.TryGet(caseSensitiveKey, out _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void SetAlwaysOverwritesExistingObject()
        {
            // Arrange.
            var newValue = new object();

            // Act.
            _storage.Set(_existingKey, newValue, _cachingOptions);

            // Assert.
            _storage.TryGet(_existingKey, out object value);
            Assert.Same(newValue, value);
        }

        [Fact]
        public void SetOverwritesKeyAndInvokesCallbackOfPreviousSet()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();

            // Act.
            _storage.Set(_existingKey, newValue, new MemoryCacheItemOptions());

            // Assert.
            Assert.True(_callbackInvoked.Wait(_callbackWaitingTime));
        }

        [Fact]
        public void SetOverwritesKeyWithReplacedReason()
        {
            // Arrange.
            var newValue = _fixture.Create<string>();

            // Act.
            _storage.Set(_existingKey, newValue, new MemoryCacheItemOptions());

            // Assert.
            Assert.True(_callbackInvoked.Wait(_callbackWaitingTime));
            Assert.Equal(EvictionReason.Replaced, _callbackReason);
        }

        [Fact]
        public void SetExistingKeyWithValueBeingExpiredRemovesOldValue()
        {
            // Arrange.
            var newValue = new object();
            var expiredPolicy = new MemoryCacheItemOptions();

            expiredPolicy.SetAbsoluteExpiration(_clock.UtcNow);

            // Act.
            _storage.Set(_existingKey, newValue, expiredPolicy);

            // Assert.
            Assert.False(_storage.TryGet(_existingKey, out _));
        }

        [Fact]
        public void SetExistingKeyWithValueBeingExpiredInvokesCallbackOfPreviousSet()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();
            var expiredPolicy = new MemoryCacheItemOptions();

            expiredPolicy.SetAbsoluteExpiration(_clock.UtcNow);

            // Act.
            _storage.Set(_existingKey, newValue, expiredPolicy);

            // Assert.
            Assert.True(_callbackInvoked.Wait(_callbackWaitingTime));
        }

        [Fact]
        public void RemoveRemovesExistingKey()
        {
            // Act.
            _storage.Remove(_existingKey);

            // Assert.
            Assert.False(_storage.TryGet(_existingKey, out _));
        }

        [Fact]
        public void RemoveInvokesCallbackRegisteredWithKey()
        {
            // Act.
            _storage.Remove(_existingKey);

            // Assert.
            Assert.True(_callbackInvoked.Wait(_callbackWaitingTime));
            Assert.Equal(EvictionReason.Removed, _callbackReason);
        }

        [Fact]
        public void RemoveRemovesOldObjectThenCallbackReAddsNewObject()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();

            _cachingOptions.RegisterPostEvictionCallback(
                (key, value, reason, state) =>
                {
                    var callbackInvoked = (ManualResetEventSlim)state;

                    _storage.Set(key, newValue, new MemoryCacheItemOptions());
                    callbackInvoked.Set();
                },
                _callbackInvoked);
            _storage.Set(_existingKey, _existingValue, _cachingOptions);

            // Act.
            _storage.Remove(_existingKey);

            // Assert.
            Assert.True(_callbackInvoked.Wait(_callbackWaitingTime));
            Assert.True(_storage.TryGet(_existingKey, out _));
        }

#pragma warning disable S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods

        [Fact]
        public void ObjectDisposedExceptionIsThrownIfTryGetIsCalledAfterDisposingStorage()
        {
            // Arrange.
            _storage.Dispose();

            // Act.
            Exception exception = Record.Exception(() => _storage.TryGet(_existingKey, out _));

            // Assert.
            Assert.IsType<ObjectDisposedException>(exception);
        }

        [Fact]
        public void ObjectDisposedExceptionIsThrownIfTSetIsCalledAfterDisposingStorage()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();

            _storage.Dispose();

            // Act.
            Exception exception = Record.Exception(() => _storage.Set(_existingKey, newValue, _cachingOptions));

            // Assert.
            Assert.IsType<ObjectDisposedException>(exception);
        }

        [Fact]
        public void ObjectDisposedExceptionIsThrownIfTContainIsCalledAfterDisposingStorage()
        {
            // Arrange.
            _storage.Dispose();

            // Act.
            Exception exception = Record.Exception(() => _storage.Contain(_existingKey));

            // Assert.
            Assert.IsType<ObjectDisposedException>(exception);
        }

#pragma warning restore S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods

        [Fact]
        public void SetReplaceRemoveAreThreadSafe()
        {
            // Arrange.
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act.
            var firstTask = Task.Run(() => SetRandomKey(cancellationToken), cancellationToken);
            var secondTask = Task.Run(() => SetRandomKey(cancellationToken), cancellationToken);
            var thirdTask = Task.Run(() => RemoveRandomKey(cancellationToken), cancellationToken);
            var forthTask = Task.Run(() => cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(50)), cancellationToken);

            Task.WaitAll(firstTask, secondTask, thirdTask, forthTask);
            cancellationTokenSource.Dispose();

            // Assert.
            Assert.Equal(TaskStatus.RanToCompletion, firstTask.Status);
            Assert.Equal(TaskStatus.RanToCompletion, secondTask.Status);
            Assert.Equal(TaskStatus.RanToCompletion, thirdTask.Status);
            Assert.Equal(TaskStatus.RanToCompletion, forthTask.Status);
        }

        private void SetRandomKey(CancellationToken cancellationToken)
        {
            var cacheOptions = new MemoryCacheItemOptions();

            while (!cancellationToken.IsCancellationRequested)
            {
                var key = _fixture.Create<int>().ToString(CultureInfo.InvariantCulture);
                var value = _fixture.Create<object>();

                _storage.Set(key, value, cacheOptions);
            }
        }

        private void RemoveRandomKey(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var key = _fixture.Create<int>().ToString(CultureInfo.InvariantCulture);

                _storage.Remove(key);
            }
        }

        [Fact]
        public void SetGetOneSpecificKeyAreThreadSafe()
        {
            // Arrange.
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var key = _fixture.Create<string>();

            // Act.
            var firstTask = Task.Run(() => SetOneSpecificKey(key, cancellationToken), cancellationToken);
            var secondTask = Task.Run(() => GetOneSpecificKey(key, cancellationToken), cancellationToken);
            var thirdTask = Task.Run(() => cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(50)), cancellationToken);

            Task.WaitAll(firstTask, secondTask, thirdTask);
            cancellationTokenSource.Dispose();

            // Assert.
            Assert.Equal(TaskStatus.RanToCompletion, firstTask.Status);
            Assert.Equal(TaskStatus.RanToCompletion, secondTask.Status);
            Assert.Equal(TaskStatus.RanToCompletion, thirdTask.Status);
        }

        private void SetOneSpecificKey(string key, CancellationToken cancellationToken)
        {
            var cacheOptions = new MemoryCacheItemOptions();

            while (!cancellationToken.IsCancellationRequested)
            {
                var value = _fixture.Create<object>();

                _storage.Set(key, value, cacheOptions);
            }
        }

        private void GetOneSpecificKey(string key, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _storage.TryGet(key, out _);
            }
        }

        public void Dispose()
            => _storage.Dispose();
    }
}