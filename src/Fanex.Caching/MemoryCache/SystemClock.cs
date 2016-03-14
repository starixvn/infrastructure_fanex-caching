namespace Fanex.Caching.Memory
{
    using System;

    internal interface SystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}