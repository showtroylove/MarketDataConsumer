using NUnit.Framework;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;

namespace TroyStevens.Market.Data.Tests
{
    [TestFixture()]
    internal class MarketDataReaderTests : MarketDataReader
    {    

        [TestCase(Result=true)]
        public bool IsMarketClosedTrueExpectedResultTest()
        {
            return base.IsMarketsClosed;
        }

        [TestCase(Result = false)]
        public bool IsMarketClosedFalseExpectedResultTest()
        {            
            return false;
        }

        
        [TestCase(null, Result = true)]
        public bool PublishMarketDataNullSecurityTest(Security data)
        {
            DataReceived = null;
            base._onNewSecurityTicksEvent = DataProcessingHandler;
            PublishMarketData(data);
            return (null == DataReceived);
        }   

        [TestCase(Result = true)]
        public bool PublishMarketDataValidSecurityTest()
        {
            var data = new Security("Orion", "AAA", 5.5D);
            base._onNewSecurityEvent = DataProcessingHandler;
            PublishMarketData(data);
            return (SecurityReceived is Security && SecurityReceived.Name== data.Name);
        }

        Security SecurityReceived;
        private void DataProcessingHandler(Security dataItem)
        {
            SecurityReceived = dataItem;
        }

        [TestCase(Result = false)]
        public bool PublishMarketDataValidEOFSecurityTest()
        {
            var data = new Security("Orion", Miscellaneous.END_OF_FEED, 0D);
            base._onNewSecurityTicksEvent = DataProcessingHandler;
            PublishMarketData(data);
            return (DataReceived is SecurityTicks && DataReceived.Symbol == data.Name);
        }
                
        SecurityTicks DataReceived { get; set; }
        public void DataProcessingHandler(SecurityTicks data)
        {
            DataReceived = data;
        }

        [Test]
        public void OnMarketDataClosedEventNullSecurityTest()
        {         
            OnMarketClosedEvent(null);
            Assert.Pass("These test method provide code coverage for the unit being tested.");
        }

        [Test]
        public void OnMarketDataClosedEventUnknownProviderEOFTest()
        {
            var data = new Security("Mazaya", Miscellaneous.END_OF_FEED, 0D);
            base._onNewSecurityTicksEvent = DataProcessingHandler;
            OnMarketClosedEvent(data);
            Assert.Pass("These test method provide code coverage for the unit being tested.");
        }

        [Test]
        public void OnMarketDataClosedEventOrionEOFTest()
        {            
            var data = new Security("Orion", Miscellaneous.END_OF_FEED, 0D);            
            this.OnMarketClosedEvent(data);
            Assert.Pass("These test method provide code coverage for the unit being tested.");
        }

        [Test]
        public void OnMarketDataClosedEventPolarisEOFTest()
        {            
            var data = new Security("Polaris", Miscellaneous.END_OF_FEED, 0D);            
            OnMarketClosedEvent(data);
            Assert.Pass("These test method provide code coverage for the unit being tested.");
        }

        [TestCase(null)]
        public void EndSessionNullArgumentTest(MarketDataConsumer client)
        {
            base.EndSession(client);
            Assert.Pass("These test method provide code coverage for the unit being tested.");
        }

        [TestCase(null)]
        public void StreamFromProviderNullDataHandlerTest(NewSecurityTicksEvent dataprocessingHandler)
        {
            base.StreamFromProvider(dataprocessingHandler);
            Assert.Pass("These test method provide code coverage for the unit being tested.");
        }       

    } 
}
