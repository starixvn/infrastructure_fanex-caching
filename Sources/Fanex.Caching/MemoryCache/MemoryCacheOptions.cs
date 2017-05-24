namespace Fanex.Caching.Memory
{
    using System;

#if COREFX

    using Microsoft.Extensions.Options;

    public class MemoryCacheOptions : IOptions<MemoryCacheOptions>
    {
        MemoryCacheOptions IOptions<MemoryCacheOptions>.Value => this;

#else

    public class MemoryCacheOptions
    {
#endif

        public bool CompactOnMemoryPressure { get; set; }
            = true;

        public TimeSpan ExpirationScanFrequency { get; set; }
            = TimeSpan.FromMinutes(1);
    }
}