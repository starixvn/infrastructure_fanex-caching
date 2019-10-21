namespace Fanex.Caching
{
    using System;

    public class CacheItemOptions
    {
        public static readonly TimeSpan DefaultRelativeExpiration = TimeSpan.FromSeconds(120);

        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? RelativeExpiration { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        public CacheItemOptions SetAbsoluteExpiration(DateTimeOffset absolute)
        {
            AbsoluteExpiration = absolute;

            return this;
        }

        public CacheItemOptions SetAbsoluteExpiration(TimeSpan relative)
        {
            if (relative <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(relative),
                    relative,
                    $"The {nameof(relative)} expiration value must not be zero.");
            }

            RelativeExpiration = relative;

            return this;
        }

        public CacheItemOptions SetSlidingExpiration(TimeSpan offset)
        {
            if (offset <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(offset),
                    offset,
                    "The sliding expiration value must not be zero.");
            }

            SlidingExpiration = offset;

            return this;
        }

        public CacheItemOptions UseDefaultExpiration()
        {
            RelativeExpiration = DefaultRelativeExpiration;

            return this;
        }

        public TimeSpan? CalculateRelativeExpiration(DateTimeOffset now)
        {
            if (RelativeExpiration.HasValue && AbsoluteExpiration.HasValue)
            {
                if (now + RelativeExpiration.Value <= AbsoluteExpiration.Value)
                {
                    return RelativeExpiration.Value;
                }

                return AbsoluteExpiration.Value - now;
            }

            if (RelativeExpiration.HasValue)
            {
                return RelativeExpiration.Value;
            }

            if (AbsoluteExpiration.HasValue)
            {
                return AbsoluteExpiration.Value - now;
            }

            return null;
        }

        public DateTimeOffset? CalculateAbsoluteExpiration(DateTimeOffset now)
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