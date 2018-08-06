namespace Fanex.Caching.Tests.Memory
{
    using System;
    using AutoFixture;
    using Fanex.Caching.Memory;
    using Xunit;

    public class MemoryCacheItemOptionsTests
    {
        private readonly Fixture _fixture;
        private readonly MemoryCacheItemOptions _options;
        private readonly SystemClockMock _clock;

        public MemoryCacheItemOptionsTests()
        {
            _fixture = new Fixture();
            _options = new MemoryCacheItemOptions();
            _clock = new SystemClockMock();
        }

        [Fact]
        public void RegisterNullPostEvictionCallbackThrowsArgumentNullException()
        {
            // Act.
            Exception exception = Record.Exception(() => _options.RegisterPostEvictionCallback(null));

            // Assert.
            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void RegisterPostEvictionCallbackAddsCallbackToPostEvictionCallbacks()
        {
            // Arrange.
            var state = _fixture.Create<object>();

            // Act.
            _options.RegisterPostEvictionCallback(_fixture.Create<PostEvictionDelegate>(), state);

            // Assert.
            Assert.Equal(1, _options.PostEvictionCallbacks.Count);
            Assert.Same(state, _options.PostEvictionCallbacks[0]?.State);
        }

        [Fact]
        public void CalculateAbsoluteExpirationReturnsSetAbsoluteExpiration()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;
            DateTimeOffset expirationTime = checkingTime + TimeSpan.FromSeconds(5);

            _options.SetAbsoluteExpiration(expirationTime);

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(expirationTime, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationFromRelativeExpiration()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;
            TimeSpan relativeExpiration = TimeSpan.FromMinutes(1);

            _options.SetAbsoluteExpiration(relativeExpiration);

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(checkingTime + relativeExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationPrioritizesSoonerAbsoluteExpiration()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;
            TimeSpan relativeExpiration = TimeSpan.FromSeconds(6);
            DateTimeOffset absoluteExpiration = checkingTime + TimeSpan.FromSeconds(5);

            _options
                .SetAbsoluteExpiration(absoluteExpiration)
                .SetAbsoluteExpiration(relativeExpiration);

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(absoluteExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationPrioritizesSoonerRelativeExpiration()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;
            TimeSpan relativeExpiration = TimeSpan.FromSeconds(1);
            DateTimeOffset absoluteExpiration = checkingTime + TimeSpan.FromSeconds(2);

            _options
                .SetAbsoluteExpiration(relativeExpiration)
                .SetAbsoluteExpiration(absoluteExpiration);

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(checkingTime + relativeExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpirationWithNoExpirationSetReturnsNull()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void CalculateAbsoluteExpirationFromDefaultExpiration()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;

            _options.UseDefaultExpiration();

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.Equal(checkingTime + CacheItemOptions.DefaultRelativeExpiration, result.Value);
        }

        [Fact]
        public void SetCachePriorityTakesAnyDefinedCachePriority()
        {
            // Arrange.
            var priority = CachePriority.High;

            // Act.
            _options.SetCachePriority(priority);

            // Assert.
            Assert.Equal(priority, _options.Priority);
        }
    }
}