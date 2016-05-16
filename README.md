# Some first words and a historical story
Recall the day of the Asgard project, we were in the need of an _**independent**_ data storage, where the session and _caching data_ would be shared among sites, with the best performance as it could be. Couchbase's memcache and Redis were 2 candidates of our election. After discussions, Redis was the winner.

To speed up the development phase, we chose to extend the Fanex.Caching (version 1.x) to make it satisfy the new requirement. But many issues were showing themselves during the review. Those issues have still remained in the latest stable version of Fanex.caching-1.0.404.97, which you may use in your projects :yum:

Here, we can count some of them:
+ The EnterpriseLibrary.Caching (one of 3 supported caching types) has not been used in any project. It means **redundant code.**
+ Sliding expiration does not work correctly when the caching type is Memcache (Couchbase). The key will be expired no matter if it is touched in the period of sliding timeout or not.
+ All supported caching types were bundled into **one** Nuget package, so whether we want to use Memcache or EnterpriseLibrary.Caching, we must have some unnecessary dependencies in our project's referenced DLLs, such as: `Microsoft.Practices.EnterpriseLibrary.Caching.dll` and `Couchbase.dll`.
+ No way to initialize the `CacheService` by configuration.
+ Lack of asynchronous methods.
+ Insecure method/properties are exposed, for example: `Count`, `Keys`or `Clear`. They hurt the performance once the storage is a remote one like Couchbase's memcache or Redis.

We did not have time to do a renovation, so we accepted to live with them by leaving the Fanex.Caching as itself, cloning it with the new name: ONEbooks.Common.Caching, making ONEbooks.Common.Caching.Redis as an extension of ONEbooks.Common.Caching and hiding the insecure methods.

**But now, it is the time to make our moves.**
# Introduction
Fanex.Caching 2.x comes to life with the expectation to be fixing known issues of the 1.x one, as well as to be an easy-to-add-new-storage structure. The accomplishments with this new version are: providing new APIs, another way to instantiate a cache storage via configuration, and bringing the EnterpriseLibrary.Caching and Couchbase's memcache out of its underlayer or supported storages. **The way you are using Fanex.Caching version 1.x could be fragile with the changes, so be careful when applying it into your projects or you will put yourself in hot water.**

The version of 2.0 gives you 2 options of cache storage: local memory and distributed data storage with Redis. Two are bundled into separate Nuget packages: Fanex.Caching and Fanex.Caching.Redis. The selection is on your hands. 

Let's get started by  these 3 following steps:
1. Install the Nuget packages
    - Install Fanex.Caching 2.0.0-*. 
        ```sh
        PM> Install-Package Fanex.Caching -Pre
        ```
    - Install Fanex.Caching.Redis 0.0.1-* . This package will install Fanex.Caching 2.0.0-* automatically as its dependency. If you choose to **not** install this package, you only have local memory as your cache storage.
        ```sh
        PM> Install-Package Fanex.Caching.Redis -Pre
        ```
