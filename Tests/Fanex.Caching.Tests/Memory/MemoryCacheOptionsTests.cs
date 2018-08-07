namespace Fanex.Caching.Tests.Memory
{
    using System;
    using Fanex.Caching.Memory;
    using Xunit;

    public class MemoryCacheOptionsTests
    {
        [Fact]
        public void NullSystemClockThrowsArgumentNullException()
        {
            // Act.
            var exception = Record.Exception(() => new MemoryCacheOptions
            {
                Clock = null
            });

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void ZeroExpirationScanFrequencyThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var zeroExpirationScanFrequency = TimeSpan.Zero;

            // Act.
            var exception = Record.Exception(() => new MemoryCacheOptions
            {
                ExpirationScanFrequency = zeroExpirationScanFrequency
            });

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void NegativeCompactionPercentageThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var negativeCompactionPercentage = -0.1;

            // Act.
            var exception = Record.Exception(() => new MemoryCacheOptions
            {
                CompactionPercentage = negativeCompactionPercentage
            });

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void GreaterThanOneCompactionPercentageThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var outOfRangeCompactionPercentage = 1.1;

            // Act.
            var exception = Record.Exception(() => new MemoryCacheOptions
            {
                CompactionPercentage = outOfRangeCompactionPercentage
            });

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }
    }
}