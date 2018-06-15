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

        public MemoryCacheItemOptions RegisterPostEvictionCallback(PostEvictionDelegate callback)
            => RegisterPostEvictionCallback(callback, null);

        public MemoryCacheItemOptions RegisterPostEvictionCallback(PostEvictionDelegate callback, object state)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
            {
                EvictionCallback = callback,
                State = state
            });

            return this;
        }

        public MemoryCacheItemOptions SetCachePriority(CachePriority priority)
        {
            Priority = priority;

            return this;
        }

        internal DateTimeOffset? CalculateAbsoluteExpiration(DateTimeOffset now)
        {
            if (AbsoluteExpiration.HasValue)
            {
                return AbsoluteExpiration;
            }

            if (RelativeExpiration.HasValue)
            {
                return now + RelativeExpiration;
            }

            return null;
        }
    }
}