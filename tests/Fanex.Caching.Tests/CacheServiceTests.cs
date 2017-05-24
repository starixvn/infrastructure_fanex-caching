namespace Fanex.Caching.Tests
{
    using System;
    using System.Threading.Tasks;
    using NSubstitute;
    using Xunit;

    public class CacheServiceTests
    {
        private readonly Cache _cacheStorage;
        private readonly CacheService _cacheService;

        public CacheServiceTests()
        {
            _cacheStorage = Substitute.For<Cache>();
            _cacheService = new CacheServiceImpl(_cacheStorage);
        }

        [Fact]
        public void ContainAlwaysCallCacheStorageContainMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            _cacheService.Contain(testKey);

            // Assert.
            _cacheStorage.Received().Contain(Arg.Is(testKey));
        }

        [Fact]
        public async Task ContainAsyncAlwaysCallCacheStorageContainAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await _cacheService.ContainAsync(testKey);

            // Assert.
            await _cacheStorage.Received().ContainAsync(Arg.Is(testKey));
        }

        [Fact]
        public void GetAlwaysCallCacheStorageTryGetMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            _cacheService.Get<object>(testKey);

            // Assert.
            _cacheStorage.Received().TryGet<object>(Arg.Is(testKey), out _);
        }

        [Fact]
        public void GetReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            string testKey = "TestKey";

            _cacheStorage.TryGet<object>(testKey, out _).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            object result = _cacheService.Get<object>(testKey);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetReturnsStoredObject()
        {
            // Arrange.
            var testKey = "TestKey";
            var testValue = new object();

            _cacheStorage.TryGet<object>(testKey, out _).Returns(parameters =>
            {
                parameters[1] = testValue;

                return true;
            });

            // Act.
            object result = _cacheService.Get<object>(testKey);

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public async Task GetAsyncAlwaysCallCacheStorageTryGetAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await _cacheService.GetAsync<object>(testKey);

            // Assert.
            await _cacheStorage.Received().TryGetAsync<object>(Arg.Is(testKey));
        }

        [Fact]
        public async Task GetAsyncReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            string testKey = "TestKey";

            _cacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await _cacheService.GetAsync<object>(testKey);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsyncReturnsStoredObject()
        {
            // Arrange.
            var testKey = "TestKey";
            var testValue = new object();

            _cacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(true, testValue));

            // Act.
            var result = await _cacheService.GetAsync<object>(testKey);

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public void TryGetAlwaysCallCacheStorageTryGetMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            _cacheService.TryGet<object>(testKey, out _);

            // Assert.
            _cacheStorage.Received().TryGet<object>(Arg.Is(testKey), out _);
        }

        [Fact]
        public async Task TryGetAsyncAlwaysCallCacheStorageTryGetAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await _cacheService.TryGetAsync<object>(testKey);

            // Assert.
            await _cacheStorage.Received().TryGetAsync<object>(Arg.Is(testKey));
        }

        [Fact]
        public void GetOrSetReturnsExistingObject()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();

            _cacheStorage.TryGet<object>(testKey, out _).Returns(parameters =>
            {
                parameters[1] = testValue;

                return true;
            });

            // Act.
            object result = _cacheService.GetOrSet(testKey, () => new object(), Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            string testKey = "TestKey";

            _cacheStorage.TryGet<object>(testKey, out _).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            object result = _cacheService.GetOrSet<object>(testKey, null, Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetOrSetMissingKeyAddsNewValueAndReturnsIt()
        {
            // Arrange.
            string testKey = "TestKey";
            var newValue = new object();
            var testOptions = Arg.Any<CacheItemOptions>();

            _cacheStorage.TryGet<object>(testKey, out _).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            object result = _cacheService.GetOrSet(testKey, () => newValue, testOptions);

            // Assert.
            _cacheStorage.Received().Set(Arg.Is(testKey), Arg.Is(newValue), Arg.Is(testOptions));
            Assert.Same(newValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithoutCacheItemOptionStoresNewValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            string testKey = "TestKey";
            var newValue = new object();

            _cacheStorage.TryGet<object>(testKey, out _).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            object result = _cacheService.GetOrSet(testKey, () => newValue);

            // Assert.
            _cacheStorage
                .Received()
                .Set(
                    Arg.Is(testKey),
                    Arg.Is(newValue),
                    Arg.Is<CacheItemOptions>(testOptions => testOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
            Assert.Same(newValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncReturnsExistingObject()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();

            _cacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(true, testValue));

            // Act.
            object result = await _cacheService.GetOrSetAsync(testKey, () => new object(), Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            string testKey = "TestKey";

            _cacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await _cacheService.GetOrSetAsync<object>(testKey, null, Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyAddsNewValueAndReturnsIt()
        {
            // Arrange.
            string testKey = "TestKey";
            var newValue = new object();
            var testOptions = Arg.Any<CacheItemOptions>();

            _cacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await _cacheService.GetOrSetAsync(testKey, () => newValue, testOptions);

            // Assert.
            await _cacheStorage.Received().SetAsync(Arg.Is(testKey), Arg.Is(newValue), Arg.Is(testOptions));
            Assert.Same(newValue, result);
        }

        [Fact]
        public async Task GetOrSetAsyncMissingKeyWithMissingCacheItemOptionStoresNewValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            string testKey = "TestKey";
            var newValue = new object();

            _cacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await _cacheService.GetOrSetAsync(testKey, () => newValue);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(testKey),
                    Arg.Is(newValue),
                    Arg.Is<CacheItemOptions>(testOptions => testOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
            Assert.Same(newValue, result);
        }

        [Fact]
        public void RemoveAlwaysCallCacheStorageRemoveMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            _cacheService.Remove(testKey);

            // Assert.
            _cacheStorage.Received().Remove(Arg.Is(testKey));
        }

        [Fact]
        public async Task RemoveAsyncAlwaysCallCacheStorageRemoveAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";

            // Act.
            await _cacheService.RemoveAsync(testKey);

            // Assert.
            await _cacheStorage.Received().RemoveAsync(Arg.Is(testKey));
        }

        [Fact]
        public void SetAlwaysCallCacheStorageSetMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();
            var testOptions = new CacheItemOptions();

            // Act.
            _cacheService.Set(testKey, testValue, testOptions);

            // Assert.
            _cacheStorage.Received().Set(Arg.Is(testKey), Arg.Is(testValue), Arg.Is(testOptions));
        }

        [Fact]
        public void SetWithoutCachingOptionStoresObjectWithDefaultRelativeExpiration()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();

            // Act.
            _cacheService.Set(testKey, testValue);

            // Assert.
            _cacheStorage
                .Received()
                .Set(
                    Arg.Is(testKey),
                    Arg.Is(testValue),
                    Arg.Is<CacheItemOptions>(testOptions => testOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }

        [Fact]
        public async Task SetAsyncAlwaysCallCacheStorageSetAsyncMethod()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();
            var testOptions = new CacheItemOptions();

            // Act.
            await _cacheService.SetAsync(testKey, testValue, testOptions);

            // Assert.
            await _cacheStorage.Received().SetAsync(Arg.Is(testKey), Arg.Is(testValue), Arg.Is(testOptions));
        }

        [Fact]
        public async Task SetAsyncWithoutCachingOptionStoresObjectWithDefaultRelativeExpiration()
        {
            // Arrange.
            string testKey = "TestKey";
            var testValue = new object();

            // Act.
            await _cacheService.SetAsync(testKey, testValue);

            // Assert.
            await _cacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(testKey),
                    Arg.Is(testValue),
                    Arg.Is<CacheItemOptions>(testOptions => testOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }
    }
}