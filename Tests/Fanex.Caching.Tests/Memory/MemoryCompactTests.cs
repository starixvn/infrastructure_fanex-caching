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
        private readonly MemoryCacheItemOptions _cachingOptions;

        private MemoryStorage _storage;

        public MemoryCompactTests()
        {
            _fixture = new Fixture();
            _clock = new SystemClockMock();
            _cachingOptions = new MemoryCacheItemOptions();

            InitializeCache(NumberOfPreInitializedKeys, _cachingOptions);
        }

        [Fact]
        public void NegativeCompactionPercentageThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var negativeCompactionPercentage = -0.1;

            // Act.
            Exception exception = Record.Exception(() => new MemoryCacheOptions
            {
                CompactionPercentage = negativeCompactionPercentage
            });

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void GreaterThanOneCompactionPercentageThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var outOfRangeCompactionPercentage = 1.1;

            // Act.
            Exception exception = Record.Exception(() => new MemoryCacheOptions
            {
                CompactionPercentage = outOfRangeCompactionPercentage
            });

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
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
            int numberOfNeverRemoveKeys = 2;
            var neverRemovePolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.NeverRemove);
            string[] neverRemoveKeys = InitializeCache(numberOfNeverRemoveKeys, neverRemovePolicy);

            // Act.
            _storage.Compact(1.0);

            // Assert.
            Assert.Equal(numberOfNeverRemoveKeys, _storage.Count);

            foreach (string key in neverRemoveKeys)
            {
                Assert.True(_storage.TryGet(key, out _));
            }
        }

        [Fact]
        public void CompactPrioritizesLowPriorityItems()
        {
            // Arrange.
            int numberOfLowPriorityKeys = 2;
            var lowPriorityPolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.Low);
            string[] lowPriorityKeys = InitializeCache(numberOfLowPriorityKeys, lowPriorityPolicy);

            // Act.
            _storage.Compact(0.5);

            // Assert.
            Assert.Equal(NumberOfPreInitializedKeys, _storage.Count);

            foreach (string key in lowPriorityKeys)
            {
                Assert.False(_storage.TryGet(key, out _));
            }
        }

        [Fact]
        public void CompactDisordersHighPriorityItems()
        {
            // Arrange.
            int numberOfHighPriorityKeys = 2;
            var highPriorityPolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.High);
            string[] highPriorityKeys = InitializeCache(numberOfHighPriorityKeys, highPriorityPolicy);

            // Act.
            _storage.Compact(0.5);

            // Assert.
            Assert.Equal(numberOfHighPriorityKeys, _storage.Count);

            foreach (string key in highPriorityKeys)
            {
                Assert.True(_storage.TryGet(key, out _));
            }
        }

        [Fact]
        public void CompactPrioritizesExpiredItems()
        {
            // Arrange.
            int numberOfExpiredKeys = 2;
            var expiredPolicy = new MemoryCacheItemOptions();

            expiredPolicy
                .SetCachePriority(CachePriority.High)
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(1));
            string[] expiredKeys = InitializeCache(numberOfExpiredKeys, expiredPolicy);

            // Act.
            _clock.Add(TimeSpan.FromSeconds(5));
            _storage.Compact(0.5);

            // Assert.
            Assert.Equal(NumberOfPreInitializedKeys, _storage.Count);

            foreach (string key in expiredKeys)
            {
                Assert.False(_storage.TryGet(key, out _));
            }
        }

        [Fact]
        public void CompactPrioritizesLRU()
        {
            // Arrange.
            int numberOfNewKeys = 2;
            string[] newKeys = InitializeCache(numberOfNewKeys, _cachingOptions);

            // Act.
            _storage.Compact(0.90);

            // Assert.
            Assert.True(_storage.TryGet(newKeys[numberOfNewKeys - 1], out _));
        }

        [Fact]
        public void CompactOnMemoryPressureTriggersDoMemoryPreassureCollection()
        {
            // Arrange.
            int numberOfKeys = 4;
            var storageOptions = new MemoryCacheOptions
            {
                CompactOnMemoryPressure = true,
                CompactionPercentage = 0.5,
                Clock = _clock
            };

            _storage = new MemoryStorage(storageOptions);

            InitializeCache(numberOfKeys, _cachingOptions);

            // Act.
            TriggerGargabeCollectionInItsOwnScope();

            // Assert.
            Assert.True(numberOfKeys > _storage.Count);
        }

        [Fact]
        public void DoMemoryPreassureCollectionTriesToSatisfyRemovalCountTarget()
        {
            // Arrange.
            int numberOfKeys = 4;
            double compactPercentage = 0.5;
            var storageOptions = new MemoryCacheOptions
            {
                CompactOnMemoryPressure = true,
                CompactionPercentage = compactPercentage,
                Clock = _clock
            };

            _storage = new MemoryStorage(storageOptions);

            InitializeCache(numberOfKeys, _cachingOptions);

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

            for (int index = 0; index < numberOfItem; index++)
            {
                createdKeys[index] = _fixture.Create<string>();

                _storage.Set(createdKeys[index], _fixture.Create<object>(), cacheOptions);
                _clock.Add(TimeSpan.FromSeconds(1));
            }

            return createdKeys;
        }
    }
}