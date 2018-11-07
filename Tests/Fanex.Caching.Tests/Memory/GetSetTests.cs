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
        private readonly MemoryCacheItemOptions _cacheOptions;
        private readonly TimeSpan _callbackWaitingTime;
        private readonly string _existingKey;
        private readonly object _existingValue;

        private MemoryStorage _storage;
        private ManualResetEventSlim _callbackInvoked;
        private EvictionReason _callbackReason;

        public GetSetTests()
        {
            _fixture = new Fixture();
            _clock = new SystemClockMock();
            _cacheOptions = new MemoryCacheItemOptions();
            _storage = new MemoryStorage(new MemoryCacheOptions
            {
                Clock = _clock
            });
            _callbackInvoked = new ManualResetEventSlim(false);
            _callbackWaitingTime = TimeSpan.FromSeconds(3);
            _existingKey = _fixture.Create<string>();
            _existingValue = _fixture.Create<object>();

            _fixture.Customizations.Add(new RandomNumericSequenceGenerator(0, 10));
            _cacheOptions.RegisterPostEvictionCallback(
                (key, value, reason, state) =>
                {
                    var callbackInvoked = (ManualResetEventSlim)state;

                    Assert.Equal(_existingKey, key);
                    Assert.Same(_existingValue, value);

                    _callbackReason = reason;
                    callbackInvoked.Set();
                },
                _callbackInvoked);
            _storage.Set(_existingKey, _existingValue, _cacheOptions);
        }

        [Fact]
        public void TryGetMissingKeyReturnsFalse()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            var found = _storage.TryGet(missingKey, out _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void TryGetMissingKeyReturnsNullAsOutput()
        {
            // Arrange.
            var missingKey = _fixture.Create<string>();

            // Act.
            _storage.TryGet(missingKey, out object actualValue);

            // Assert.
            Assert.Null(actualValue);
        }

        [Fact]
        public void TryGetExistingKeyReturnsTrue()
        {
            // Act.
            var found = _storage.TryGet(_existingKey, out _);

            // Assert.
            Assert.True(found);
        }

        [Fact]
        public void TryGetReturnsObjectStoredBySet()
        {
            // Act.
            _storage.TryGet(_existingKey, out object actualValue);

            // Assert.
            Assert.Same(_existingValue, actualValue);
        }

        [Fact]
        public void SetAndTryGetWorkWithCaseSensitiveKeys()
        {
            // Arrange.
            var caseSensitiveKey = _existingKey.ToUpperInvariant();

            // Act.
            var found = _storage.TryGet(caseSensitiveKey, out _);

            // Assert.
            Assert.False(found);
        }

        [Fact]
        public void SetAlwaysOverwritesExistingObject()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();

            // Act.
            _storage.Set(_existingKey, newValue, _cacheOptions);

            // Assert.
            _storage.TryGet(_existingKey, out object actualValue);
            Assert.Same(newValue, actualValue);
        }

        [Fact]
        public void SetOverwritesKeyAndInvokesCallbackOfPreviousSet()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();
            var newCacheOptions = new MemoryCacheItemOptions();

            // Act.
            _storage.Set(_existingKey, newValue, newCacheOptions);

            // Assert.
            Assert.True(_callbackInvoked.Wait(_callbackWaitingTime));
        }

        [Fact]
        public void SetOverwritesKeyWithReplacedReason()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();
            var newCacheOptions = new MemoryCacheItemOptions();

            // Act.
            _storage.Set(_existingKey, newValue, newCacheOptions);

            // Assert.
            Assert.True(_callbackInvoked.Wait(_callbackWaitingTime));
            Assert.Equal(EvictionReason.Replaced, _callbackReason);
        }

        [Fact]
        public void SetExistingKeyWithValueBeingExpiredRemovesOldValue()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();
            var expiredPolicy = new MemoryCacheItemOptions().SetAbsoluteExpiration(_clock.UtcNow);

            // Act.
            _storage.Set(_existingKey, newValue, expiredPolicy);

            // Assert.
            var found = _storage.TryGet(_existingKey, out _);

            Assert.False(found);
        }

        [Fact]
        public void SetExistingKeyWithValueBeingExpiredInvokesCallbackOfPreviousSet()
        {
            // Arrange.
            var newValue = _fixture.Create<object>();
            var expiredPolicy = new MemoryCacheItemOptions().SetAbsoluteExpiration(_clock.UtcNow);

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
            var found = _storage.TryGet(_existingKey, out _);

            Assert.False(found);
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

#pragma warning disable S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods

        [Fact]
        public void ObjectDisposedExceptionIsThrownIfTryGetIsCalledAfterDisposingStorage()
        {
            // Arrange.
            _storage.Dispose();

            // Act.
            var exception = Record.Exception(() => _storage.TryGet(_existingKey, out _));

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
            var exception = Record.Exception(() => _storage.Set(_existingKey, newValue, _cacheOptions));

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
        {
            _storage.Dispose();
            _storage = null;
            _callbackInvoked.Dispose();
            _callbackInvoked = null;
        }
    }
}