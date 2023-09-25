#pragma warning disable S109 // Magic numbers should not be used


namespace Fanex.Caching.Redis.Example;

public class Employee
{
    public Employee()
    {
        Id = 2346;
        Nickname = "Jason";
    }

    public int Id { get; set; }

    public string Nickname { get; set; }

    public override string ToString()
        => $"ID: {Id} - Nickname: {Nickname}";
}