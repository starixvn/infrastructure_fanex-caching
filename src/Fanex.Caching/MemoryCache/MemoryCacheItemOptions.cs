namespace Fanex.Caching.Memory
{
    using System;
    using System.Collections.Generic;

    public class MemoryCacheItemOptions : CacheItemOptions
    {
        internal CachePriority Priority { get; private set; }
            = CachePriority.Normal;

        internal IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }
            = new List<PostEvictionCallbackRegistration>();

        public CacheItemOptions RegisterPostEvictionCallback(PostEvictionDelegate callback)
        {
            return RegisterPostEvictionCallback(callback, null);
        }

        public CacheItemOptions RegisterPostEvictionCallback(PostEvictionDelegate callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration()
            {
                EvictionCallback = callback,
                State = state
            });

            return this;
        }

        public CacheItemOptions SetCachePriority(CachePriority priority)
        {
            Priority = priority;

            return this;
        }

        public DateTimeOffset? CalculateAbsoluteExpiration(DateTimeOffset now)
        {
            if (RelativeExpiration.HasValue)
            {
                return now + RelativeExpiration;
            }

            if (AbsoluteExpiration.HasValue && AbsoluteExpiration <= now)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(AbsoluteExpiration),
                    AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }

            return AbsoluteExpiration;
        }
    }
}