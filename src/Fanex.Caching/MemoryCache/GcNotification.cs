namespace Fanex.Caching.Memory
{
    using System;

    internal class GcNotification
    {
        private readonly Func<object, bool> _callback;
        private readonly int _initialCollectionCount;
        private readonly object _state;

        private GcNotification(Func<object, bool> callback, object state)
        {
            _callback = callback;
            _initialCollectionCount = GC.CollectionCount(2);
            _state = state;
        }

        ~GcNotification()
        {
            bool reRegister = true;

            try
            {
                // Only invoke the callback after this instance has made it into gen2.
                if (_initialCollectionCount < GC.CollectionCount(2))
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

        public static void Register(Func<object, bool> callback, object state)
        {
            var notification = new GcNotification(callback, state);
        }
    }
}