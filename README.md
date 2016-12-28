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

The version of 2.0 gives you 2 options of cache storage: local memory and distributed data storage with Redis. Two are bundled into separate Nuget packages: Fanex.Caching and Fanex.Caching.Redis. Then the selection is on your hands.

Let's get started by  these 3 following steps:
1. Install the Nuget packages
    - Install Fanex.Caching 2.0.0-*.

        ```sh
        PM> Install-Package Fanex.Caching -Pre
        ```
    - Install Fanex.Caching.Redis 0.0.1-* . This package will install Fanex.Caching 2.0.0-* automatically as its dependency. **If you choose to _NOT_ install this package, you only have local memory as your cache storage.**
    
        ```sh
        PM> Install-Package Fanex.Caching.Redis -Pre
        ```
2. Configure your Fanex.Caching section in the configuration file
    - If you want to use MemoryCache, add these to your `app.config` or `web.config`. Refer the [Memory as Cache Storage](#memory-as-cache-storage) for more information.

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
    - If you want Redis as your cache storage, add these lines to you `app.config` or `web.config`. Refer the [Redis as Cache Storage](#redis-as-cache-storage) for more details:
        
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
3. Now, you can simply use the built-in static service named `ConfiguredCacheService` to access cache storage defined in the configuration file.
    ```csharp
    ConfiguredCacheService.Set("TestKey1", "Test key 1", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
    Console.WriteLine(ConfiguredCacheService.Get<string>("TestKey1"));
    ```
    If you want to make the IoC framework be happy or want to create the cache storage by your own, you can create an instance of `CacheService` and use it as the access point.
    ```csharp
    CacheService cacheService = new CacheServiceImpl(); // the storage is created based on the configuration.
    cacheService.Set("TestKey1", "Test key 1", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
    Console.WriteLine(cacheService.Get<string>("TestKey1"));
    ```
    The choice between creating instance of `CacheService` or using `ConfiguredCacheService` is to depend on your projects' caching strategy, the way you write unit tests, etc.

    For more information, please head to sections named [CacheServiceImpl](#cacheserviceimpl) and [ConfiguredCacheService](#configuredcacheservice).

# `CacheServiceImpl`
The `CacheServiceImpl` wraps and extends the functionalities provided the the Cache Storage (Memory or Redis).
It lets your site cache data without any knowledge of which under Cache Storage is being used.
Switching between storages or concurrently using multiple storages is easier than ever.

Implemented interface: `CacheService`

## Constructor list
Name| Decription
---|---
CacheServiceImpl()|New instance's Cache Storage is created based on information defined in `Fanex.Caching` section in the configuration file
CacheServiceImpl(Cache cacheStorage)|You create the Cache Storage by your own and pass it to the `CacheService` instance. It is useful in case that your site uses multiple Cache Storages at the same time. Each is for a specific purpose
## Property list
Name| Type| Decription
---|---|---
CacheStorage|Fanex.Caching.Cache|[Readonly] Provide access to the `Cache` object which the current service instance is basing on
## Method list
Name|Returned Type| Description
---|---|---
Contain(string key)|bool|Check if there is any object associated with  `key` in the cache storage or not.
ContainAsync(string key)|Task&lt;bool&gt;|Check if there is any object associated with  `key` in the cache storage or not.
Get&lt;T&gt;(string key)|T|Get a specific cache entry from the storage. The `default(T)` will be returned if the `key` does not exist or the value cannot be casted to the type of `T`.
GetAsync&lt;T&gt;(string key)|Task&lt;T&gt;|Get a specific cache entry from the storage. The `default(T)` will be returned if the `key` does not exist or the value cannot be casted to the type of `T`.
TryGet&lt;T&gt;(string key, out T value)|bool|If there is any value stored in the cache associated with `key` and the value is castable to the type of `T`, the method returns `true` and the output parameter is the value. Otherwise, it returns `false` and the output parameter is `default(T)`. 
TryGetAsync&lt;T&gt;(string key)|Task&lt;Tuple&lt;bool, T&gt;&gt;|If the `key` has already been inserted into the cache and the value is castable to the type of `T`, returns a `Tuple` which its `Item1` is set to `true` and its `Item2` is the needed value. Otherwise, `false` and `default(T)` are set as `Item1` and `Item2` of the returned `Tuple`.
GetOrSet&lt;T&gt;(string key, Func&lt;T&gt; factory)|T|If the `key` has not been stored or cannot be casted to the type of `T`, call the `factory` to create a new value, insert it into the cache associates with the `key`, then return it. Otherwise, return inserted entry.
GetOrSetAsync&lt;T&gt;(string key, Func&lt;T&gt; factory)|Task&lt;T&gt;|Get value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, insert it into the cache, then return it.
GetOrSet&lt;T&gt;(string key, Func&lt;T&gt; factory, CacheItemOptions options)|T|Get value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, cache it under the `key` with rules passing by the `CacheItemOptions`, then return it. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
GetOrSetAsync&lt;T&gt;(string key, Func&lt;T&gt; factory, CacheItemOptions options)|Task&lt;T&gt;|Get value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, cache it under the `key` with rules passed by the `CacheItemOptions`, then return it. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
Remove(string key)||Remove a key from the cache storage.
RemoveAsync(string key)|Task|Remove a key from the cache storage.
Set&lt;T&gt;(string key, T value)||Insert a cache entry with the specific key and value of type `T` into the cache storage.
SetAsync&lt;T&gt;(string key, T value)|Task|Insert a cache entry with the specific key and value of type `T` into the cache storage.
Set&lt;T&gt;(string key, T value, CacheItemOptions options)||Insert a cache entry into the cache storage, specifying information about when the entry will be evicted. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
SetAsync&lt;T&gt;(string key, T value, CacheItemOptions options)|Task|Insert a cache entry into the cache storage, specifying information about when the entry will be evicted. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
# `ConfiguredCacheService`
The `ConfiguredCacheService` class encloses an `CacheService` instance to provide you static medthods to access the Cache Storage. It reads your `web.config` or `app.config`, then creates a Cache Storage based on the configured information.

## Property list
Name| Type| Decription
---|---|---
CacheStorage|Fanex.Caching.Cache|[Readonly] Provide access to the `Cache` object created based on the information configured in the `Fanex.Caching` section of `app.config` or `web.config`.
## Static Method list
Name|Returned Type| Description
---|---|---
SwitchCacheStorage(Cache newCacheStorage)|Fanex.Caching.Cache|**[Obsolete] - Consider using 'new CacheService(Cache cacheStorage)' instead.** Replace the currently using cache storage with the new one. It returns the old cache storage if the replacement is successful. If your site switches back and forth storages, make sure that you are pointing to the right storage before sending data to it.
Contain(string key)|bool|Expose the method of the underlay `CacheService` instance. Check if there is any object associated with  `key` in the cache storage or not.
ContainAsync(string key)|Task&lt;bool&gt;|Expose the method of the underlay `CacheService` instance. Check if there is any object associated with  `key` in the cache storage or not asynchronously.
Get&lt;T&gt;(string key)|T|Expose the method of the underlay `CacheService` instance. Get a specific cache entry from the storage. The `default(T)` will be returned if the `key` does not exist or the value cannot be casted to the type of `T`.
GetAsync&lt;T&gt;(string key)|Task&lt;T&gt;|Expose the method of the underlay `CacheService` instance. Get a specific cache entry from the storage asynchronously. The `default(T)` will be returned if the `key` does not exist or the value cannot be casted to the type of `T`.
TryGet&lt;T&gt;(string key, out T value)|bool|Expose the method of the underlay `CacheService` instance. If there is any value stored in the cache associated with `key` and the value is castable to the type of `T`, the method returns `true` and the output parameter is the caching value. Otherwise, it returns `false` and the output parameter is `default(T)`. 
TryGetAsync&lt;T&gt;(string key)|Task&lt;Tuple&lt;bool, T&gt;&gt;|Expose the method of the underlay `CacheService` instance. If the `key` has already been inserted into the cache and the value is castable to the type of `T`, returns a `Tuple` which its `Item1` is set to `true` and its `Item2` is the needed value. Otherwise, `false` and `default(T)` are set as `Item1` and `Item2` of the returned `Tuple`.
GetOrSet&lt;T&gt;(string key, Func&lt;T&gt; factory)|T|Expose the method of the underlay `CacheService` instance. If the `key` has not been stored or cannot be casted to the type of `T`, call the `factory` to create a new value, insert it into the cache associates with the `key`, then return it. Otherwise, return inserted entry.
GetOrSetAsync&lt;T&gt;(string key, Func&lt;T&gt; factory)|Task&lt;T&gt;|Expose the method of the underlay `CacheService` instance. Get value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, insert it into the cache, then return it.
GetOrSet&lt;T&gt;(string key, Func&lt;T&gt; factory, CacheItemOptions options)|T|Expose the method of the underlay `CacheService` instance. Get value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, cache it under the `key` with rules passing by the `CacheItemOptions`, then return it. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
GetOrSetAsync&lt;T&gt;(string key, Func&lt;T&gt; factory, CacheItemOptions options)|Task&lt;T&gt;|Expose the method of the underlay `CacheService` instance. Get value stored with the `key`. If the `key` has not been in the storage, call the `factory` to create a new value, cache it under the `key` with rules passed by the `CacheItemOptions`, then return it. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
Remove(string key)||Expose the method of the underlay `CacheService` instance. Remove a key from the cache storage.
RemoveAsync(string key)|Task|Expose the method of the underlay `CacheService` instance. Remove a key from the cache storage asynchronously.
Set&lt;T&gt;(string key, T value)||Expose the method of the underlay `CacheService` instance. Insert a cache entry with the specific key and value of type `T` into the cache storage.
SetAsync&lt;T&gt;(string key, T value)|Task|Expose the method of the underlay `CacheService` instance. Insert a cache entry with the specific key and value of type `T` into the cache storage.
Set&lt;T&gt;(string key, T value, CacheItemOptions options)||Expose the method of the underlay `CacheService` instance. Insert a cache entry into the cache storage, specifying information about when the entry will be evicted. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
SetAsync&lt;T&gt;(string key, T value, CacheItemOptions options)|Task|Expose the method of the underlay `CacheService` instance. Insert a cache entry into the cache storage, specifying information about when the entry will be evicted. Read the [`CacheItemOptions`](#cacheitemoptions) for more information about its properties and methods.
# `CacheItemOptions`
## Property list
Name| Type| Decription
---|---|---
AbsoluteExpiration|DateTimeOffset?|Date time when the key will be expired
RelativeExpiration|TimeSpan?|A TimeSpan value specify the number of seconds, minutes, or hours, ect. that the key will be expired from the date and time the cache entry inserted into the cache.
SlidingExpiration|TimeSpan?|A TimeSpan value specify a period of time in which if the key is touched, its expiry will be reset
## Method list
Name|Returned Type| Description
---|---|---
SetAbsoluteExpiration(DateTimeOffset absolute)|CacheItemOptions|Sets value to `AbsoluteExpiration` property. Returns the `CacheItemOptions` object for fluent configuration.
SetAbsoluteExpiration(TimeSpan relative)|CacheItemOptions|Sets value to `RelativeExpiration` property. Returns the `CacheItemOptions` object for fluent configuration.
SetSlidingExpiration(TimeSpan offset)|CacheItemOptions|Sets value to `SlidingExpiration` property. Returns the `CacheItemOptions` object for fluent configuration.
# Memory as cache storage
The following is the list of properties you can use to configure the local memory as storage in your app.config or web.config.

Name|Type|Description
---|---|---
CompactOnMemoryPressure|bool|If it is set to `true`, the library will try to cleanup your memory in case the memory is in a pressure. Otherwise, it will do nothing. Default value is set to `true`.
ExpirationScanFrequency|TimeSpan|Specify the time the cleanup process will run to free your memory. Its default value is 1 minute.
# Redis as cache storage
The following is the list of properties you can use to configure in your app.config or web.config once the Redis is set as your cache storage.

Name|Type|Description
---|---|---
Serializer|string|The assembly-qualified name of the class which will take the role of an object serializer. It will serialize and deserialize objects before they are sent to or after they are fetched from Redis servers.<br> Fanex.Caching.Redis uses [MsgPack-Cli](https://github.com/msgpack/msgpack-cli) as the default serializer.<br> You can implement your own serializer with your favorite 3rd library by creating a class that implements  `Fanex.Caching.Redis.Serializer` interface.
ApplicationName|string|Name of the site or application which is using the library. This is used to seperate the keys in Redis databases into namespaces.
ConnectionString|string|A well full configured connection string. **If this property is set, its value will be used to connect to Redis servers and the properties listed after this property in the table will be ignored.** Its format follows `StackExchange.Redis.ConfigurationOptions`.
Host|string|The IP of server which runs the Redis server.
Port|number|Port which Redis server listens to requests.
UseSsl|bool|Indicates whether the connection should be encrypted.
OperationTimeout|number|Specifies the time in milliseconds that the system should allow for synchronous operations (defaults to 5 second).
ConnectionTimeout|number|Specifies the time in milliseconds that should be allowed for connection.(defaults to 5 second).
Password|string| The password to use to authenticate with the server.