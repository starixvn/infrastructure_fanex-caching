namespace Fanex.Caching.Tests
{
    using System;
    using System.Threading.Tasks;
    using NSubstitute;
    using Xunit;

    public class ConfiguredCacheServiceTests
    {
        private readonly CacheService _cacheService;

        public ConfiguredCacheServiceTests()
        {
            _cacheService = Substitute.For<CacheService>();
            ConfiguredCacheService.Initialize(_cacheService);
        }

#pragma warning disable CS0618 // Type or member is obsolete

        [Fact]
        public void SwitchCacheStorageThrowsArgumentNullExceptionWhenNewCacheStorageIsNull()
        {
            // Arrange.
            Cache newCacheStorage = null;

            // Act & Assert.
            Assert.Throws<ArgumentNullException>("newCacheStorage", () => ConfiguredCacheService.SwitchCacheStorage(newCacheStorage));
        }

        [Fact]
        public void SwitchCacheStorageSetsNewCacheStorangeToLocalCacheStorage()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();

            // Act.
            ConfiguredCacheService.SwitchCacheStorage(newCacheStorage);

            // Assert.
            Assert.Same(newCacheStorage, ConfiguredCacheService.CacheStorage);
        }

#pragma warning restore CS0618 // Type or member is obsolete

        [Fact]
        public void ContainAlwaysCallBackgroundInstaceContainMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            ConfiguredCacheService.Contain(testKey);

            // Assert.
            _cacheService.Received().Contain(Arg.Is(testKey));
        }

        [Fact]
        public async Task ContainAsyncAlwaysCallBackgroundInstaceContainAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await ConfiguredCacheService.ContainAsync(testKey);

            // Assert.
            await _cacheService.Received().ContainAsync(Arg.Is(testKey));
        }

        [Fact]
        public void GetAlwaysCallBackgroundInstaceGetMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            ConfiguredCacheService.Get<object>(testKey);

            // Assert.
            _cacheService.Received().Get<object>(Arg.Is(testKey));
        }

        [Fact]
        public async Task GetAsyncAlwaysCallBackgroundInstaceGetAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await ConfiguredCacheService.GetAsync<object>(testKey);

            // Assert.
            await _cacheService.Received().GetAsync<object>(Arg.Is(testKey));
        }

        [Fact]
        public void TryGetAlwaysCallBackgroundInstaceTryGetMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            object result;

            // Act.
            ConfiguredCacheService.TryGet(testKey, out result);

            // Assert.
            _cacheService.Received().TryGet(Arg.Is(testKey), out result);
        }

        [Fact]
        public async Task TryGetAsyncAlwaysCallBackgroundInstaceTryGetAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await ConfiguredCacheService.TryGetAsync<object>(testKey);

            // Assert.
            await _cacheService.Received().TryGetAsync<object>(Arg.Is(testKey));
        }

        [Fact]
        public void GetOrSetAlwaysCallBackgroundInstaceGetOrSetMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testOptions = new CacheItemOptions();

            // Act.
            ConfiguredCacheService.GetOrSet(testKey, () => new object(), testOptions);

            // Assert.
            _cacheService.Received().GetOrSet(Arg.Is(testKey), Arg.Any<Func<object>>(), Arg.Is(testOptions));
        }

        [Fact]
        public async Task GetOrSetAsyncAlwaysCallBackgroundInstaceGetOrSetAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testOptions = new CacheItemOptions();

            // Act.
            await ConfiguredCacheService.GetOrSetAsync(testKey, () => new object(), testOptions);

            // Assert.
            await _cacheService.Received().GetOrSetAsync(Arg.Is(testKey), Arg.Any<Func<object>>(), Arg.Is(testOptions));
        }

        [Fact]
        public void RemoveAlwaysCallBackgroundInstaceRemoveMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            ConfiguredCacheService.Remove(testKey);

            // Assert.
            _cacheService.Received().Remove(Arg.Is(testKey));
        }

        [Fact]
        public async Task RemoveAsyncAlwaysCallBackgroundInstaceRemoveAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await ConfiguredCacheService.RemoveAsync(testKey);

            // Assert.
            await _cacheService.Received().RemoveAsync(Arg.Is(testKey));
        }

        [Fact]
        public void SetAlwaysCallBackgroundInstaceSetMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();
            var testOptions = new CacheItemOptions();

            // Act.
            ConfiguredCacheService.Set(testKey, testValue, testOptions);

            // Assert.
            _cacheService.Received().Set(Arg.Is(testKey), Arg.Is(testValue), Arg.Is(testOptions));
        }

        [Fact]
        public void SetWithoutCacheItemOptionsAlwaysCallBackgroundInstaceSetWithoutCacheItemOptionsMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();

            // Act.
            ConfiguredCacheService.Set(testKey, testValue);

            // Assert.
            _cacheService.Received().Set(Arg.Is(testKey), Arg.Is(testValue));
        }

        [Fact]
        public async Task SetAsyncAlwaysCallBackgroundInstaceSetAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();
            var testOptions = new CacheItemOptions();

            // Act.
            await ConfiguredCacheService.SetAsync(testKey, testValue, testOptions);

            // Assert.
            await _cacheService.Received().SetAsync(Arg.Is(testKey), Arg.Is(testValue), Arg.Is(testOptions));
        }

        [Fact]
        public async Task SetAsyncWithoutCacheItemOptionsAlwaysCallBackgroundInstaceSetAsyncWithoutCacheItemOptionsMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();

            // Act.
            await ConfiguredCacheService.SetAsync(testKey, testValue);

            // Assert.
            await _cacheService.Received().SetAsync(Arg.Is(testKey), Arg.Is(testValue));
        }
    }
}