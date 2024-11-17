namespace Fanex.Caching.Memory;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class MemoryCacheItem(
    string key,
    object value,
    MemoryCacheItemOptions options,
    DateTimeOffset createdTime)
{
    private readonly DateTimeOffset? _absoluteExpiration = options.CalculateAbsoluteExpiration(createdTime);

    private IList<PostEvictionCallbackRegistration> _postEvictionCallbacks = options.PostEvictionCallbacks;
    private EvictionReason _evictionReason;
    private bool _isExpired;

    public string Key { get; } = key;

    public object Value { get; } = value;

    public MemoryCacheItemOptions Options { get; } = options;

    public DateTimeOffset LastAccess { get; private set; } = createdTime;

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

        if (callbackRegistrations == null)
        {
            return;
        }

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

    public bool CheckExpired(DateTimeOffset now)
        => _isExpired || CheckForExpiredTime(now);

    private bool CheckForExpiredTime(DateTimeOffset now)
    {
        if (!IsAbsolutelyExpired(now) && !IsSlidingExpired(now))
        {
            return false;
        }

        SetExpired(EvictionReason.Expired);

        return true;

    }

    private bool IsAbsolutelyExpired(DateTimeOffset now)
        => _absoluteExpiration.HasValue && _absoluteExpiration.Value <= now;

    private bool IsSlidingExpired(DateTimeOffset now)
        => Options.SlidingExpiration.HasValue && now - LastAccess >= Options.SlidingExpiration.Value;
}