namespace Fanex.Caching.Example
{
    public class Customer
    {
        public Customer()
        {
            FirstName = "First Name";
            LastName = "Last Name";
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override string ToString()
            => $"{FirstName} {LastName}";
    }
}