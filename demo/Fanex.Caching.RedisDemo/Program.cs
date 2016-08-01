namespace Fanex.Caching.RedisDemo
{
    using System;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var customer = new Customer();

            Console.WriteLine("Using Redis cache");

            // Cache in 120 seconds, default timeout value.
            Console.WriteLine("Cache a string in 120s:");
            CacheService.Set("TestKey1", "Test key 1");
            Console.WriteLine(CacheService.Get<string>("TestKey1"));

            // Cache in 10 seconds.
            Console.WriteLine("Cache a stringwith sliding expiration in 10s:");
            CacheService.Set("TestKey2", "Test key 2", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
            Thread.Sleep(6000);
            Console.WriteLine("Get sliding expired key after 6s:");
            Console.WriteLine(CacheService.Get<string>("TestKey2"));

            // Cache an object.
            Console.WriteLine("Cache an object:");
            CacheService.Set("TestCust", customer);
            Console.WriteLine(CacheService.Get<Customer>("TestCust")?.ToString());

            Thread.Sleep(10000);
            Console.WriteLine("Get expired key which has sliding expiration set to 10s:");
            Console.WriteLine(CacheService.Get<string>("TestKey2"));

            // Cache an integer.
            Console.WriteLine("Cache a number:");
            CacheService.Set("TestInt", 123);
            Console.WriteLine(CacheService.Get<int>("TestInt"));

            // Get an invalid key.
            Console.WriteLine("Get an invalid key:");
            Console.WriteLine(CacheService.Get<int>("InvalidKey"));

            Console.ReadLine();
        }
    }
}