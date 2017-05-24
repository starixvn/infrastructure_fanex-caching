namespace Fanex.Caching.Memory
{
    using System;

    internal class SystemClockImpl : SystemClock
    {
        DateTimeOffset SystemClock.UtcNow => DateTimeOffset.UtcNow;
    }
}