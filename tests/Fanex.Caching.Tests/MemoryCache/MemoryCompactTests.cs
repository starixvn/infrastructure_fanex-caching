namespace Fanex.Caching.Tests.MemoryCache
{
    using System;
    using Memory;
    using Mocks;
    using Xunit;

    public sealed class MemoryCompactTests : IDisposable
    {
        private readonly MemoryCacheItemOptions _cachingOptions;
        private readonly SystemClockMock _clock;

        private MemoryStorageImpl _storage;

        public MemoryCompactTests()
        {
            var options = new MemoryCacheOptions
            {
                CompactOnMemoryPressure = false
            };

            _clock = new SystemClockMock();
            _storage = new MemoryStorageImpl(options, _clock);
            _cachingOptions = new MemoryCacheItemOptions();
        }

        [Fact]
        public void Compact100PercentClearsAll()
        {
            // Arrange.
            _storage.Set("key1", "value1", _cachingOptions);
            _storage.Set("key2", "value2", _cachingOptions);

            // Act.
            _storage.Compact(1.0);

            // Assert.
            Assert.Equal(0, _storage.Count);
        }

        [Fact]
        public void Compact100PercentClearsAllButNeverRemoveItems()
        {
            // Arrange.
            _storage.Set("key1", "value1", _cachingOptions);
            _storage.Set("key2", "value2", _cachingOptions);
            _storage.Set("key3", "value3", new MemoryCacheItemOptions().SetCachePriority(CachePriority.NeverRemove));
            _storage.Set("key4", "value4", new MemoryCacheItemOptions().SetCachePriority(CachePriority.NeverRemove));

            // Act.
            _storage.Compact(1.0);

            // Assert.
            Assert.Equal(2, _storage.Count);
            Assert.True(_storage.TryGet("key3", out _));
            Assert.True(_storage.TryGet("key4", out _));
        }

        [Fact]
        public void CompactPrioritizesLowPriortyItems()
        {
            // Arrange.
            _storage.Set("key1", "value1", _cachingOptions);
            _storage.Set("key2", "value2", _cachingOptions);
            _storage.Set("key3", "value3", new MemoryCacheItemOptions().SetCachePriority(CachePriority.Low));
            _storage.Set("key4", "value4", new MemoryCacheItemOptions().SetCachePriority(CachePriority.Low));

            // Act.
            _storage.Compact(0.5);

            // Assert.
            Assert.Equal(2, _storage.Count);
            Assert.False(_storage.TryGet("key3", out _));
            Assert.False(_storage.TryGet("key4", out _));
        }

        [Fact]
        public void CompactPrioritizesLRU()
        {
            // Arrange.
            _storage.Set("key1", "value1", _cachingOptions);
            _clock.Add(TimeSpan.FromSeconds(1));
            _storage.Set("key2", "value2", _cachingOptions);
            _clock.Add(TimeSpan.FromSeconds(1));
            _storage.Set("key3", "value3", _cachingOptions);
            _clock.Add(TimeSpan.FromSeconds(1));
            _storage.Set("key4", "value4", _cachingOptions);

            // Act.
            _storage.Compact(0.90);

            // Assert.
            bool found = _storage.TryGet("key4", out object result);

            Assert.Equal(1, _storage.Count);
            Assert.True(found);
            Assert.Equal("value4", result);
        }

        public void Dispose()
        {
            _storage.Dispose();
            _storage = null;
        }
    }
}