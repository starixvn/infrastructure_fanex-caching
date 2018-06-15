namespace Fanex.Caching.Memory
{
    using System;

    internal class SystemClock : ISystemClock
    {
        DateTimeOffset ISystemClock.UtcNow => DateTimeOffset.UtcNow;
    }
}