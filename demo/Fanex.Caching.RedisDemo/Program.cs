namespace Fanex.Caching.RedisDemo
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
            CacheService.Set("TestKey1", "Test key 1");
            Console.WriteLine(CacheService.Get<string>("TestKey1"));

            // Cache in 500 seconds.
            CacheService.Set("TestKey2", "Test key 2", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
            Thread.Sleep(6000);
            Console.WriteLine(CacheService.Get<string>("TestKey2"));

            // Cache an object.
            CacheService.Set("TestCust", customer);
            Console.WriteLine(CacheService.Get<Customer>("TestCust")?.ToString());

            Thread.Sleep(3000);
            Console.WriteLine(CacheService.Get<Customer>("TestCust"));
            Thread.Sleep(10000);
            Console.WriteLine(CacheService.Get<string>("TestKey2"));

            Console.ReadLine();
        }
    }
}