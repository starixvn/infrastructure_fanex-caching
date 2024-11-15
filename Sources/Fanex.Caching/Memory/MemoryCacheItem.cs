namespace Fanex.Caching.Memory;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class MemoryCacheItem
{
    private readonly DateTimeOffset? _absoluteExpiration;

    private IList<PostEvictionCallbackRegistration> _postEvictionCallbacks;
    private EvictionReason _evictionReason;
    private bool _isExpired;

    public MemoryCacheItem(
        string key,
        object value,
        MemoryCacheItemOptions options,
        DateTimeOffset createdTime)
    {
        Key = key;
        Value = value;
        Options = options;
        LastAccess = createdTime;

        _absoluteExpiration = options.CalculateAbsoluteExpiration(createdTime);
        _postEvictionCallbacks = options.PostEvictionCallbacks;
    }

    public string Key { get; }

    public object Value { get; }

    public MemoryCacheItemOptions Options { get; }

    public DateTimeOffset LastAccess { get; private set; }

    public bool IsReplaced => _evictionReason == EvictionReason.Replaced;

    public void Touch(DateTimeOffset now)
        => LastAccess = now;

    public void SetExpired(EvictionReason reason)
    {
        _isExpired = true;

        if (_evictionReason == EvictionReason.None)
        {
            _evictionReason = reason;
        }
    }

    public void InvokeEvictionCallbacks()
    {
        if (_postEvictionCallbacks != null)
        {
            Task.Run(() => InvokeCallbacks(this));
        }
    }

    private static void InvokeCallbacks(MemoryCacheItem item)
    {
        var callbackRegistrations = Interlocked.Exchange(ref item._postEvictionCallbacks, null);

        if (callbackRegistrations != null)
        {
            foreach (var registration in callbackRegistrations)
            {
                try
                {
                    registration.EvictionCallback?.Invoke(item.Key, item.Value, item._evictionReason, registration.State);
                }
                catch
                {
                    // Ignore all exceptions throw during executing callbacks.
                }
            }
        }
    }

    public bool CheckExpired(DateTimeOffset now)
        => _isExpired || CheckForExpiredTime(now);

    private bool CheckForExpiredTime(DateTimeOffset now)
    {
        if (IsAbsolutelyExpired(now) || IsSlidinglyExpired(now))
        {
            SetExpired(EvictionReason.Expired);

            return true;
        }

        return false;
    }

    private bool IsAbsolutelyExpired(DateTimeOffset now)
        => _absoluteExpiration.HasValue && _absoluteExpiration.Value <= now;

    private bool IsSlidinglyExpired(DateTimeOffset now)
        => Options.SlidingExpiration.HasValue && now - LastAccess >= Options.SlidingExpiration.Value;
}