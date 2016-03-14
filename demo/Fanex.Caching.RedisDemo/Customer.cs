namespace Fanex.Caching.RedisDemo
{
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