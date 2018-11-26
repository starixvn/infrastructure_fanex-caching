namespace Fanex.Caching.Memory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class MemoryStorage : IMemoryStorage
    {
        private readonly ConcurrentDictionary<string, MemoryCacheItem> _items;
        private readonly ISystemClock _clock;
        private readonly TimeSpan _expirationScanFrequency;
        private readonly double _compactionPercentage;

        private DateTimeOffset _lastExpirationScan;
        private bool _disposed;

        public MemoryStorage(MemoryCacheOptions options)
        {
            _items = new ConcurrentDictionary<string, MemoryCacheItem>();
            _clock = options.Clock;
            _expirationScanFrequency = options.ExpirationScanFrequency;
            _lastExpirationScan = _clock.UtcNow;
            _compactionPercentage = options.CompactionPercentage;

            if (options.CompactOnMemoryPressure)
            {
                RegisterMemoryPreassureCollection();
            }
        }

        public int Count => _items.Count;

        private ICollection<KeyValuePair<string, MemoryCacheItem>> Items => _items;

        public bool Contain(string key)
            => TryGet(key, out _);

        public bool TryGet(string key, out object value)
        {
            var found = false;
            var now = _clock.UtcNow;

            CheckDisposed();
            value = null;

            if (_items.TryGetValue(key, out MemoryCacheItem item))
            {
                if (item.CheckExpired(now) && !item.IsReplaced)
                {
                    RemoveItem(item);
                }
                else
                {
                    item.Touch(now);
                    value = item.Value;
                    found = true;
                }
            }

            StartScanForExpiredItems();

            return found;
        }

        public void Remove(string key)
        {
            CheckDisposed();

            if (_items.TryRemove(key, out MemoryCacheItem item))
            {
                item.SetExpired(EvictionReason.Removed);
                item.InvokeEvictionCallbacks();
            }

            StartScanForExpiredItems();
        }

        public void Set(string key, object value, MemoryCacheItemOptions options)
        {
            CheckDisposed();

            var utcNow = _clock.UtcNow;
            var item = new MemoryCacheItem(key, value, options, utcNow);

            if (_items.TryGetValue(key, out MemoryCacheItem previousItem))
            {
                previousItem.SetExpired(EvictionReason.Replaced);
            }

            if (!item.CheckExpired(utcNow))
            {
                TryAddOrUpdateItem(item, previousItem);

                if (previousItem != null)
                {
                    previousItem.InvokeEvictionCallbacks();
                }
            }
            else
            {
                item.InvokeEvictionCallbacks();

                if (previousItem != null)
                {
                    RemoveItem(previousItem);
                }
            }

            StartScanForExpiredItems();
        }

        internal void TryAddOrUpdateItem(MemoryCacheItem item, MemoryCacheItem previousItem)
        {
            bool added;

            if (previousItem == null)
            {
                added = _items.TryAdd(item.Key, item);
            }
            else
            {
                added = _items.TryUpdate(item.Key, item, previousItem);

                if (!added)
                {
                    added = _items.TryAdd(item.Key, item);
                }
            }

            if (!added)
            {
                item.SetExpired(EvictionReason.Replaced);
                item.InvokeEvictionCallbacks();
            }
        }

        private void RegisterMemoryPreassureCollection()
            => GcNotification.Register(DoMemoryPreassureCollection, this);

        private void StartScanForExpiredItems()
        {
            var now = _clock.UtcNow;

            if (_expirationScanFrequency < now - _lastExpirationScan)
            {
                _lastExpirationScan = now;

                Task.Run(() => ScanForExpiredItems(this));
            }
        }

        private static void ScanForExpiredItems(MemoryStorage cache)
        {
            var now = cache._clock.UtcNow;

            foreach (var item in cache._items.Values)
            {
                if (item.CheckExpired(now))
                {
                    cache.RemoveItem(item);
                }
            }
        }

        private static bool DoMemoryPreassureCollection(object state)
        {
            if (state is MemoryStorage storage && !storage._disposed)
            {
                storage.Compact(storage._compactionPercentage);

                return true;
            }

            return false;
        }

        internal void Compact(double percentage)
        {
            var expiredItems = new List<MemoryCacheItem>();
            var lowPriorityItems = new List<MemoryCacheItem>();
            var normalPriorityItems = new List<MemoryCacheItem>();
            var highPriorityItems = new List<MemoryCacheItem>();
            var removalCountTarget = (int)(_items.Count * percentage);

            CategorizeItems(
                    ref expiredItems,
                    ref lowPriorityItems,
                    ref normalPriorityItems,
                    ref highPriorityItems);

            ExpirePriorityBucket(removalCountTarget, expiredItems, lowPriorityItems);
            ExpirePriorityBucket(removalCountTarget, expiredItems, normalPriorityItems);
            ExpirePriorityBucket(removalCountTarget, expiredItems, highPriorityItems);

            foreach (var item in expiredItems)
            {
                RemoveItem(item);
            }
        }

#pragma warning disable S131 // "switch/Select" statements should contain a "default/Case Else" clauses

        private void CategorizeItems(
            ref List<MemoryCacheItem> expiredItems,
            ref List<MemoryCacheItem> lowPriorityItems,
            ref List<MemoryCacheItem> normalPriorityItems,
            ref List<MemoryCacheItem> highPriorityItems)
        {
            var now = _clock.UtcNow;

            foreach (var item in _items.Values)
            {
                if (item.CheckExpired(now))
                {
                    expiredItems.Add(item);
                }
                else
                {
                    switch (item.Options.Priority)
                    {
                        case CachePriority.Low:
                            lowPriorityItems.Add(item);
                            break;

                        case CachePriority.Normal:
                            normalPriorityItems.Add(item);
                            break;

                        case CachePriority.High:
                            highPriorityItems.Add(item);
                            break;
                    }
                }
            }
        }

#pragma warning restore S131 // "switch/Select" statements should contain a "default/Case Else" clauses

        private static void ExpirePriorityBucket(
            int removalCountTarget,
            List<MemoryCacheItem> expiredItems,
            List<MemoryCacheItem> priorityItems)
        {
            if (removalCountTarget <= expiredItems.Count)
            {
                return;
            }

            if (expiredItems.Count + priorityItems.Count <= removalCountTarget)
            {
                foreach (var item in priorityItems)
                {
                    item.SetExpired(EvictionReason.Capacity);
                }

                expiredItems.AddRange(priorityItems);

                return;
            }

            var orderedPriorityItems = priorityItems.OrderBy(entry => entry.LastAccess).GetEnumerator();

            try
            {
                MemoryCacheItem item;

                while (orderedPriorityItems.MoveNext() && removalCountTarget > expiredItems.Count)
                {
                    item = orderedPriorityItems.Current;
                    item.SetExpired(EvictionReason.Capacity);
                    expiredItems.Add(item);
                }
            }
            finally
            {
                orderedPriorityItems.Dispose();
            }
        }

        private void RemoveItem(MemoryCacheItem item)
        {
            if (Items.Remove(new KeyValuePair<string, MemoryCacheItem>(item.Key, item)))
            {
                item.InvokeEvictionCallbacks();
            }
        }

        public void Dispose()
            => _disposed = true;

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(MemoryStorage).FullName);
            }
        }
    }
}