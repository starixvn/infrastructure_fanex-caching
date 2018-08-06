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

        public new MemoryCacheItemOptions SetAbsoluteExpiration(DateTimeOffset absolute)
        {
            base.SetAbsoluteExpiration(absolute);

            return this;
        }

        public new MemoryCacheItemOptions SetAbsoluteExpiration(TimeSpan relative)
        {
            base.SetAbsoluteExpiration(relative);

            return this;
        }

        public new MemoryCacheItemOptions SetSlidingExpiration(TimeSpan offset)
        {
            base.SetSlidingExpiration(offset);

            return this;
        }

        public new MemoryCacheItemOptions UseDefaultExpiration()
        {
            base.UseDefaultExpiration();

            return this;
        }

        internal DateTimeOffset? CalculateAbsoluteExpiration(DateTimeOffset now)
        {
            if (RelativeExpiration.HasValue && AbsoluteExpiration.HasValue)
            {
                if (AbsoluteExpiration.Value <= now + RelativeExpiration.Value)
                {
                    return AbsoluteExpiration.Value;
                }
                return now + RelativeExpiration.Value;
            }
            if (RelativeExpiration.HasValue)
            {
                return now + RelativeExpiration.Value;
            }
            if (AbsoluteExpiration.HasValue)
            {
                return AbsoluteExpiration.Value;
            }
            return null;
        }
    }
}