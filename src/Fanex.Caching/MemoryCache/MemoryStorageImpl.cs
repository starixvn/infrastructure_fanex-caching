namespace Fanex.Caching.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MemoryStorageImpl : MemoryStorage, IDisposable
    {
        private const double CompactPercentage = 0.1;
        private readonly Dictionary<object, MemoryCacheItem> _items;
        private readonly ReaderWriterLockSlim _lock;
        private readonly SystemClock _clock;
        private TimeSpan _expirationScanFrequency;
        private DateTimeOffset _lastExpirationScan;
        private bool _disposed;

        public MemoryStorageImpl(MemoryCacheOptions options)
        {
            _clock = new SystemClockImpl();
            _items = new Dictionary<object, MemoryCacheItem>();
            _lock = new ReaderWriterLockSlim();
            _expirationScanFrequency = options.ExpirationScanFrequency;
            _lastExpirationScan = _clock.UtcNow;

            if (options.CompactOnMemoryPressure)
            {
                GcNotification.Register(DoMemoryPreassureCollection, null);
            }
        }

        ~MemoryStorageImpl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Contain(object key)
        {
            bool contained = false;

            CheckDisposed();
            _lock.EnterReadLock();

            try
            {
                MemoryCacheItem item;

                contained = _items.TryGetValue(key, out item);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            return contained;
        }

        public object Get(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            object value = null;
            MemoryCacheItem expiredItem = null;

            CheckDisposed();
            _lock.EnterReadLock();

            try
            {
                MemoryCacheItem item;

                if (_items.TryGetValue(key, out item))
                {
                    if (item.CheckExpired(_clock.UtcNow))
                    {
                        expiredItem = item;
                    }
                    else
                    {
                        item.Touch(_clock.UtcNow);
                        value = item.Value;
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (expiredItem != null)
            {
                RemoveItem(expiredItem);
            }

            StartScanForExpiredItems();

            return value;
        }

        public void Remove(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CheckDisposed();
            MemoryCacheItem item;
            _lock.EnterReadLock();

            try
            {
                if (_items.TryGetValue(key, out item))
                {
                    item.SetExpired(EvictionReason.Removed);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (item != null)
            {
                RemoveItem(item);
            }

            StartScanForExpiredItems();
        }

        public void Set(object key, object value, MemoryCacheItemOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CheckDisposed();

            DateTimeOffset utcNow = _clock.UtcNow;

            MemoryCacheItem previousItem = null;
            bool added = false;
            var item = new MemoryCacheItem(key, value, options, utcNow);

            _lock.EnterWriteLock();

            try
            {
                if (_items.TryGetValue(key, out previousItem))
                {
                    _items.Remove(key);
                    previousItem.SetExpired(EvictionReason.Replaced);
                }

                if (!item.CheckExpired(utcNow))
                {
                    _items[key] = item;
                    added = true;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            if (previousItem != null)
            {
                previousItem.InvokeEvictionCallbacks();
            }

            if (!added)
            {
                item.InvokeEvictionCallbacks();
            }

            StartScanForExpiredItems();
        }

        private static void ScanForExpiredItems(MemoryStorageImpl cache)
        {
            var expiredEntries = new List<MemoryCacheItem>();

            cache._lock.EnterReadLock();

            try
            {
                DateTimeOffset now = cache._clock.UtcNow;

                foreach (MemoryCacheItem entry in cache._items.Values)
                {
                    if (entry.CheckExpired(now))
                    {
                        expiredEntries.Add(entry);
                    }
                }
            }
            finally
            {
                cache._lock.ExitReadLock();
            }

            cache.RemoveItems(expiredEntries);
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
                    TaskCreationOptions.DenyChildAttach,
                    TaskScheduler.Default);
            }
        }

        private bool DoMemoryPreassureCollection(object state)
        {
            if (_disposed)
            {
                return false;
            }

            Compact(CompactPercentage);

            return true;
        }

        private void Compact(double percentage)
        {
            var expiredItems = new List<MemoryCacheItem>();
            var lowPriorityItems = new List<MemoryCacheItem>();
            var normalPriorityItems = new List<MemoryCacheItem>();
            var highPriorityItems = new List<MemoryCacheItem>();
            var neverRemovePriorityItems = new List<MemoryCacheItem>();

            _lock.EnterReadLock();

            try
            {
                CategorizeItems(
                    ref expiredItems,
                    ref lowPriorityItems,
                    ref normalPriorityItems,
                    ref highPriorityItems,
                    ref neverRemovePriorityItems);

                int totalEntries =
                    expiredItems.Count
                    + lowPriorityItems.Count
                    + normalPriorityItems.Count
                    + highPriorityItems.Count
                    + neverRemovePriorityItems.Count;
                int removalCountTarget = (int)(totalEntries * percentage);

                ExpirePriorityBucket(removalCountTarget, expiredItems, lowPriorityItems);
                ExpirePriorityBucket(removalCountTarget, expiredItems, normalPriorityItems);
                ExpirePriorityBucket(removalCountTarget, expiredItems, highPriorityItems);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            RemoveItems(expiredItems);
        }

        private void CategorizeItems(
            ref List<MemoryCacheItem> expiredItems,
            ref List<MemoryCacheItem> lowPriorityItems,
            ref List<MemoryCacheItem> normalPriorityItems,
            ref List<MemoryCacheItem> highPriorityItems,
            ref List<MemoryCacheItem> neverRemovePriorityItems)
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
                            neverRemovePriorityItems.Add(item);
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
                foreach (var entry in priorityItems)
                {
                    entry.SetExpired(EvictionReason.Capacity);
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

        private void RemoveItems(List<MemoryCacheItem> items)
        {
            _lock.EnterWriteLock();

            try
            {
                foreach (MemoryCacheItem item in items)
                {
                    MemoryCacheItem currentItem;

                    if (_items.TryGetValue(item.Key, out currentItem) && ReferenceEquals(currentItem, item))
                    {
                        _items.Remove(item.Key);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            foreach (MemoryCacheItem item in items)
            {
                item.InvokeEvictionCallbacks();
            }
        }

        private void RemoveItem(MemoryCacheItem item)
        {
            _lock.EnterWriteLock();

            try
            {
                MemoryCacheItem currentItem;

                if (_items.TryGetValue(item.Key, out currentItem) && ReferenceEquals(currentItem, item))
                {
                    _items.Remove(item.Key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            item.InvokeEvictionCallbacks();
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }

                _disposed = true;
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