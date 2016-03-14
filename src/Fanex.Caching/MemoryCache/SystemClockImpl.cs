namespace Fanex.Caching.MemoryCache
{
    using System;

    internal class SystemClockImpl : SystemClock
    {
        DateTimeOffset SystemClock.UtcNow
        {
            get
            {
                return DateTimeOffset.UtcNow;
            }
        }
    }
}