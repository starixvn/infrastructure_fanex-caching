namespace Fanex.Caching.MemoryCache
{
    using System;

    public class MemoryCacheOptions
    {
        public bool CompactOnMemoryPressure { get; set; }
            = true;

        public TimeSpan ExpirationScanFrequency { get; set; }
            = TimeSpan.FromMinutes(1);
    }
}