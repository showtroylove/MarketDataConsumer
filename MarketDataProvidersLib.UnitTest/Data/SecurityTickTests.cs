using System;
using NUnit.Framework;
using System.Linq;
using TroyStevens.Market.Providers;
using System.Threading;

namespace TroyStevens.Market.Data.Tests
{
    [TestFixture()]
    public class SecurityTicksTests
    {
        [TestCase(ExpectedException = typeof(ArgumentNullException))]
        public void SecurityTicksNullSymbolTest()
        {            
            var tick = new SecurityTicks(null);
        }

        [TestCase("Orion", "AAA", "AAA", 6.73D, Result = true)]
        [TestCase("Orion", "AAA", "GGG", 6.73D, Result = false)]
        public bool OnDataReceivedTest(string providerid, string symbol, string name, double value)
        {            
            var tick = new SecurityTicks(symbol);
            var sec = new Security(providerid, name, value);
            tick.OnDataReceived(sec);
            return tick.SymbolTicks.ToList().Contains(sec);
        }

        [TestCase("AAA", null, Result = false)]
        public bool OnDataReceivedNullDataTest(string symbol, Security sec)
        {
            var tick = new SecurityTicks(symbol);            
            tick.OnDataReceived(sec);
            return tick.SymbolTicks.ToList().Contains(sec);
        }

        [TestCase("AAA", ExpectedException = typeof(ArgumentNullException))]
        public void PublishSymbolTicksNullPublisher(string symbol)
        {
            var tick = new SecurityTicks(symbol);
            tick.PublishSymbolTicks(null);            
        }


        // See FR6 for more info
        SecurityTicks _tick;
        private Security _published;
        [TestCase("Orion", "AAA", "AAA", 6.73D, Result = true)]
        [TestCase("Orion", "BBB", "BBB", 8.25D, Result = true)]
        public bool PublishSymbolTicksNoProcessorsGetLatestTest(string providerid, string symbol, string name, double value)
        {            
            _tick = new SecurityTicks(symbol);           

            // Simulatation - Arrived in the processing Q while processing a Security.
            var sec1 = new Security("Aggregator", "AAA", 6.32D);
            _tick.OnDataReceived(sec1);
            var sec2 = new Security("Aggregator", "BBB", 6.22D);
            _tick.OnDataReceived(sec2);
            var sec3 = new Security("Aggregator", "CCC", 6.12D);
            _tick.OnDataReceived(sec3);

            // We want the input security to have the latest timestamp.
            Thread.Sleep(1000);
            // Now add our provided security
            var sec = new Security(providerid, name, value);
            _tick.OnDataReceived(sec);

            _tick.CompleteAdding();
            _tick.PublishSymbolTicks(
                PublishSymbolTicksTestPublisher);
            Assert.IsTrue(!_tick.SymbolTicks.Any());
            return _published.Equals(sec);
        }

        private void PublishSymbolTicksTestPublisher(Security dataItem)
        {            
            _published = dataItem;
        }

    }
}
