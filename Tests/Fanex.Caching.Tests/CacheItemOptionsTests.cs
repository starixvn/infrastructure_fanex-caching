namespace Fanex.Caching.Tests
{
    using System;
    using AutoFixture;
    using Xunit;

    public class CacheItemOptionsTests
    {
        private readonly Fixture _fixture;
        private readonly CacheItemOptions _options;
        private readonly SystemClockMock _clock;

        public CacheItemOptionsTests()
        {
            _fixture = new Fixture();
            _options = new CacheItemOptions();
            _clock = new SystemClockMock();
        }

        [Fact]
        public void SetAbsoluteExpirationTakesAnyDateTime()
        {
            // Arrange.
            var absolute = _fixture.Create<DateTimeOffset>();

            // Act.
            _options.SetAbsoluteExpiration(absolute);

            // Assert.
            Assert.Equal(_options.AbsoluteExpiration, absolute);
        }

        [Fact]
        public void SetAbsoluteExpirationWithZeroTimespanThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var relative = TimeSpan.Zero;

            // Act.
            var exception = Record.Exception(() => _options.SetAbsoluteExpiration(relative));

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void SetAbsoluteExpirationOnlyAcceptsNonZeroRelativeTimespan()
        {
            // Arrange.
            var relative = _fixture.Create<TimeSpan>();

            // Act.
            _options.SetAbsoluteExpiration(relative);

            // Assert.
            Assert.Equal(_options.RelativeExpiration, relative);
        }

        [Fact]
        public void SetSlidingExpirationWithZeroOffsetThrowsArgumentOutOfRangeException()
        {
            // Arrange.
            var offset = TimeSpan.Zero;

            // Act.
            var exception = Record.Exception(() => _options.SetSlidingExpiration(offset));

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void SetSlidingExpirationOnlyAcceptsNonZeroOffset()
        {
            // Arrange.
            var offset = _fixture.Create<TimeSpan>();

            // Act.
            _options.SetSlidingExpiration(offset);

            // Assert.
            Assert.Equal(_options.SlidingExpiration, offset);
        }

        [Fact]
        public void UseDefaultExpirationTakesDefaultRelativeExpiration()
        {
            // Act.
            _options.UseDefaultExpiration();

            // Assert.
            Assert.Equal(CacheItemOptions.DefaultRelativeExpiration, _options.RelativeExpiration);
        }

        [Fact]
        public void CalculateRelativeExpirationReturnsSetRelativeExpiration()
        {
            // Arrange.
            var relativeExpiration = _fixture.Create<TimeSpan>();

            _options.SetAbsoluteExpiration(relativeExpiration);

            // Act.
            var result = _options.CalculateRelativeExpiration(_clock.UtcNow);

            // Assert.
            Assert.Equal(relativeExpiration, result.Value);
        }

        [Fact]
        public void CalculateRelativeExpirationFromAbsoluteExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;
            var timeGap = _fixture.Create<TimeSpan>();
            var expirationTime = checkingTime + timeGap;

            _options.SetAbsoluteExpiration(expirationTime);

            // Act.
            var result = _options.CalculateRelativeExpiration(checkingTime);

            // Assert.
            Assert.Equal(timeGap, result.Value);
        }

        [Fact]
        public void CalculateRelativeExpirationPrioritizesSoonerAbsoluteExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;
            var timeGap = _fixture.Create<TimeSpan>();
            var absoluteExpiration = checkingTime + timeGap;
            var relativeExpiration = _fixture.Create<TimeSpan>() + timeGap;

            _options
                .SetAbsoluteExpiration(absoluteExpiration)
                .SetAbsoluteExpiration(relativeExpiration);

            // Act.
            var result = _options.CalculateRelativeExpiration(checkingTime);

            // Assert.
            Assert.Equal(timeGap, result.Value);
        }

        [Fact]
        public void CalculateRelativeExpirationPrioritizesSoonerRelativeExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;
            var relativeExpiration = _fixture.Create<TimeSpan>();
            var timeGap = _fixture.Create<TimeSpan>();
            var absoluteExpiration = checkingTime + relativeExpiration + timeGap;

            _options
                .SetAbsoluteExpiration(absoluteExpiration)
                .SetAbsoluteExpiration(relativeExpiration);

            // Act.
            var result = _options.CalculateRelativeExpiration(checkingTime);

            // Assert.
            Assert.Equal(relativeExpiration, result.Value);
        }

        [Fact]
        public void CalculateRelativeExpirationWithNoExpirationSetReturnsNull()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;

            // Act.
            var result = _options.CalculateRelativeExpiration(checkingTime);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void CalculateRelativeExpirationFromDefaultExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;

            _options.UseDefaultExpiration();

            // Act.
            var result = _options.CalculateRelativeExpiration(checkingTime);

            // Assert.
            Assert.Equal(CacheItemOptions.DefaultRelativeExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationReturnsSetAbsoluteExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;
            var expirationTime = checkingTime + _fixture.Create<TimeSpan>();

            _options.SetAbsoluteExpiration(expirationTime);

            // Act.
            var result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(expirationTime, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationFromRelativeExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;
            var relativeExpiration = _fixture.Create<TimeSpan>();

            _options.SetAbsoluteExpiration(relativeExpiration);

            // Act.
            var result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(checkingTime + relativeExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationPrioritizesSoonerAbsoluteExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;
            var timeGap = _fixture.Create<TimeSpan>();
            var relativeExpiration = _fixture.Create<TimeSpan>() + timeGap;
            var absoluteExpiration = checkingTime + timeGap;

            _options
                .SetAbsoluteExpiration(relativeExpiration)
                .SetAbsoluteExpiration(absoluteExpiration);

            // Act.
            var result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(absoluteExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationPrioritizesSoonerRelativeExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;
            var relativeExpiration = _fixture.Create<TimeSpan>();
            var timeGap = _fixture.Create<TimeSpan>();
            var absoluteExpiration = checkingTime + relativeExpiration + timeGap;

            _options
                .SetAbsoluteExpiration(relativeExpiration)
                .SetAbsoluteExpiration(absoluteExpiration);

            // Act.
            var result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(checkingTime + relativeExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationWithNoExpirationSetReturnsNull()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;

            // Act.
            var result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void CalculateAbsoluteExpirationFromDefaultExpiration()
        {
            // Arrange.
            var checkingTime = _clock.UtcNow;

            _options.UseDefaultExpiration();

            // Act.
            var result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(checkingTime + CacheItemOptions.DefaultRelativeExpiration, result.Value);
        }
    }
}