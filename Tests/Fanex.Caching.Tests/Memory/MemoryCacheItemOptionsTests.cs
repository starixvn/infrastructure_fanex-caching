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

        public MemoryCacheItemOptionsTests()
        {
            _fixture = new Fixture();
            _options = new MemoryCacheItemOptions();
        }

        [Fact]
        public void RegisterNullPostEvictionCallbackThrowsArgumentNullException()
        {
            // Act.
            var exception = Record.Exception(() => _options.RegisterPostEvictionCallback(null));

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
            const int expectedNumberOfCallbacks = 1;

            Assert.Equal(expectedNumberOfCallbacks, _options.PostEvictionCallbacks.Count);
            Assert.Same(state, _options.PostEvictionCallbacks[0].State);
        }

        [Fact]
        public void SetCachePriorityTakesAnyDefinedCachePriority()
        {
            // Arrange.
            const CachePriority priority = CachePriority.High;

            // Act.
            _options.SetCachePriority(priority);

            // Assert.
            Assert.Equal(priority, _options.Priority);
        }
    }
}