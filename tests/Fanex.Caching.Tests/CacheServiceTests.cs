namespace Fanex.Caching.Tests
{
    using System;
    using NSubstitute;
    using Xunit;

    public class CacheServiceTests
    {
        [Fact]
        public void SwitchCacheStorageThrowsArgumentNullExceptionWhenNewCacheStorageIsNull()
        {
            // Arrange.
            Cache newCacheStorage = null;

            // Act & Assert.
            Assert.Throws<ArgumentNullException>("newCacheStorage", () => CacheService.SwitchCacheStorage(newCacheStorage));
        }

        [Fact]
        public void SwitchCacheStorageSetsNewCacheStorangeToLocalCacheStorage()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();

            // Act.
            CacheService.SwitchCacheStorage(newCacheStorage);

            // Assert.
            Assert.Same(newCacheStorage, CacheService.CacheStorage);
        }

        [Fact]
        public void ContainAlwaysCallCacheStorageContainMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            CacheService.Contain(testKey);

            // Assert.
            newCacheStorage.Received().Contain(Arg.Is(testKey));
        }

        [Fact]
        public async void ContainAsyncAlwaysCallCacheStorageContainAsyncMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            await CacheService.ContainAsync(testKey);

            // Assert.
            await newCacheStorage.Received().ContainAsync(Arg.Is(testKey));
        }

        [Fact]
        public void GetAlwaysCallCacheStorageTryGetMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            CacheService.Get<object>(testKey);

            // Assert.
            object result = null;
            newCacheStorage.Received().TryGet(Arg.Is(testKey), out result);
        }

        [Fact]
        public void GetReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            object testValue = null;

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGet(testKey, out testValue).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            object result = CacheService.Get<object>(testKey);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetReturnsStoredObject()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            var testKey = "TestKey";
            var testValue = new object();
            object result = null;

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGet(testKey, out result).Returns(parameters =>
            {
                parameters[1] = testValue;

                return true;
            });

            // Act.
            result = CacheService.Get<object>(testKey);

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public async void GetAsyncAlwaysCallCacheStorageTryGetAsyncMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(true, new object()));

            // Act.
            await CacheService.GetAsync<object>(testKey);

            // Assert.
            await newCacheStorage.Received().TryGetAsync<object>(Arg.Is(testKey));
        }

        [Fact]
        public async void GetAsyncReturnsDefaultValueForMissingKey()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await CacheService.GetAsync<object>(testKey);

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async void GetAsyncReturnsStoredObject()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            var testKey = "TestKey";
            var testValue = new object();

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(true, testValue));

            // Act.
            var result = await CacheService.GetAsync<object>(testKey);

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public void TryGetAlwaysCallCacheStorageTryGetMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            object result = null;

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            CacheService.TryGet(testKey, out result);

            // Assert.
            newCacheStorage.Received().TryGet(Arg.Is(testKey), out result);
        }

        [Fact]
        public async void TryGetAsyncAlwaysCallCacheStorageTryGetAsyncMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(true, new object()));

            // Act.
            await CacheService.TryGetAsync<object>(testKey);

            // Assert.
            await newCacheStorage.Received().TryGetAsync<object>(Arg.Is(testKey));
        }

        [Fact]
        public void GetOrSetReturnsExistingObject()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();
            object result = null;

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGet(testKey, out result).Returns(parameters =>
            {
                parameters[1] = testValue;

                return true;
            });

            // Act.
            result = CacheService.GetOrSet(testKey, () => new object(), Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();
            object result = null;

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGet(testKey, out result).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            result = CacheService.GetOrSet<object>(testKey, null, Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public void GetOrSetMissingKeyAddsNewValueAndReturnsIt()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var newValue = new object();
            object result = null;
            var testOptions = Arg.Any<CacheItemOptions>();

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGet(testKey, out result).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            result = CacheService.GetOrSet(testKey, () => newValue, testOptions);

            // Assert.
            newCacheStorage.Received().Set(Arg.Is(testKey), Arg.Is(newValue), Arg.Is(testOptions));
            Assert.Same(newValue, result);
        }

        [Fact]
        public void GetOrSetMissingKeyWithMissingCacheItemOptionStoresNewValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var newValue = new object();
            object result = null;

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGet(testKey, out result).Returns(parameters =>
            {
                parameters[1] = null;

                return false;
            });

            // Act.
            result = CacheService.GetOrSet(testKey, () => newValue);

            // Assert.
            newCacheStorage
                .Received()
                .Set(
                    Arg.Is(testKey),
                    Arg.Is(newValue),
                    Arg.Is<CacheItemOptions>(testOptions => testOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
            Assert.Same(newValue, result);
        }

        [Fact]
        public async void GetOrSetAsyncReturnsExistingObject()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(true, testValue));

            // Act.
            object result = await CacheService.GetOrSetAsync(testKey, () => new object(), Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Same(testValue, result);
        }

        [Fact]
        public async void GetOrSetAsyncMissingKeyWithInvalidValueFactoryReturnsNull()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await CacheService.GetOrSetAsync<object>(testKey, null, Arg.Any<CacheItemOptions>());

            // Assert.
            Assert.Null(result);
        }

        [Fact]
        public async void GetOrSetAsyncMissingKeyAddsNewValueAndReturnsIt()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var newValue = new object();
            var testOptions = Arg.Any<CacheItemOptions>();

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await CacheService.GetOrSetAsync(testKey, () => newValue, testOptions);

            // Assert.
            await newCacheStorage.Received().SetAsync(Arg.Is(testKey), Arg.Is(newValue), Arg.Is(testOptions));
            Assert.Same(newValue, result);
        }

        [Fact]
        public async void GetOrSetAsyncMissingKeyWithMissingCacheItemOptionStoresNewValueWithDefaultRelativeExpiration()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var newValue = new object();

            CacheService.SwitchCacheStorage(newCacheStorage);
            newCacheStorage.TryGetAsync<object>(testKey).Returns(new Tuple<bool, object>(false, null));

            // Act.
            object result = await CacheService.GetOrSetAsync(testKey, () => newValue);

            // Assert.
            await newCacheStorage
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
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            CacheService.Remove(testKey);

            // Assert.
            newCacheStorage.Received().Remove(Arg.Is(testKey));
        }

        [Fact]
        public async void RemoveAsyncAlwaysCallCacheStorageRemoveAsyncMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            await CacheService.RemoveAsync(testKey);

            // Assert.
            await newCacheStorage.Received().RemoveAsync(Arg.Is(testKey));
        }

        [Fact]
        public void SetAlwaysCallCacheStorageSetMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();
            var testOptions = new CacheItemOptions();

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            CacheService.Set(testKey, testValue, testOptions);

            // Assert.
            newCacheStorage.Received().Set(Arg.Is(testKey), Arg.Is(testValue), Arg.Is(testOptions));
        }

        [Fact]
        public void SetWithoutCachingOptionStoresObjectWithDefaultRelativeExpiration()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            CacheService.Set(testKey, testValue);

            // Assert.
            newCacheStorage
                .Received()
                .Set(
                    Arg.Is(testKey),
                    Arg.Is(testValue),
                    Arg.Is<CacheItemOptions>(testOptions => testOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }

        [Fact]
        public async void SetAsyncAlwaysCallCacheStorageSetAsyncMethod()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();
            var testOptions = new CacheItemOptions();

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            await CacheService.SetAsync(testKey, testValue, testOptions);

            // Assert.
            await newCacheStorage.Received().SetAsync(Arg.Is(testKey), Arg.Is(testValue), Arg.Is(testOptions));
        }

        [Fact]
        public async void SetAsyncWithoutCachingOptionStoresObjectWithDefaultRelativeExpiration()
        {
            // Arrange.
            Cache newCacheStorage = Substitute.For<Cache>();
            string testKey = "TestKey";
            var testValue = new object();

            CacheService.SwitchCacheStorage(newCacheStorage);

            // Act.
            await CacheService.SetAsync(testKey, testValue);

            // Assert.
            await newCacheStorage
                .Received()
                .SetAsync(
                    Arg.Is(testKey),
                    Arg.Is(testValue),
                    Arg.Is<CacheItemOptions>(testOptions => testOptions.RelativeExpiration == CacheItemOptions.DefaultRelativeExpiration));
        }
    }
}