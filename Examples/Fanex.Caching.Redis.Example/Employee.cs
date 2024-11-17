#pragma warning disable S109 // Magic numbers should not be used


namespace Fanex.Caching.Redis.Example;

public class Employee
{
    public int Id { get; set; } = 2346;

    public string Nickname { get; set; } = "Jason";

    public override string ToString()
        => $"ID: {Id} - Nickname: {Nickname}";
}