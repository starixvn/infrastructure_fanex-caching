namespace Fanex.Caching.Memory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class MemoryStorageImpl : MemoryStorage, IDisposable
    {
        private const TaskCreationOptions TaskCreationOption = TaskCreationOptions.DenyChildAttach;
        private const double CompactPercentage = 0.1;

        private readonly ConcurrentDictionary<object, MemoryCacheItem> _items;
        private readonly SystemClock _clock;
        private readonly TimeSpan _expirationScanFrequency;

        private DateTimeOffset _lastExpirationScan;
        private bool _disposed;

        public MemoryStorageImpl(MemoryCacheOptions options, SystemClock clock)
        {
            _items = new ConcurrentDictionary<object, MemoryCacheItem>();
            _clock = clock;
            _expirationScanFrequency = options.ExpirationScanFrequency;
            _lastExpirationScan = _clock.UtcNow;

            if (options.CompactOnMemoryPressure)
            {
                GcNotification.Register(DoMemoryPreassureCollection, this);
            }
        }

        internal int Count => _items.Count;

        private ICollection<KeyValuePair<object, MemoryCacheItem>> Items => _items;

        public void Dispose()
            => _disposed = true;

        public bool Contain(object key)
        {
            CheckDisposed();

            return _items.TryGetValue(key, out _);
        }

        public bool TryGet(object key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var found = false;

            CheckDisposed();
            value = null;

            if (_items.TryGetValue(key, out MemoryCacheItem item))
            {
                if (item.CheckExpired(_clock.UtcNow) && !item.IsReplaced)
                {
                    RemoveItem(item);
                }
                else
                {
                    item.Touch(_clock.UtcNow);
                    value = item.Value;
                    found = true;
                }
            }

            StartScanForExpiredItems();

            return found;
        }

        public void Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CheckDisposed();

            if (_items.TryRemove(key, out MemoryCacheItem item))
            {
                item.SetExpired(EvictionReason.Removed);
                item.InvokeEvictionCallbacks();
            }

            StartScanForExpiredItems();
        }

        public void Set(object key, object value, MemoryCacheItemOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_disposed)
            {
                // No-op instead of throwing since this is called during CacheEntry.Dispose
                return;
            }

            DateTimeOffset utcNow = _clock.UtcNow;
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

        private static void ScanForExpiredItems(MemoryStorageImpl cache)
        {
            DateTimeOffset now = cache._clock.UtcNow;

            foreach (MemoryCacheItem item in cache._items.Values)
            {
                if (item.CheckExpired(now))
                {
                    cache.RemoveItem(item);
                }
            }
        }

        private void StartScanForExpiredItems()
        {
            DateTimeOffset now = _clock.UtcNow;

            if (_expirationScanFrequency < now - _lastExpirationScan)
            {
                _lastExpirationScan = now;

                Task.Factory.StartNew(
                    state => ScanForExpiredItems((MemoryStorageImpl)state),
                    this,
                    CancellationToken.None,
                    TaskCreationOption,
                    TaskScheduler.Default);
            }
        }

        private bool DoMemoryPreassureCollection(object state)
        {
            if (_disposed)
            {
                return false;
            }

            if (state is MemoryStorageImpl storage)
            {
                storage.Compact(CompactPercentage);

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

            foreach (MemoryCacheItem item in expiredItems)
            {
                RemoveItem(item);
            }
        }

        private void CategorizeItems(
            ref List<MemoryCacheItem> expiredItems,
            ref List<MemoryCacheItem> lowPriorityItems,
            ref List<MemoryCacheItem> normalPriorityItems,
            ref List<MemoryCacheItem> highPriorityItems)
        {
            DateTimeOffset now = _clock.UtcNow;

            foreach (var item in _items.Values)
            {
                if (item.CheckExpired(now))
                {
                    expiredItems.Add(item);
                }
                else
                {
                    var itemOptions = item.Options as MemoryCacheItemOptions;

                    switch (itemOptions.Priority)
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

                        case CachePriority.NeverRemove:
                            break;
                    }
                }
            }
        }

        private void ExpirePriorityBucket(
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

            IEnumerable<MemoryCacheItem> orderedPriorityItems = priorityItems.OrderBy(item => item.LastAccess);

            foreach (MemoryCacheItem item in orderedPriorityItems)
            {
                item.SetExpired(EvictionReason.Capacity);
                expiredItems.Add(item);

                if (removalCountTarget <= expiredItems.Count)
                {
                    break;
                }
            }
        }

        private void RemoveItem(MemoryCacheItem item)
        {
            if (Items.Remove(new KeyValuePair<object, MemoryCacheItem>(item.Key, item)))
            {
                item.InvokeEvictionCallbacks();
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(MemoryStorageImpl).FullName);
            }
        }
    }
}