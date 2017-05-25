namespace Fanex.Caching.RedisDemo
{
    using System.Globalization;

    public class Customer
    {
        public Customer()
        {
            FirstName = "Test First Name";
            LastName = "Test Last Name";
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "{0} {1}", this.FirstName, this.LastName);
    }
}