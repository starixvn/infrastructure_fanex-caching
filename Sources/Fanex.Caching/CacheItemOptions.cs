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
                    "The relative expiration value must be positive.");
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
                    "The sliding expiration value must be positive.");
            }

            SlidingExpiration = offset;

            return this;
        }

        public CacheItemOptions UseDefaultExpiration()
        {
            RelativeExpiration = DefaultRelativeExpiration;

            return this;
        }
    }
}