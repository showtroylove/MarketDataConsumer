using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TroyStevens.Market.Data;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;

namespace MarketDataProvidersLib.UnitTest.Providers
{
    [TestFixture]
    internal class MarketDataProviderTest : MarketDataProvider
    {
        MockOperationContext Context = new MockOperationContext();

        public MarketDataProviderTest() : base("MarketDataProviderTestProvider")
        {            
        }
        
        [TestCase(Result=true)]
        public bool PublishTest()
        {
            MarketDataProviderTest test = new MarketDataProviderTest();
            test = this;
            // Intercept the OnPublish so we receive the Security we just sent as a client would
            Context.OnPublishMaketData = PublishTestPublishMarketDataMock;

            // We call this method to establish a session as a client would which in
            // turns causes the Publish method to be called from the base class.
            // Calling Publish() directly here is redundant, but still works.
            GetAllProviderEvents();
            //Publish();

            Assert.IsTrue(dataItems.Count == 1);
            var data = dataItems.First();
            // We expect to receive the End of feed back from a sucessful run.
            return (data.ProviderId == ProviderId && data.Name == Miscellaneous.END_OF_FEED);
        }


        [TestCase(Result = true)]
        public bool GetAllProviderEventsReentryTest()
        {
            // Intercept the OnPublish so we receive the Security we just sent as a client would
            Context.OnPublishMaketData = PublishTestPublishMarketDataMock;

            // We call this method to establish a session as a client would which in
            // turns causes the Publish method to be called from the base class.
            // Calling Publish() directly here is redundant, but still works.
            // For this test we want this redundency to test reentry of the Publish method.
            // We don't want the data gen providers to kick off several sets of numbers.
            GetAllProviderEvents();
            GetAllProviderEvents();

            // Despite being called twice we should still see only one EOF data item
            Assert.IsTrue(dataItems.Count == 1);

            var data = dataItems.First();
            // We expect to receive the End of feed back from a sucessful run.
            return (data.ProviderId == ProviderId && data.Name == Miscellaneous.END_OF_FEED);
        }                

        protected override void SymbolReaderTaskWork()
        {            
        }

        List<Security> dataItems = new List<Security>();
        private void PublishTestPublishMarketDataMock(Security data)
        {
            dataItems.Add(data);            
        }

        ConcurrentBag<Security> bag = new ConcurrentBag<Security>();
        private void SymbolProcessorTaskWorkPublishResultsTestHandler(Security data)
        {
            bag.Add(data);
        }
            
        protected override IMarketDataProviderCallback ContextCallbackChannel
        {
            get
            {
                return Context.GetCallbackChannel();
            }
        }

        protected override string ContextSessionId
        {
            get
            {
                return Context.SessionId;
            }
        }
    }

    [TestFixture]
    internal class MarketDataProviderTest2 : MarketDataProvider
    {
        MockOperationContext Context = new MockOperationContext();

        public MarketDataProviderTest2()
            : base("MarketDataProviderTestProvider")
        {
        }

        [TestCase("TROY", Result = true)]
        public bool PublishResultsSymbolProcessorTaskWorkTest(string name)
        {
            // Intercept the OnPublish so we receive the Security we just sent as a client would
            Context.OnPublishMaketData = SymbolProcessorTaskWorkPublishResultsTestHandler;
            Assert.IsTrue(dataQueue.Count == 0);

            AddClient();
            dataQueue.Add(new Security("MarketDataProviderTestProvider", name, 5.6D));
            dataQueue.Add(new Security("MarketDataProviderTestProvider", name, 8.1D));
            dataQueue.Add(new Security("MarketDataProviderTestProvider", name, 10.5D));
            dataQueue.CompleteAdding();
            SymbolProcessorTaskWork();

            return (bag.Where(x => x.Name == name).Count() == 3);
        }

        protected override void SymbolReaderTaskWork()
        {
            // Already covered unit testing. See Compubox test.
        }
             
        ConcurrentBag<Security> bag = new ConcurrentBag<Security>();
        private void SymbolProcessorTaskWorkPublishResultsTestHandler(Security data)
        {
            bag.Add(data);
        }

        protected override IMarketDataProviderCallback ContextCallbackChannel
        {
            get
            {
                return Context.GetCallbackChannel();
            }
        }

        protected override string ContextSessionId
        {
            get
            {
                return Context.SessionId;
            }
        }
    }
}