2. Configure your Fanex.Caching section in the configuration file
    - If you want to use MemoryCache, add these to your app.config or web.config. Refer the [Memory as Cache Storage]( Read the [`CacheItemOptions`](https://gitlab.nexcel.vn/infrastructure/fanex-caching#cacheitemoptions) for more information about its properties and methods.) for more details:
        ```xml
        <configSections>
            <section name="Fanex.Caching" type="Fanex.Caching.Configuration.CacheSettings, Fanex.Caching" />
        </configSections>
        <Fanex.Caching cacheType="Fanex.Caching.Memory.MemoryCache, Fanex.Caching">
            <connectionSettings>
                <add name="CompactOnMemoryPressure" value="true" />
                <add name="ExpirationScanFrequency" value="1" />
            </connectionSettings>
        </Fanex.Caching>
        ```
    - If you want Redis as your cache storage, add these lines to you app.config or web.config. Refer the [Redis as Cache Storage](https://gitlab.nexcel.vn/infrastructure/fanex-caching#redis-as-cache-storage) for more details:
        ```xml
        <configSections>
            <section name="Fanex.Caching" type="Fanex.Caching.Configuration.CacheSettings, Fanex.Caching" />
        </configSections>
        <Fanex.Caching cacheType="Fanex.Caching.Redis.RedisCache, Fanex.Caching.Redis">
            <connectionSettings>
                <add name="Host" value="10.70.70.249" />
                <add name="Port" value="8000" />
                <add name="Serializer" value="Fanex.Caching.Redis.MessagePackSerializer, Fanex.Caching.Redis" />
            </connectionSettings>
        </Fanex.Caching>
        ```
3. Now, you can use methods of the static class named `CacheService` to access the Cache Storage instance you have created with the configuration
    ```csharp
    CacheService.Set("TestKey1", "Test key 1", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
    Console.WriteLine(CacheService.Get<string>("TestKey1"));
    ```
Please head to the section named ['CacheService's Methods'](https://gitlab.nexcel.vn/infrastructure/fanex-caching#cacheservices-methods) for more information about methods offered by CacheService class

# `CacheService`'s methods
The `CacheService` class reads your configuration, news CacheStorage based on the configured properties, and exposes methods so that you can access to the cache in your code. The following is the lists of its properties and methods

## Property list
Name| Type| Decription
---|---|---
CacheStorage|Fanex.Caching.Cache|Readonly property provides access to the `Cache` object created based on the information configured in the Fanex.Caching section of app.config or web.config

## Method list
Name|Returned Type| Description
---|---|---
SwitchCacheStorage(Cache newCacheStorage)|Fanex.Caching.Cache|Replaces the currently using cache storage with the new one. It returns the old cache storage if the replacement is successful.
Contain(string key)|bool|Checks if there is any object associated with  `key` in the cache storage or not.
ContainAsync(string key)|Task<bool>|Checks if there is any object associated with  `key` in the cache storage or not.
Get<T>(string key)|T|Gets a specific cache entry from the storage. The `default(T)` will be returned if the `key` does not exist or it cannot be casted to the type of `T`.
GetAsync<T>(string key)|Task<T>|Gets a specific cache entry from the storage. The `default(T)` will be returned if the `key` does not exist or it cannot be casted to the type of `T`.
TryGet<T>(string key, out T value)|bool|If there is any value stored in the cache with the passed `key` and the value is castable to the type of `T`, returns `true` and the output parameter is the value. Otherwise, returns `false` and the output parameter is `default(T)`. 
TryGetAsync<T>(string key)|Task<Tuple<bool, T>>|If the `key` has already been inserted into the cache and the value is castable to the type of `T`, returns a `Tuple` which its `Item1` is set to `true` and its `Item2` is the needed value. Otherwise, `false` and `default(T)` are set as `Item1` and `Item2` of the returned `Tuple`. 
GetOrSet<T>(string key, Func<T> factory)|T|If the `key` has not been stored or cannot be casted to the type of `T`, call the `factory` to create a new value, insert it into the cache associates with the `key`, then return it. Otherwise, return inserted entry.
GetOrSetAsync<T>(string key, Func<T> factory)|Task<T>|Gets value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, insert it into the cache, then return it.
GetOrSet<T>(string key, Func<T> factory, CacheItemOptions options)|T|Gets value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, cache it under the `key` with rules passing by the `CacheItemOptions`, then return it. Read the [`CacheItemOptions`](https://gitlab.nexcel.vn/infrastructure/fanex-caching#cacheitemoptions) for more information about its properties and methods.
GetOrSetAsync<T>(string key, Func<T> factory, CacheItemOptions options)|Task<T>|Gets value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, cache it under the `key` with rules passing by the `CacheItemOptions`, then return it. Read the [`CacheItemOptions`](https://gitlab.nexcel.vn/infrastructure/fanex-caching#cacheitemoptions) for more information about its properties and methods.
Remove(string key)||Removes a key from the cache storage.
RemoveAsync(string key)|Task|Removes a key from the cache storage.
Set<T>(string key, T value)||Inserts a cache entry with the specific key and value of type `T` into the cache storage.
SetAsync<T>(string key, T value)|Task|Inserts a cache entry with the specific key and value of type `T` into the cache storage.
Set<T>(string key, T value, CacheItemOptions options)||Inserts a cache entry into the cache storage, specifying information about when the entry will be evicted. Read the [`CacheItemOptions`](https://gitlab.nexcel.vn/infrastructure/fanex-caching#cacheitemoptions) for more information about its properties and methods.
SetAsync<T>(string key, T value, CacheItemOptions options)|Task|Inserts a cache entry into the cache storage, specifying information about when the entry will be evicted. Read the [`CacheItemOptions`](https://gitlab.nexcel.vn/infrastructure/fanex-caching#cacheitemoptions) for more information about its properties and methods.
# `CacheItemOptions`
## Property list
Name| Type| Decription
---|---|---
AbsoluteExpiration|DateTimeOffset?|Date time when the key will be expired
RelativeExpiration|TimeSpan?|A TimeSpan value specify the number of seconds, minutes, or hours, ect. the key will be expired from the date and time the cache entry inserted into the cache.
SlidingExpiration|TimeSpan?|A TimeSpan value specify a period of time in which if the key is touched, its expiry will be reset
## Method list
Name|Returned Type| Description
---|---|---
SetAbsoluteExpiration(DateTimeOffset absolute)|CacheItemOptions|Sets value to `AbsoluteExpiration` property. Returns the `CacheItemOptions` object for fluent configuration.
SetAbsoluteExpiration(TimeSpan relative)|CacheItemOptions|Sets value to `RelativeExpiration` property. Returns the `CacheItemOptions` object for fluent configuration.
SetSlidingExpiration(TimeSpan offset)|CacheItemOptions|Sets value to `SlidingExpiration` property. Returns the `CacheItemOptions` object for fluent configuration.
# Memory as cache storage
The following is the list of properties you can use to configure the local memory as cache storage in your app.config or web.config.

Name|Type|Description
---|---|---
CompactOnMemoryPressure|bool|If it set to `true`, the library will try to cleanup your memory in case the memory is in a pressure. Otherwise, it will do nothing. Default value is set to `true`.
ExpirationScanFrequency|TimeSpan|Specify the time the cleanup process will run to free your memory. Its default value is 1 minute.
# Redis as cache storage
The following is the list of properties you can use to configure in your app.config or web.config once the Redis is set as your cache storage.

Name|Type|Description
---|---|---
Serializer|string|The assembly-qualified name of the class which will take the role of an object serializer. It will serialze and deserialize objects before they are sent to or afted they are fetched from Redis servers.<br> Fanex.Caching.Redis uses [MsgPack-Cli](https://github.com/msgpack/msgpack-cli) as the default serializer.<br> You can implement your own serializer with your favorite 3rd library by creating a class that implements  `Fanex.Caching.Redis.Serializer` interface
ApplicationName|string|Name of the site or application which is using the library. This is used to seperate the keys in Redis databases into namespaces.
ConnectionString|string|A well full configured connection string. **If this property is set, its value will be used to connect to Redis servers and the properties listed after this property in table will be ignored.** Its format follows `StackExchange.Redis.ConfigurationOptions`.
Host|string|The IP of server which run the Redis server.
Port|number|Port which Redis server listens to requests.
UseSsl|bool|Indicates whether the connection should be encrypted.
OperationTimeout|number|Specifies the time in milliseconds that the system should allow for synchronousoperations (defaults to 5 second).
ConnectionTimeout|number|Specifies the time in milliseconds that should be allowed for connection.(defaults to 5 second).
Password|string| The password to use to authenticate with the server.