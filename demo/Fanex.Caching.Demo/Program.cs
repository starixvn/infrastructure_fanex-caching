namespace Fanex.Caching.Demo
{
    using System;
    using System.Threading;
    using Memory;

    public class Program
    {
        public static void Main(string[] args)
        {
            var customer = new Customer();

            Console.WriteLine("Using Memory cache");

            // Cache in 120 seconds, default timeout value.
            CacheService.Set("TestKey1", "Test key 1");
            Console.WriteLine(CacheService.Get<string>("TestKey1"));

            // Cache in 500 seconds.
            CacheService.Set("TestKey2", "Test key 2", new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(500)));
            Console.WriteLine(CacheService.Get<string>("TestKey2"));

            // Cache an object.
            CacheService.Set("TestCust", customer);
            Console.WriteLine(CacheService.Get<Customer>("TestCust")?.ToString());

            // Callback when evicted
            var options = new MemoryCacheItemOptions()
                .RegisterPostEvictionCallback(
                (key, value, reason, substate) =>
                {
                    Console.WriteLine($"'{key}: {value}' was evicted due to {reason}.");
                })
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(5));
            CacheService.Set("TestKey3", "Test key 3", options);
            Thread.Sleep(3000);
            Console.WriteLine(CacheService.Get<string>("TestKey3"));
            Thread.Sleep(6000);
            Console.WriteLine(CacheService.Get<string>("TestKey3"));

            Console.ReadLine();
        }
    }
}