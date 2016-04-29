namespace Fanex.Caching.Tests.MemoryCache
{
    using System;
    using System.Threading;
    using Memory;
    using Xunit;

    public class GcNotificationTests
    {
        private ManualResetEvent _callbackInvoked;

        public GcNotificationTests()
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
            bool registerAgainForFinalize = true;
            GcNotification.Register(
                state =>
                {
                    _callbackInvoked.Set();

                    return registerAgainForFinalize;
                },
                null);

            // Act.
            GcCollectAndWait();

            // Assert.
            Assert.True(_callbackInvoked.WaitOne(0));

            // Act.
            _callbackInvoked.Reset();
            registerAgainForFinalize = false;
            GcCollectAndWait();

            // Assert.
            Assert.True(_callbackInvoked.WaitOne(0));

            // Act.
            _callbackInvoked.Reset();
            GcCollectAndWait();

            // Assert.
            // No callback expected to invoke the 3rd time.
            Assert.False(_callbackInvoked.WaitOne(0));
        }

        private static void GcCollectAndWait()
        {
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }
    }
}