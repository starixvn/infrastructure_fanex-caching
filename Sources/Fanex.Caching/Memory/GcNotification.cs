namespace Fanex.Caching.Memory;

using System;

internal class GcNotification
{
    private const int GenerationWhenInvokingCallback = 2;

    private readonly Func<object, bool> _callback;
    private readonly int _initialCollectionCount;
    private readonly object _state;

    private GcNotification(Func<object, bool> callback, object state)
    {
        _callback = callback;
        _initialCollectionCount = GC.CollectionCount(GenerationWhenInvokingCallback);
        _state = state;
    }

    ~GcNotification()
    {
        var reRegister = true;

        try
        {
            // Only invoke the callback after this instance has made it into gen2.
            if (_initialCollectionCount < GC.CollectionCount(GenerationWhenInvokingCallback))
            {
                reRegister = _callback(_state);
            }
        }
        catch
        {
            // Never throw from the finalizer thread.
        }

        if (reRegister && !Environment.HasShutdownStarted)
        {
            GC.ReRegisterForFinalize(this);
        }
    }

    public static GcNotification Register(Func<object, bool> callback, object state) => new(callback, state);
}