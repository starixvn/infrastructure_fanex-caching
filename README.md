# Some first words and a historical story
Recall the day of the Asgard project, we were in the need of an _**independent**_ data storage, where the session and caching data would be shared among sites, with the best performance as it could be. Couchbase's memcache and Redis were 2 candidates of our election. After discussions, Redis was the winner.

To speed up the development phase, we chose to extend the Fanex.Caching (version 1.x) to make it satisfy the new requirement. But many issues were showing themselves during the review. Those issues have still remained in the latest stable version of Fanex.caching-1.0.404.97, which you may use in your projects :yum:

Here, we can count some of them:
+ The EnterpriseLibrary.Caching (one of 3 supported caching types) has not been used in any project. **It means redundant code.**
+ Sliding expiration does not work correctly when the caching type is Memcache (Couchbase). The key will be expired no matter if it is touched in the period of sliding timeout or not.
+ All supported caching types were bundled into **one** Nuget package, so whether we want to use Memcache or EnterpriseLibrary.Caching, we must have some unnecessary dependencies in our project's referenced DLLs, such as: `Microsoft.Practices.EnterpriseLibrary.Caching.dll` and `Couchbase.dll`.
+ No way to initialize the `CacheService` by configuration.
+ Lack of asynchronous methods.
+ Insecure method/properties are exposed, for example: `Count`, `Keys`or `Clear`. They hurt the performance once the storage is a remote one, for instance: Couchbase's memcache or Redis.
+ Ambiguous APIs' parameters:
    ```csharp
    Set<T>(string key, T value, int timeOut = 120)
    Set<T>(string key, T value, DateTime absoluteExpiration)
    Set<T>(string key, T value, TimeSpan slidingExpiration)
    ```
    What if we want set the relative timeout by a `TimeSpan` value? If we used it by accident, the key could never be expired since the sliding expiration `Set` would be called.

We did not have time to do a renovation, so we accepted to live with them by making Fanex.Caching.Redis as an extension of Fanex.Caching and hiding the insecure ones.

**But now, it is the time.**
# Introduction
Fanex.Caching 2.x comes to life with the expectation to be fixing known issues of the 1.x one, as well as an easy-to-add-new-storage structure. Accomplishments with this new version are: providing new APIs, another way to instantiate a cache storage via configuration, and bringing the EnterpriseLibrary.Caching and Couchbase's memcache out of its underlayer or supported storages. **The way you are using Fanex.Caching version 1.x could be fragile with the changes, so be careful when applying it into your projects or you will put yourself in the hot water.**

The version of 2.0 gives you 2 options of cache storage: local memory and distributed data storage with Redis. Two are bundled into separate Nuget packages: Fanex.Caching and Fanex.Caching.Redis. The selection is on your hands by following these 2 simple steps:
1. Install the Nuget packages
    - Install Fanex.Caching 2.0.0-*. 
        ```sh
        PM> Install-Package Fanex.Caching -Pre
        ```
    - Install Fanex.Caching.Redis 0.0.1-*. This package will install Fanex.Caching 2.0.0-* automatically as its dependency. If you choose to **not** install this package, you only have local memory as your cache storage.
        ```sh
        PM> Install-Package Fanex.Caching.Redis -Pre
        ```
2. Configure your Fanex.Caching
    - If you want to use MemoryCache, add these to your app.config or web.config. Refer the Memory as Cache Storage for more details:
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
    - If you want Redis as your cache storage, add these lines to you app.config or web.config. Refer the Redis as Cache Storage for more details:
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
3. Now, you can use methods of the static class named CacheService to access the Cache Storage instance you have created with the configuration
    ```csharp
    CacheService.Set("TestKey1", "Test key 1");
    Console.WriteLine(CacheService.Get<string>("TestKey1"));
    ```
Please head to the next section named 'CacheService's Methods' for more information about methods offered by CacheService class
# `CacheService`'s methods

# Memory as cache storage

# Redis as cache storage