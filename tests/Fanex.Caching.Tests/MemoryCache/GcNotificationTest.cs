namespace Fanex.Caching.Tests.MemoryCache
{
    using System;
    using System.Threading;
    using Memory;
    using Xunit;

    public class GcNotificationTest
    {
        private ManualResetEvent _callbackInvoked;

        public GcNotificationTest()
        {
            _callbackInvoked = new ManualResetEvent(false);
        }

        [Fact]
        public void CallbackRegisteredAndInvoked()
        {
            // Arrange.
            GcNotification.Register(
                state =>
                {
                    _callbackInvoked.Set();

                    return false;
                },
                null);

            // Act.
            GcCollectAndWait();

            // Assert.
            Assert.True(_callbackInvoked.WaitOne(0));
        }

        [Fact]
        public void CallbackInvokedMultipleTimes()
        {
            // Arrange.
            bool reRegisterForFinalize = true;
            GcNotification.Register(
                state =>
                {
                    _callbackInvoked.Set();

                    return reRegisterForFinalize;
                },
                null);

            // Act.
            GcCollectAndWait();

            // Assert.
            Assert.True(_callbackInvoked.WaitOne(0));

            // Act.
            _callbackInvoked.Reset();
            reRegisterForFinalize = false;
            GcCollectAndWait();

            // Assert.
            Assert.True(_callbackInvoked.WaitOne(0));

            // Act.
            _callbackInvoked.Reset();
            GcCollectAndWait();

            // Assert.
            // No callback expected the 3rd time
            Assert.False(_callbackInvoked.WaitOne(0));
        }

        private static void GcCollectAndWait()
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
        }
    }
}