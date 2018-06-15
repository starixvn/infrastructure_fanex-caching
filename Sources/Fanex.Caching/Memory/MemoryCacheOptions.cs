namespace Fanex.Caching.Memory
{
    using System;

    public class MemoryCacheOptions
    {
        private double _compactionPercentage = 0.1;

        public ISystemClock Clock { get; set; }

        public bool CompactOnMemoryPressure { get; set; }
            = true;

        public TimeSpan ExpirationScanFrequency { get; set; }
            = TimeSpan.FromMinutes(1);

        public double CompactionPercentage
        {
            get => _compactionPercentage;
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        "Compaction percentage must be between 0 and 1 inclusive.");
                }

                _compactionPercentage = value;
            }
        }
    }
}