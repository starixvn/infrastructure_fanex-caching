namespace Fanex.Caching.Tests.MemoryCache
{
    using System;
    using Memory;
    using Mocks;
    using NSubstitute;
    using Xunit;

    public class MemoryCacheItemOptionsTests
    {
        private MemoryCacheItemOptions _options;
        private SystemClockMock _clock;

        public MemoryCacheItemOptionsTests()
        {
            _options = new MemoryCacheItemOptions();
            _clock = new SystemClockMock();
        }

        [Fact]
        public void RegisterPostEvictionCallback_CallbackIsNull_ThrowArgumentNullException()
        {
            // Act & Assert.
            Assert.Throws<ArgumentNullException>("callback", () => _options.RegisterPostEvictionCallback(null));
        }

        [Fact]
        public void RegisterPostEvictionCallback_CallbackIsNotNull_AddNewItemToPostEvictionCallbacks()
        {
            // Arrange.
            var state = new object();

            // Act.
            _options.RegisterPostEvictionCallback(Substitute.For<PostEvictionDelegate>(), state);

            // Assert.
            Assert.Equal(1, _options.PostEvictionCallbacks.Count);

            PostEvictionCallbackRegistration registeredCallback = _options.PostEvictionCallbacks[0];

            Assert.NotNull(registeredCallback);
            Assert.Same(state, registeredCallback.State);
        }

        [Fact]
        public void CalculateAbsoluteExpiration_RelativeExpirationHasValue_ReturnRelativeExpirationFromNow()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;
            _options.SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                .SetAbsoluteExpiration(_clock.UtcNow + TimeSpan.FromSeconds(5));

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(checkingTime + TimeSpan.FromMinutes(1), result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpiration_AbsoluteExpirationDoesNotExpire_ReturnAbsoluteExpiration()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;
            DateTimeOffset absoluteExpiration = _clock.UtcNow + TimeSpan.FromSeconds(5);
            _options.SetAbsoluteExpiration(absoluteExpiration);

            // Act.
            DateTimeOffset? result = _options.CalculateAbsoluteExpiration(checkingTime);

            // Assert.
            Assert.NotNull(result);
            Assert.Equal(absoluteExpiration, result.Value);
        }

        [Fact]
        public void CalculateAbsoluteExpiration_AbsoluteExpirationExpires_ThrowArgumentOutOfRangeException()
        {
            // Arrange.
            DateTimeOffset checkingTime = _clock.UtcNow;
            DateTimeOffset absoluteExpiration = _clock.UtcNow + TimeSpan.FromSeconds(-5);
            _options.SetAbsoluteExpiration(absoluteExpiration);

            // Act & Assert.
            Assert.Throws<ArgumentOutOfRangeException>("AbsoluteExpiration", () => _options.CalculateAbsoluteExpiration(checkingTime));
        }
    }
}