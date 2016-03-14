namespace Fanex.Caching.Redis
{
    using StackExchange.Redis;

    internal class RedisCommand
    {
        public string Script { get; set; }

        public RedisKey[] Keys { get; set; }

        public RedisValue[] Values { get; set; }
    }
}