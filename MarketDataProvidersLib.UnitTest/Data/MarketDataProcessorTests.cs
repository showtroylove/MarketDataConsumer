using NUnit.Framework;
namespace TroyStevens.Market.Data.Tests
{
    [TestFixture()]
    internal class MarketDataProcessorTest : MarketDataProcessor
    {
        [TestCase(Result=3)]
        public int InitializedProviderSymbolsTest()
        {
            base.InitializedProviderSymbols();
            return base.securities.Count;            
        }

        [TestCase("AAA", Result = true)]
        [TestCase("BBB", Result = true)]
        [TestCase("CCC", Result = true)]
        public bool InitializedProviderSymbolsCorrectTest(string symbol)
        {
            return base.securities.ContainsKey(symbol);
        }

        [TestCase(Result = 9.25D)]
        public double ProcessSecurityAverageRequiredTest()
        {
            // securities dict already preinit with 10.5 from Orion
            var newsecurity = new Security("Polaris", "AAA", 8D);
            base.ProcessSecurity(newsecurity);

            return newsecurity.Value;
        }

        [TestCase("III", 8D, Result = 8D)]
        [TestCase("GGG", 10.5D, Result = 10.5D)]
        public double ProcessSecurityAvailableInOnlyOneProviderTest(string symbol, double value)
        {
            // securities dict already preinit with 10.5 from Orion
            var newsecurity = new Security("Polaris", symbol, value);
            base.ProcessSecurity(newsecurity);

            //expect unchanged value
            return newsecurity.Value;
        }

        [TestCase("AAA", 8D, Result = 8D)]
        [TestCase("BBB", 5.5D, Result = 5.5D)]
        [TestCase("CCC", 9.35D, Result = 9.35D)]
        public double ProcessSecurityUpdateFromSameProviderTest(string symbol, double value)
        {
            // securities dict already preinit with 10.5 from Orion
            var newsecurity = new Security("Orion", symbol, value);
            base.ProcessSecurity(newsecurity);

            //expect unchanged value
            return newsecurity.Value;
        }
    }
}
