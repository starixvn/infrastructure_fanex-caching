namespace Fanex.Caching.Memory
{
    using System;

    public class MemoryCacheOptions
    {
        private ISystemClock _clock = new SystemClock();
        private TimeSpan _expirationScanFrequency = TimeSpan.FromMinutes(1);
        private double _compactionPercentage = 0.1;

        public ISystemClock Clock
        {
            get => _clock;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), $"{nameof(Clock)} is required.");
                }

                _clock = value;
            }
        }

        public bool CompactOnMemoryPressure { get; set; }
            = true;

        public TimeSpan ExpirationScanFrequency
        {
            get => _expirationScanFrequency;
            set
            {
                if (value == TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        "Expiration scan frequency must not be zero.");
                }

                _expirationScanFrequency = value;
            }
        }

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