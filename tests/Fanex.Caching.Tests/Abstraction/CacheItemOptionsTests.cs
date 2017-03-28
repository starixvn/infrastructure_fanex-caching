namespace Fanex.Caching.Tests.Abstraction
{
    using System;
    using Ploeh.AutoFixture;
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
        public void SetAbsoluteExpiration_AbsoluteExpirationEqualsNewValue()
        {
            // Arrange.
            var absolute = _fixture.Create<DateTimeOffset>();

            // Act.
            _cacheItemOptions.SetAbsoluteExpiration(absolute);

            // Assert.
            Assert.Equal(_cacheItemOptions.AbsoluteExpiration, absolute);
        }

        [Fact]
        public void SetAbsoluteExpiration_RelativeIsZero_ThrowArgumentOutOfRangeException()
        {
            // Arrange.
            var relative = TimeSpan.Zero;

            // Act.
            var exception = Record.Exception(() => _cacheItemOptions.SetAbsoluteExpiration(relative));

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void SetAbsoluteExpiration__RelativeIsNotZero_RelativeExpirationEqualsNewValue()
        {
            // Arrange.
            var relative = _fixture.Create<TimeSpan>();

            // Act.
            _cacheItemOptions.SetAbsoluteExpiration(relative);

            // Assert.
            Assert.Equal(_cacheItemOptions.RelativeExpiration, relative);
        }

        [Fact]
        public void SetSlidingExpiration_OffsetIsZero_ThrowArgumentOutOfRangeException()
        {
            // Arrange.
            var offset = TimeSpan.Zero;

            // Act.
            var exception = Record.Exception(() => _cacheItemOptions.SetSlidingExpiration(offset));

            // Assert.
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        [Fact]
        public void SetSlidingExpiration_OffsetIsNotZero_SlidingExpirationEqualsNewValue()
        {
            // Arrange.
            var offset = _fixture.Create<TimeSpan>();

            // Act.
            _cacheItemOptions.SetSlidingExpiration(offset);

            // Assert.
            Assert.Equal(_cacheItemOptions.SlidingExpiration, offset);
        }
    }
}