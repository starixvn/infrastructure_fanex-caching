namespace Fanex.Caching
{
    using System;

    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}