namespace Fanex.Caching.Tests.Memory;

using System;
using AutoFixture;
using Fanex.Caching.Memory;
using Xunit;

public sealed class MemoryCompactTests : IDisposable
{
    private const int NumberOfPreInitializedKeys = 2;
    private const double CompactionPercentage = 0.5;

    private readonly Fixture _fixture;
    private readonly SystemClockMock _clock;
    private readonly MemoryCacheItemOptions _cacheOptions;
    private readonly MemoryStorage _storage;

    public MemoryCompactTests()
    {
        _fixture = new Fixture();
        _clock = new SystemClockMock();
        _cacheOptions = new MemoryCacheItemOptions();
        _storage = new MemoryStorage(new MemoryCacheOptions
        {
            Clock = _clock,
            ExpirationScanFrequency = TimeSpan.FromMinutes(10),
            CompactionPercentage = CompactionPercentage
        });

        InitializeCache(NumberOfPreInitializedKeys, _cacheOptions);
    }

    [Fact]
    public void Compact100PercentClearsAll()
    {
        // Arrange.
        const double fullCompactionPercentage = 1;

        // Act.
        _storage.Compact(fullCompactionPercentage);

        // Assert.
        Assert.Equal(0, _storage.Count);
    }

    [Fact]
    public void Compact100PercentClearsAllExcludeItemsWithNeverRemovePriority()
    {
        // Arrange.
        const int numberOfNeverRemoveKeys = 2;
        const double fullCompactionPercentage = 1;

        var neverRemovePolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.NeverRemove);
        var neverRemoveKeys = InitializeCache(numberOfNeverRemoveKeys, neverRemovePolicy);

        // Act.
        _storage.Compact(fullCompactionPercentage);

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
        const int numberOfLowPriorityKeys = 2;

        var lowPriorityPolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.Low);
        var lowPriorityKeys = InitializeCache(numberOfLowPriorityKeys, lowPriorityPolicy);

        // Act.
        _storage.Compact(CompactionPercentage);

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
        const int numberOfHighPriorityKeys = 2;

        var highPriorityPolicy = new MemoryCacheItemOptions().SetCachePriority(CachePriority.High);
        var highPriorityKeys = InitializeCache(numberOfHighPriorityKeys, highPriorityPolicy);

        // Act.
        _storage.Compact(CompactionPercentage);

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
        const int numberOfExpiredKeys = 2;

        var timeToLive = TimeSpan.FromSeconds(1);
        var expiredPolicy = new MemoryCacheItemOptions()
            .SetCachePriority(CachePriority.High)
            .SetAbsoluteExpiration(timeToLive);
        var expiredKeys = InitializeCache(numberOfExpiredKeys, expiredPolicy);

        // Act.
        var timeToExpire = TimeSpan.FromSeconds(5);

        _clock.Add(timeToExpire);
        _storage.Compact(CompactionPercentage);

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
        const int numberOfNewKeys = 8;
        const double highCompactionPercentage = 0.9;

        var newKeys = InitializeCache(numberOfNewKeys, _cacheOptions);

        // Act.
        _storage.Compact(highCompactionPercentage);

        // Assert.
        var lastAddedKey = newKeys[numberOfNewKeys - 1];
        var found = _storage.TryGet(lastAddedKey, out _);

        Assert.True(found);
    }

    [Fact]
    public void CompactOnMemoryPressureTriggersDoMemoryPreassureCollection()
    {
        // Arrange.
        const int numberOfNewKeys = 4;

        InitializeCache(numberOfNewKeys, _cacheOptions);

        // Act.
        TriggerGargabeCollectionInItsOwnScope();

        // Assert.
        const int numberOfCachedKeys = numberOfNewKeys + NumberOfPreInitializedKeys;

        Assert.True(numberOfCachedKeys > _storage.Count);
    }

    [Fact]
    public void DoMemoryPreassureCollectionTriesToSatisfyRemovalCountTarget()
    {
        // Arrange.
        const int numberOfNewKeys = 8;

        InitializeCache(numberOfNewKeys, _cacheOptions);

        // Act.
        TriggerGargabeCollectionInItsOwnScope();

        // Assert.
        const int numberOfCachedKeys = numberOfNewKeys + NumberOfPreInitializedKeys;
        var expectedCount = numberOfCachedKeys - (int)(CompactionPercentage * numberOfCachedKeys);

        Assert.Equal(expectedCount, _storage.Count);
    }

#pragma warning disable S1215 // "GC.Collect" should not be called

    private static void TriggerGargabeCollectionInItsOwnScope()
    {
        const int oldestGenerationBeCollected = 2;

        GC.Collect(oldestGenerationBeCollected, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
    }

#pragma warning restore S1215 // "GC.Collect" should not be called

    public void Dispose()
        => _storage.Dispose();

    private string[] InitializeCache(int numberOfItem, MemoryCacheItemOptions cacheOptions)
    {
        var createdKeys = new string[numberOfItem];
        var creationInterval = TimeSpan.FromSeconds(1);

        for (var index = 0; index < numberOfItem; index++)
        {
            createdKeys[index] = _fixture.Create<string>();
            _storage.Set(createdKeys[index], _fixture.Create<object>(), cacheOptions);
            _clock.Add(creationInterval);
        }

        return createdKeys;
    }
}