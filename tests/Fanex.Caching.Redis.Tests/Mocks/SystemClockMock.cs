namespace Fanex.Caching.Tests.Mocks
{
    using System;

    public class SystemClockMock
    {
        public SystemClockMock()
            => UtcNow = new DateTimeOffset(2016, 1, 1, 12, 34, 56, 789, new TimeSpan(0, 0, 0));

        public DateTimeOffset UtcNow { get; set; }

        public void Add(TimeSpan addedTime)
            => UtcNow = UtcNow + addedTime;
    }
}