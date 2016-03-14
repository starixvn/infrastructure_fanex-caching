namespace Fanex.Caching
{
    using System;

    public class CacheItemOptions
    {
        private DateTimeOffset? _absoluteExpiration;
        private TimeSpan? _relativeExpiration;
        private TimeSpan? _slidingExpiration;

        public static readonly TimeSpan DefaultRelativeExpiration = TimeSpan.FromSeconds(120);

        public DateTimeOffset? AbsoluteExpiration
        {
            get
            {
                return _absoluteExpiration;
            }

            set
            {
                _absoluteExpiration = value;
            }
        }

        public TimeSpan? RelativeExpiration
        {
            get
            {
                return _relativeExpiration;
            }

            set
            {
                _relativeExpiration = value;
            }
        }

        public TimeSpan? SlidingExpiration
        {
            get
            {
                return _slidingExpiration;
            }

            set
            {
                _slidingExpiration = value;
            }
        }

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
    }
}