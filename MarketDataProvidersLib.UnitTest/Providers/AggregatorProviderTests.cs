using System.Collections.Concurrent;
using System.Linq;
using MarketDataProvidersLib.UnitTest.Providers;
using NUnit.Framework;
using TroyStevens.Market.Data;

namespace TroyStevens.Market.Providers.Tests
{
    [TestFixture()]
    internal class AggregatorProviderTests : AggregatorProvider 
    {
        MockOperationContext Context = new MockOperationContext();

        public AggregatorProviderTests()
        {
            ProviderId = "AggregatorProviderTestProvider";
        }

        [TestCase("TROY", Result = true)]
        public bool AggregatorSymbolProcessorTaskWorkPublishResultsTest(string name)
        {
            // Intercept the OnPublish so we receive the Security we just sent as a client would
            Context.OnPublishMaketData = SymbolProcessorTaskWorkPublishResultsTestHandler;
            Assert.IsTrue(dataQueueAggregator.Count == 0);

            AddClient();

            SecurityTicks[] ticks = { 
                                        new SecurityTicks("AAA"),
                                        new SecurityTicks("BBB"),
                                        new SecurityTicks("CCC")
                                    };

            AddingComplete onCompleteAdding = null;
            DataReceivedHandler onNewDataReceived = null;
            foreach (var tick in ticks)
            {
                onCompleteAdding += tick.CompleteAdding;
                onNewDataReceived += tick.OnDataReceived;
                var sec = new Security("Orion", tick.Symbol, 5.6D);
                onNewDataReceived(sec);
                dataQueueAggregator.Add(tick);
            }
            
            dataQueueAggregator.CompleteAdding();
            onCompleteAdding();
            
            SymbolProcessorTaskWork();

            foreach (var tick in ticks)
            {
                onCompleteAdding -= tick.CompleteAdding;
                onNewDataReceived -= tick.OnDataReceived;
            }
            var results = bag.Count();
            return (results == 3);
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
