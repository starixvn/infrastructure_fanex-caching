namespace Fanex.Caching.Redis.Example
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

#pragma warning disable S4055 // Literals should not be passed as localized parameters

    internal static class Program
    {
        private static void Main()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath("./"))
                .AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build();
            var services = new ServiceCollection().AddRedisCache(configuration.GetSection("fanex.caching.redis"));
            var provider = services.BuildServiceProvider();
            var cacheService = provider.GetService<ICacheService>();
            var options = provider.GetService<RedisCacheOptions>();

            Console.WriteLine("                       Using Redis Cache With Following Configurations");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine($"ApplicationName: {options.ApplicationName}");
            Console.WriteLine("End points:");

            for (var counter = 0; counter < options.ConnectionOptions.EndPoints.Count; counter++)
            {
                Console.WriteLine($"#{counter + 1}: {options.ConnectionOptions.EndPoints[counter]}");
            }

            Console.WriteLine();

            var key1 = "Key 1";
            var value1 = "Text value of 'Key 1'";
            Console.WriteLine($"Cache a text using default policy, which is {CacheItemOptions.DefaultRelativeExpiration} seconds:");
            Console.WriteLine($"Key: '{key1}'");
            Console.WriteLine($"Value: '{value1}'");
            cacheService.Set(key1, value1);
            Console.WriteLine("Sleep in 3 seconds");
            Thread.Sleep(3000);
            Console.WriteLine($"Text value got from Redis associates with '{key1}': {cacheService.Get<string>(key1)}");

            var key2 = "Key 2";
            var value2 = 101090;
            var timeToLive = 30;
            Console.WriteLine($"Cache a number in {timeToLive} seconds:");
            Console.WriteLine($"Key: '{key2}'");
            Console.WriteLine($"Value: '{value2}'");
            cacheService.Set(key2, value2, new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(timeToLive)));
            Console.WriteLine("Sleep in 3 seconds");
            Thread.Sleep(3000);
            Console.WriteLine($"Numeric value got from Redis associates with '{key2}': {cacheService.Get<int>(key2)}");

            var key3 = "Key 3";
            var value3 = new Employee();
            var slidingTimeout = 5;
            Console.WriteLine($"Cache an object and reset its time-to-live if got touch in {slidingTimeout} seconds (Slinding Expiration):");
            Console.WriteLine($"Key: '{key3}'");
            Console.WriteLine($"Value: '{value3}'");
            cacheService.Set(key3, value3, new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(slidingTimeout)));
            Console.WriteLine("Sleep in 3 seconds");
            Thread.Sleep(3000);
            Console.WriteLine($"Touch '{key3}' and get its value as: {cacheService.Get<Employee>(key3)}");
            Console.WriteLine("Sleep in another 4 seconds");
            Thread.Sleep(4000);
            Console.WriteLine($"'{key3}' is still alive with its value as: {cacheService.Get<Employee>(key3)}");
            Console.WriteLine("Sleep in another 6 seconds");
            Thread.Sleep(6000);
            Console.WriteLine($"'{key3}' is expired and value we get from cache is: {cacheService.Get<Employee>(key3)}");

            Console.ReadLine();
        }
    }
}