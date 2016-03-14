namespace Fanex.Caching.MemoryCache
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MemoryCacheItem : CacheItem
    {
        private IList<PostEvictionCallbackRegistration> _postEvictionCallbacks;
        private DateTimeOffset? _absoluteExpiration;
        private EvictionReason _evictionReason;
        private bool _isExpired;

        public MemoryCacheItem(
            object key,
            object value,
            MemoryCacheItemOptions options,
            DateTimeOffset createdTime)
            : base(key, value, options)
        {
            _absoluteExpiration = options.CalculateAbsoluteExpiration(createdTime);
            LastAccess = createdTime;
        }

        public DateTimeOffset LastAccess { get; private set; }

        public void Touch(DateTimeOffset now)
        {
            LastAccess = now;
        }

        public void SetExpired(EvictionReason reason)
        {
            _isExpired = true;

            if (_evictionReason == EvictionReason.None)
            {
                _evictionReason = reason;
            }
        }

        public void InvokeEvictionCallbacks()
        {
            if (_postEvictionCallbacks != null)
            {
                Task.Factory.StartNew(
                    state => InvokeCallbacks((MemoryCacheItem)state),
                    this,
                    CancellationToken.None,
                    TaskCreationOptions.DenyChildAttach,
                    TaskScheduler.Default);
            }
        }

        private static void InvokeCallbacks(MemoryCacheItem item)
        {
            var callbackRegistrations = Interlocked.Exchange(ref item._postEvictionCallbacks, null);

            if (callbackRegistrations == null)
            {
                return;
            }

            foreach (PostEvictionCallbackRegistration registration in callbackRegistrations)
            {
                try
                {
                    registration.EvictionCallback?.Invoke(item.Key, item.Value, item._evictionReason, registration.State);
                }
                catch
                {
                }
            }
        }

        public bool CheckExpired(DateTimeOffset now)
        {
            return _isExpired || CheckForExpiredTime(now);
        }

        private bool CheckForExpiredTime(DateTimeOffset now)
        {
            if (_absoluteExpiration.HasValue && _absoluteExpiration.Value <= now)
            {
                SetExpired(EvictionReason.Expired);

                return true;
            }

            if (Options.SlidingExpiration.HasValue && (now - LastAccess) >= Options.SlidingExpiration)
            {
                SetExpired(EvictionReason.Expired);

                return true;
            }

            return false;
        }
    }
}