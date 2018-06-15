namespace Fanex.Caching.Memory
{
    using System;

    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}