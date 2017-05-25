namespace Fanex.Caching.RedisDemo
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal static class Program
    {
        private static void Main()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Path.GetFullPath("./")).AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build();
            var services = new ServiceCollection().AddRedisCache(configuration.GetSection("fanex.caching"));
            var provider = services.BuildServiceProvider();
            var cacheService = provider.GetService<CacheService>();
            var customer = new Customer();

            Console.WriteLine("Using Redis cache");

            // Cache in 120 seconds, default timeout value.
            Console.WriteLine("Cache a string in 120s:");
            cacheService.Set("TestKey1", "Test key 1");
            Console.WriteLine(cacheService.Get<string>("TestKey1"));

            // Cache in 10 seconds.
            Console.WriteLine("Cache a string with sliding expiration in 10s:");
            cacheService.Set("TestKey2", "Test key 2", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
            Thread.Sleep(6000);
            Console.WriteLine("Get sliding expired key after 6s:");
            Console.WriteLine(cacheService.Get<string>("TestKey2"));

            // Cache an object.
            Console.WriteLine("Cache an object:");
            cacheService.Set("TestCust", customer);
            Console.WriteLine(cacheService.Get<Customer>("TestCust")?.ToString());

            Thread.Sleep(10000);
            Console.WriteLine("Get expired key which has sliding expiration set to 10s:");
            Console.WriteLine(cacheService.Get<string>("TestKey2"));

            // Cache an integer.
            Console.WriteLine("Cache a number:");
            cacheService.Set("TestInt", 123);
            Console.WriteLine(cacheService.Get<int>("TestInt"));

            // Get an invalid key.
            Console.WriteLine("Get an invalid key:");
            Console.WriteLine(cacheService.Get<int>("InvalidKey"));

            Console.ReadLine();
        }
    }
}