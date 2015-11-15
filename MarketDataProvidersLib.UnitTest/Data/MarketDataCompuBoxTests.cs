using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TroyStevens.Market.Data.Tests
{
    [TestFixture()]
    public class MarketDataCompuBoxTests
    {
        List<Security> mockdataList;

        [TestCase("Orion")] // Good
        [TestCase("Polaris")] // Good
        public void GenerateMockDataGoodProviderIdTest(string providerid)
        {
            mockdataList = new List<Security>();
            MarketDataCompuBox box = new MarketDataCompuBox(providerid);
            box.GenerateMockData(MockDataHandler);

            Assert.Greater(mockdataList.Count, 0);
        }

        [TestCase("SoftwareJedi"), ExpectedException(typeof(ArgumentException))]  // Bad 
        public void GenerateMockDataBadProviderIdTest(string providerid)
        {
            mockdataList = new List<Security>();
            MarketDataCompuBox box = new MarketDataCompuBox(providerid);            
        }

        [TestCase("Orion")] // Good
        [TestCase("Polaris")] // Good
        public void GetProviderSymbolsLoadTest(string providerid)
        {
            MarketDataCompuBox box = new MarketDataCompuBox(providerid);
            var symbols = box.GetProviderSymbols();
            Assert.IsTrue(symbols.Any());
        }

        [TestCase("Orion")] // Good
        [TestCase("Polaris")] // Good
        public void GetProviderSymbolsAreCorrectTest(string providerid)
        {
            var validsymbolnames = string.Empty;            

            MarketDataCompuBox box = new MarketDataCompuBox(providerid);
            var symbols = box.GetProviderSymbols();

            if(providerid == "Orion")
            {
                validsymbolnames = Properties.Settings.Default.CommaSeperatedProviderSymbolsOrion;
                
            }
            else if(providerid == "Polaris")            
            {
                validsymbolnames = Properties.Settings.Default.CommaSeperatedProviderSymbolsPolaris;                
            }        
            
            foreach (var name in validsymbolnames.Split(','))
                    Assert.IsTrue(symbols.Contains(name.Trim()));
        }

        [TestCase("Orion"), ExpectedException(typeof(ArgumentNullException))]  // Bad 
        public void GenerateMockDataHandlerExceptionTest(string providerid)
        {
            MarketDataCompuBox box = new MarketDataCompuBox(providerid);
            box.GenerateMockData(null);            
        }

        [TestCase("Orion")]  // Bad 
        public void GenerateMockAggregationExceptionHandledTest(string providerid)
        {
            Properties.Settings.Default.MaxNumberOfSymbolUpdates = 2;
            Properties.Settings.Default.MaxiumNumberOfSymbolBots = 1;

            MarketDataCompuBox box = new MarketDataCompuBox(providerid);
            box.GenerateMockData(ExceptionMockDataHandler);
            Assert.Pass("AggregationException handled from ThreadProc.");
        }

        private void MockDataHandler(Security data)
        {
            mockdataList.Add(data);
        }

        private void ExceptionMockDataHandler(Security data)
        {
            throw new Exception("Unit test exception");
        }
    }
}
