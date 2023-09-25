namespace Fanex.Caching.Tests;

using System;

internal class SystemClockMock : ISystemClock
{
    public SystemClockMock()
        => UtcNow = new DateTimeOffset(2018, 10, 10, 12, 34, 56, 789, new TimeSpan(0, 0, 0));

    public DateTimeOffset UtcNow { get; private set; }

    public void Add(TimeSpan addedTime)
        => UtcNow += addedTime;
}