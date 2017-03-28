namespace Fanex.Caching.Demo
{
    using System;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var customer = new Customer();

            Console.WriteLine("Using Memory cache");

            // Cache in 120 seconds, default timeout value.
            ConfiguredCacheService.Set("TestKey1", "Test key 1");
            Console.WriteLine(ConfiguredCacheService.Get<string>("TestKey1"));

            // Cache in 500 seconds.
            ConfiguredCacheService.Set("TestKey2", "Test key 2", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
            Thread.Sleep(6000);
            Console.WriteLine(ConfiguredCacheService.Get<string>("TestKey2"));

            // Cache an object.
            ConfiguredCacheService.Set("TestCust", customer);
            Console.WriteLine(ConfiguredCacheService.Get<Customer>("TestCust")?.ToString());

            Thread.Sleep(3000);
            Console.WriteLine(ConfiguredCacheService.Get<Customer>("TestCust"));
            Thread.Sleep(5000);
            Console.WriteLine(ConfiguredCacheService.Get<Customer>("TestKey2"));

            Console.ReadLine();
        }
    }
}