using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TroyStevens.Market.Client;
using NUnit.Framework;
using TroyStevens.Market.Client.Data;
using TroyStevens.Market.Extensions;
namespace TroyStevens.Market.Client.Tests
{
    [TestFixture()]
    public class RealTimeQuoteViewModelTests
    {

        #region OnNewDataReceivedHandler Test Methods

        SymbolTick testdata = new SymbolTick("AAA", 10.01D);

        [TestCase(Result=true)]
        public bool OnNewDataReceivedHandlerTest()
        {
            RealTimeQuoteViewModel vm = new RealTimeQuoteViewModel();
            vm.OnNewDataReceivedHandler(testdata);

            return (vm.MarketData.First() == testdata);
        }

        [TestCase(Result = false)]
        public bool OnNewDataReceivedHandlerNullDataTest()
        {
            RealTimeQuoteViewModel vm = new RealTimeQuoteViewModel();
            vm.OnNewDataReceivedHandler(null);

            // No data in no data out...
            return vm.MarketData.Any();
        }

        [TestCase(Result = true)]
        public bool OnNewDataReceivedHandlerEOFEventTest()
        {
            RealTimeQuoteViewModel vm = new RealTimeQuoteViewModel();
            vm.OnNewDataReceivedHandler(new SymbolTick(Miscellaneous.END_OF_FEED, 0D));

            return vm.RecordsUpdateText.StartsWith("Market Closed!");
        }

        [TestCase(Result = true)]
        public bool OnNewDataReceivedHandlerInActivityEventTest()
        {
            RealTimeQuoteViewModel vm = new RealTimeQuoteViewModel();
            vm.OnNewDataReceivedHandler(new SymbolTick(Miscellaneous.INACTIVITY_TIMEOUT, 0D));

            return vm.RecordsUpdateText.StartsWith("Market Inactivity!");
        }


        #endregion

        [TestCase(Result=true)]
        public bool ProviderStringTest()
        {
            RealTimeQuoteViewModel vm = new RealTimeQuoteViewModel();
            var providers = vm.Providers;
            return (providers.Count() == 3);
        }
    }
}
