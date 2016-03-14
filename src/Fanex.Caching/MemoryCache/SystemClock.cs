namespace Fanex.Caching.MemoryCache
{
    using System;

    internal interface SystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}