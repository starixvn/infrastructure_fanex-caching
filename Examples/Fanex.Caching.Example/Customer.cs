namespace Fanex.Caching.Example;

public class Customer
{
    public string FirstName { get; set; } = "First Name";

    public string LastName { get; set; } = "Last Name";

    public override string ToString()
        => $"{FirstName} {LastName}";
}