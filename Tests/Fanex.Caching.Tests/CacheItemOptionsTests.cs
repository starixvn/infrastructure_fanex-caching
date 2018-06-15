namespace Fanex.Caching.Tests
{
    using System;
    using AutoFixture;
    using Xunit;

    public class CacheItemOptionsTests
    {
        private readonly Fixture _fixture;
        private readonly CacheItemOptions _cacheItemOptions;

        public CacheItemOptionsTests()
        {
            _fixture = new Fixture();
            _cacheItemOptions = _fixture.Create<CacheItemOptions>();
        }

        [Fact]
        public void SetAbsoluteExpirationTakesAnyDateTime()
        {
            // Arrange.
            var absolute = _fixture.Create<DateTimeOffset>();

            // Act.
            _cacheItemOptions.SetAbsoluteExpiration(absolute);

            // Assert.
            Assert.Equal(_cacheItemOptions.AbsoluteExpiration, absolute);
        }

        [Fact]
        public void SetAbsoluteExpirationWithZeroTimespanThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var relative = TimeSpan.Zero;

            // Act.
            var exception = Record.Exception(() => _cacheItemOptions.SetAbsoluteExpiration(relative));

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void SetAbsoluteExpirationOnlyAcceptsNonZeroRelativeTimespan()
        {
            // Arrange.
            var relative = _fixture.Create<TimeSpan>();

            // Act.
            _cacheItemOptions.SetAbsoluteExpiration(relative);

            // Assert.
            Assert.Equal(_cacheItemOptions.RelativeExpiration, relative);
        }

        [Fact]
        public void SetSlidingExpirationWithZeroOffsetThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var offset = TimeSpan.Zero;

            // Act.
            var exception = Record.Exception(() => _cacheItemOptions.SetSlidingExpiration(offset));

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void SetSlidingExpirationOnlyAcceptsNonZeroOffset()
        {
            // Arrange.
            var offset = _fixture.Create<TimeSpan>();

            // Act.
            _cacheItemOptions.SetSlidingExpiration(offset);

            // Assert.
            Assert.Equal(_cacheItemOptions.SlidingExpiration, offset);
        }

        [Fact]
        public void OffExpirationClearsAbsoluteExpirationValue()
        {
            // Act.
            _cacheItemOptions.OffExpiration();

            // Assert.
            Assert.Null(_cacheItemOptions.AbsoluteExpiration);
        }

        [Fact]
        public void OffExpirationClearsRelativeExpirationValue()
        {
            // Act.
            _cacheItemOptions.OffExpiration();

            // Assert.
            Assert.Null(_cacheItemOptions.RelativeExpiration);
        }

        [Fact]
        public void OffExpirationClearsSlidingExpirationValue()
        {
            // Act.
            _cacheItemOptions.OffExpiration();

            // Assert.
            Assert.Null(_cacheItemOptions.SlidingExpiration);
        }
    }
}