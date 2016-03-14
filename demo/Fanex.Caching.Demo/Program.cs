namespace Fanex.Caching.Demo
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            var customer = new Customer();

            // Use the functionalities of created instances to do our jobs.
            CacheService.Set<string>("TestKey1", "Test key 1");
            CacheService.Set<string>("TestKey2", "Test key 2", new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(500)));

            System.Console.WriteLine("Using Memory cache");
            System.Console.WriteLine(CacheService.Get<string>("TestKey1"));
            System.Console.WriteLine(CacheService.Get<string>("TestKey2"));

            CacheService.Set<Customer>("TestCust", customer);
            var cachedCust = CacheService.Get<Customer>("TestCust");

            System.Console.WriteLine(cachedCust.ToString());
            System.Console.ReadLine();
        }
    }

    public class Customer
    {
        public Customer()
        {
            this.FirstName = "Test First Name";
            this.LastName = "Test Last Name";
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.FirstName, this.LastName);
        }
    }
}