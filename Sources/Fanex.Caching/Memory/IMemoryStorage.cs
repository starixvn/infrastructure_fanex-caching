namespace Fanex.Caching.Memory;

using System;

internal interface IMemoryStorage : IDisposable
{
    int Count { get; }

    bool Contain(string key);

    bool TryGet(string key, out object value);

    void Set(string key, object value, MemoryCacheItemOptions options);

    void Remove(string key);
}