using System;
using System.Threading;
using NUnit.Framework;

namespace TroyStevens.Market.Utils.Tests
{
    [TestFixture()]
    public class InActivityPollerTests
    {
        [TestCase(null, ExpectedException=typeof(ArgumentNullException))]
        public void InActivityPollerCTORNullWaitObjectTest(EventWaitHandle hnd)
        {
            var poll = new InActivityPoller<EventWaitHandle>(hnd, PollingFunc);
        }

        [TestCase(null, ExpectedException = typeof(ArgumentNullException))]
        public void InActivityPollerCTORNullPollerCallbackTest(KeepPolling hnd)
        {
            var wait = new ManualResetEvent(false);
            var poll = new InActivityPoller<EventWaitHandle>(wait, hnd);
        }

        [Test]
        public void PollReturnsWhenSignaledTest()
        {
            var wait = new ManualResetEvent(true);
            var poller = new InActivityPoller<EventWaitHandle>(wait, PollingFunc);
            poller.Poll();
            Assert.Pass("Event signaled releaseing thread.");
        }

        KeepPolling PollingFunc { get; set; }
        InActivityPoller<EventWaitHandle> _poller;
        InActivityPoller<EventWaitHandle> Poller
        {
            get
            {
                ManualResetEvent wait = null;
                if (null == _poller)
                {
                    wait = new ManualResetEvent(false);
                    _poller = new InActivityPoller<EventWaitHandle>(wait, PollingFunc, 0);
                }
                
                return _poller;
            }
        }

        [Test]
        public void PollReturnsWhenExplicitlyStopPollingTest()
        {
            PollingFunc = ExplicitlyStopPollingTrueKeepPolling;
            Poller.Poll();
            Assert.Pass("Event signaled releasing thread.");
        }

        [Test]
        public void PollReturnsWhenExplicitlySignaledStopPollingTest()
        {
            PollingFunc = ExplicitlySignaledStopPollingTrueKeepPolling;
            Poller.Poll();
            Assert.Pass("Event signaled releasing thread.");
        }

        private bool ExplicitlyStopPollingTrueKeepPolling()
        {
            Poller.StopPolling();
            return true;
        }

        private bool ExplicitlySignaledStopPollingTrueKeepPolling()
        {
            Poller.SignaledStopPolling();
            return true;
        }  
    }
}
