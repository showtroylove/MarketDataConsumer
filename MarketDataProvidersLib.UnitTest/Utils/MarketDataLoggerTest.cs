using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroyStevens.Market.Utils;
using NUnit.Framework;
using System.Diagnostics;

namespace MarketDataProvidersLib.UnitTest.Utils
{
    [TestFixture]
    public class MarketDataLoggerTest
    {
        [TestCase(Result=false)]
        public bool EnableFileLoggingDisabledTest()
        {
            Assert.IsTrue(!MarketDataLogger.EnableFileLogging());            
            var listener = Trace.Listeners.OfType<TextWriterTraceListener>();
            if (!listener.Any())
                return true;
            bool bFound = false;
            foreach (var itm in listener)
            {
                if (itm.Name == "Market Data")
                {
                    bFound = true;
                    break;
                }
            }           
            
            return bFound;
        }        
    }
}
