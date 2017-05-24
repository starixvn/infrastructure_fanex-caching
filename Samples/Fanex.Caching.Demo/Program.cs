namespace Fanex.Caching.Demo
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Path.GetFullPath("./")).AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build();
            var services = new ServiceCollection().AddMemoryCache(configuration.GetSection("fanex.caching"));
            var provider = services.BuildServiceProvider();
            var cacheService = provider.GetService<CacheService>();
            var customer = new Customer();

            Console.WriteLine("Using Memory cache");

            // Cache in 120 seconds, default timeout value.
            cacheService.Set("TestKey1", "Test key 1");
            Console.WriteLine(cacheService.Get<string>("TestKey1"));

            // Cache in 500 seconds.
            cacheService.Set("TestKey2", "Test key 2", new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromSeconds(10)));
            Thread.Sleep(6000);
            Console.WriteLine(cacheService.Get<string>("TestKey2"));

            // Cache an object.
            cacheService.Set("TestCust", customer);
            Console.WriteLine(cacheService.Get<Customer>("TestCust")?.ToString());

            Thread.Sleep(3000);
            Console.WriteLine(cacheService.Get<Customer>("TestCust"));
            Thread.Sleep(5000);
            Console.WriteLine(cacheService.Get<Customer>("TestKey2"));

            Console.ReadLine();
        }
    }
}