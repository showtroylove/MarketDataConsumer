using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TroyStevens.Market.Client.Data;
using NUnit.Framework;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Data;
using System.Threading;

namespace TroyStevens.Market.Client.Data.Tests
{
    [TestFixture()]
    public class MarketDataReaderTests : MarketDataReader
    {
        #region PublishMarketData Method Test

        List<Security> data = new List<Security>();
        Security _nullsec;
        Security _AAAsec;
        SecurityTicksNoSkip _ticks;
        [TestCase("Aggregator", "AAA", 9.30D, Result = true)]
        public bool PublishMarketDataNewDataEventTest(string providerid, string name, double value)
        {
            OnDataReceived = PublishMarketDataNewDataEventTestHandler;
            _onNewSecurityTicksEvent = OnNewSecurityTicksEvent;
            var sec = new Security(providerid, name, value);
            PublishMarketData(sec);

            // Excepted result is this item will be published to 
            // listeners / clients.
            return (null != _ticks && _ticks.SymbolTicks.First() == _AAAsec);
        }

        private void OnNewSecurityTicksEvent(SecurityTicksNoSkip data)
        {
            _ticks = data;
        }

        private void PublishMarketDataNewDataEventTestHandler(Security dataItem)
        {
            _AAAsec = dataItem;
        }

        [TestCase(null, Result = true)]
        public bool PublishMarketDataNullDataTest(Security sec)
        {
            OnDataReceived = PublishMarketDataNullDataTestDataHandler;
            PublishMarketData(sec);

            // Excepted result is that NO data will be published to 
            // listeners / clients.
            return (null == _nullsec);
        }

        [TestCase("Aggregator", Miscellaneous.END_OF_FEED, 0D, Result = true)]
        public bool PublishMarketDataMarketClosedEventTest(string providerid, string name, double value)
        {
            OnDataReceived = MarketDataReaderTestsDataReceivedHandler;
            var sec = new Security(providerid, name, value);
            PublishMarketData(sec);

            // Excepted result is this item will be published to 
            // listeners / clients.
            return (sec == data.First());
        }

        [TestCase(Result = true)]
        public bool PublishMarketDataNullDataHandlerTest()
        {
            var sec = new Security("Aggregator", "YYY", 9.05D);
            OnDataReceived = null;
            PublishMarketData(sec);

            // Excepted result is that NO data will be published to 
            // listeners / clients.
            return (null == _nullsec);
        }

        private void PublishMarketDataNullDataTestDataHandler(Security dataItem)
        {
            _nullsec = dataItem;
        }

        private void MarketDataReaderTestsDataReceivedHandler(Security dataItem)
        {
            data.Add(dataItem);
        }

        #endregion

        #region OnMarketInActivityEvent Test Methods

        Security _onMarketInActivityEvent;
        [TestCase(Result=true)]
        public bool OnMarketInActivityEventTest()
        {
            OnDataReceived = OnMarketInActivityEventTestHandler; 
            OnMarketInActivityEvent();
            
            return (_onMarketInActivityEvent.Name == Miscellaneous.INACTIVITY_TIMEOUT);
        }

        private void OnMarketInActivityEventTestHandler(Security dataItem)
        {
            _onMarketInActivityEvent = dataItem;
        }

        #endregion

        protected override void EndSession()
        {            
           //overridden to avoid wcf communication calls unrelated to unit test.            
        }
    }

    [TestFixture()]
    public class MarketDataReaderTests1 : MarketDataReader
    {
        Security data;
        private bool _canceltestEndSessionReceived;
        [TestCase("Aggregator", "YYY", 9.05D, Result = true)]
        public bool PublishMarketDataCancellationEventTest(string providerid, string name, double value)
        {
            var cts = new CancellationTokenSource();
            base._token = cts.Token;
            cts.Cancel();
            OnDataReceived = PublishMarketDataCancellationEventTestHandler;
            var sec = new Security(providerid, name, value);
            PublishMarketData(sec);

            // Excepted result is this item will NOT be published to 
            // listeners / clients, session will end and cancel property true
            return (_canceltestEndSessionReceived && IsCancellationRequested);
        }

        private void PublishMarketDataCancellationEventTestHandler(Security dataItem)
        {
            data = dataItem;
        }

        protected override void EndSession()
        {
            _canceltestEndSessionReceived = true;
            //overridden to avoid wcf communication calls unrelated to unit test.
        }
    }
}
