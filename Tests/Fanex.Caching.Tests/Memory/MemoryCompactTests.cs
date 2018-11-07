namespace Fanex.Caching.Tests.Memory
{
    using System;
    using AutoFixture;
    using Fanex.Caching.Memory;
    using Xunit;

    public sealed class MemoryCompactTests : IDisposable
    {
        private const int NumberOfPreInitializedKeys = 2;

        private readonly Fixture _fixture;
        private readonly SystemClockMock _clock;
        private readonly MemoryCacheItemOptions _cacheOptions;

        private MemoryStorage _storage;

        public MemoryCompactTests()
        {
            _fixture = new Fixture();
            _clock = new SystemClockMock();
            _cacheOptions = new MemoryCacheItemOptions();

            InitializeCache(NumberOfPreInitializedKeys, _cacheOptions);
        }

        [Fact]
        public void Compact100PercentClearsAll()
        {
            // Act.
            _storage.Compact(1.0);

            // Assert.
            Assert.Equal(0, _storage.Count);
        }

        [Fact]
        public void Compact100PercentClearsAllExcludeItemsWithNeverRemovePriority()
        {
            // Arrange.
            var numberOfNeverRemoveKeys = 2;
            var neverRemovePolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.NeverRemove);
            var neverRemoveKeys = InitializeCache(numberOfNeverRemoveKeys, neverRemovePolicy);

            // Act.
            _storage.Compact(1.0);

            // Assert.
            Assert.Equal(numberOfNeverRemoveKeys, _storage.Count);

            foreach (var key in neverRemoveKeys)
            {
                var found = _storage.TryGet(key, out _);

                Assert.True(found);
            }
        }

        [Fact]
        public void CompactPrioritizesLowPriorityItems()
        {
            // Arrange.
            var numberOfLowPriorityKeys = 2;
            var lowPriorityPolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.Low);
            var lowPriorityKeys = InitializeCache(numberOfLowPriorityKeys, lowPriorityPolicy);

            // Act.
            _storage.Compact(0.5);

            // Assert.
            Assert.Equal(NumberOfPreInitializedKeys, _storage.Count);

            foreach (var key in lowPriorityKeys)
            {
                var found = _storage.TryGet(key, out _);

                Assert.False(found);
            }
        }

        [Fact]
        public void CompactDisordersHighPriorityItems()
        {
            // Arrange.
            var numberOfHighPriorityKeys = 2;
            var highPriorityPolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.High);
            var highPriorityKeys = InitializeCache(numberOfHighPriorityKeys, highPriorityPolicy);

            // Act.
            _storage.Compact(0.5);

            // Assert.
            Assert.Equal(numberOfHighPriorityKeys, _storage.Count);

            foreach (var key in highPriorityKeys)
            {
                var found = _storage.TryGet(key, out _);

                Assert.True(found);
            }
        }

        [Fact]
        public void CompactPrioritizesExpiredItems()
        {
            // Arrange.
            var numberOfExpiredKeys = 2;
            var expiredPolicy = new MemoryCacheItemOptions()
                .SetCachePriority(CachePriority.High)
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(1));
            var expiredKeys = InitializeCache(numberOfExpiredKeys, expiredPolicy);

            // Act.
            _clock.Add(TimeSpan.FromSeconds(5));
            _storage.Compact(0.5);

            // Assert.
            Assert.Equal(NumberOfPreInitializedKeys, _storage.Count);

            foreach (var key in expiredKeys)
            {
                var found = _storage.TryGet(key, out _);

                Assert.False(found);
            }
        }

        [Fact]
        public void CompactPrioritizesLRU()
        {
            // Arrange.
            var numberOfNewKeys = 2;
            var newKeys = InitializeCache(numberOfNewKeys, _cacheOptions);

            // Act.
            _storage.Compact(0.90);

            // Assert.
            var found = _storage.TryGet(newKeys[numberOfNewKeys - 1], out _);

            Assert.True(found);
        }

        [Fact]
        public void CompactOnMemoryPressureTriggersDoMemoryPreassureCollection()
        {
            // Arrange.
            var numberOfKeys = 4;
            var storageOptions = new MemoryCacheOptions
            {
                CompactOnMemoryPressure = true,
                CompactionPercentage = 0.5,
                Clock = _clock
            };

            _storage = new MemoryStorage(storageOptions);

            InitializeCache(numberOfKeys, _cacheOptions);

            // Act.
            TriggerGargabeCollectionInItsOwnScope();

            // Assert.
            Assert.True(numberOfKeys > _storage.Count);
        }

        [Fact]
        public void DoMemoryPreassureCollectionTriesToSatisfyRemovalCountTarget()
        {
            // Arrange.
            var numberOfKeys = 4;
            var compactPercentage = 0.5;
            var storageOptions = new MemoryCacheOptions
            {
                CompactOnMemoryPressure = true,
                CompactionPercentage = compactPercentage,
                Clock = _clock
            };

            _storage = new MemoryStorage(storageOptions);

            InitializeCache(numberOfKeys, _cacheOptions);

            // Act.
            TriggerGargabeCollectionInItsOwnScope();

            // Assert.
            int expectedCount = numberOfKeys - (int)(compactPercentage * numberOfKeys);

            Assert.True(expectedCount >= _storage.Count);
        }

#pragma warning disable S1215 // "GC.Collect" should not be called

        private static void TriggerGargabeCollectionInItsOwnScope()
        {
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

#pragma warning restore S1215 // "GC.Collect" should not be called

        public void Dispose()
            => _storage.Dispose();

        private string[] InitializeCache(int numberOfItem, MemoryCacheItemOptions cacheOptions)
        {
            if (_storage == null)
            {
                _storage = new MemoryStorage(new MemoryCacheOptions
                {
                    Clock = _clock
                });
            }

            var createdKeys = new string[numberOfItem];

            for (var index = 0; index < numberOfItem; index++)
            {
                createdKeys[index] = _fixture.Create<string>();

                _storage.Set(createdKeys[index], _fixture.Create<object>(), cacheOptions);
                _clock.Add(TimeSpan.FromSeconds(1));
            }

            return createdKeys;
        }
    }
}