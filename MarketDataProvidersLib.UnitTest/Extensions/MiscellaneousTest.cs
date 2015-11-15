using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using System.Linq;
using TroyStevens.Market.Data;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;

namespace TroyStevens.Market.UnitTest
{    
    
    /// <summary>
    ///This is a test class for SynchronizeTest and is intended
    ///to contain all SynchronizeTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class MiscellaneousTest
    {
        /// <summary>
        ///A test for AverageIfRequired
        ///</summary>
        [Test()]
        public void AverageIfRequiredTest()
        {
            Assert.Pass("See Process Security AverageRequiredTest and others test methods.");
        }
        
        /// <summary>
        ///A tests for GenerateUniqueLogFileName
        ///</summary>        
        [TestCase("FOOBAR", "log", true, TestName = "GenerateUniqueLogFileNameWithSuffixArgsTest")]
        [TestCase("", "log", true, TestName = "GenerateUniqueLogFileNameWithFileExtensionArgsTest")]
        [TestCase("", "", true, TestName = "GenerateUniqueLogFileNameWithEXEDirectoryCreationOptionTRUEArgTest")]
        [TestCase("", "", false, TestName = "GenerateUniqueLogFileNameWithEXEDirectoryCreationOptionFALSEArgTest")]
        public void GenerateUniqueLogFileNameTest(string suffix = "Market Data", string ext = "log", bool exeDirectory = true)
        {
            string f = string.Empty;      
            
            var exeDirPath = Miscellaneous.ExePath;
            var actual = f.GenerateUniqueLogFileName(suffix, ext, exeDirectory);

            Assert.IsTrue(actual.Contains(suffix) && 
                          actual.EndsWith(ext) && 
                          (System.IO.Directory.Exists(exeDirPath) && exeDirectory ? actual.Contains(exeDirPath) : !actual.Contains(exeDirPath)));
        }

        [TestCase(TestName = "GenerateUniqueLogFileNameNoArgsTest")]
        public void GenerateUniqueLogFileNameTest()
        {
            string f = string.Empty;

            var exeDirPath = Miscellaneous.ExePath;
            var actual = f.GenerateUniqueLogFileName();

            Assert.IsTrue(actual.Contains("Market Data") &&
                          actual.EndsWith("log") && ( System.IO.Directory.Exists(exeDirPath) && actual.Contains(exeDirPath)));
        }

        /// <summary>
        ///A test for MarketDataServiceFactory
        ///</summary>
        [TestCase("AGGREGATOR", Result=true)]
        [TestCase("ORION", Result = true)]
        [TestCase("POLARIS", Result = true)]
        [TestCase("Ziploc", Result = false)]
        public bool MarketDataServiceFactoryReturnsCorrectProviderTest(string providerid)
        {
            ServiceHost mock = null;
            var svc = mock.MarketDataServiceFactory(providerid);
            
            if (providerid == Miscellaneous.ORION)
                return (svc.SingletonInstance is MarketDataProvider);
            else if (providerid == Miscellaneous.POLARIS)
                return  (svc.SingletonInstance is PolarisProvider);
            else if (providerid == Miscellaneous.AGGREGATOR)
                return (svc.SingletonInstance is AggregatorProvider);

            return false;
        }

        [TestCase("AGGREGATOR", Result = true)]
        [TestCase("ORION", Result = true)]
        [TestCase("POLARIS", Result = true)]
        [TestCase("Aggregator", Result = true)]
        [TestCase("Orion", Result = true)]
        [TestCase("Polaris", Result = true)]
        [TestCase("fubar", Result = false)]
        [TestCase("frag", Result = false)]
        [TestCase("exceptional", Result = false)]
        public bool ValidCommandLineArgumentsTest(string providerid)
        {
            string [] args = new string[]{ providerid };
            return Miscellaneous.ValidCommandLineArguments(args);            
        }

        [TestCase(Result = false)]
        public bool ValidCommandLineArgumentsNullParamTest()
        {            
            return Miscellaneous.ValidCommandLineArguments(null);
        }

        [TestCase(Result = false)]
        public bool ValidCommandLineArgumentsEmptyArrayParamTest()
        {
            List<string> args = new List<string>();
            return Miscellaneous.ValidCommandLineArguments(args.ToArray());
        }

        [TestCase(Result = true)]
        public bool RemoveTest()
        {
            BlockingCollection<Security> dataQueue = new BlockingCollection<Security>();
            var sec1 = new Security("Aggregator", "AAA", 6.32D);            
            var sec2 = new Security("Aggregator", "BBB", 6.22D);
            var sec3 = new Security("Aggregator", "CCC", 6.12D);
            dataQueue.Add(sec1);
            dataQueue.Add(sec2);
            dataQueue.Add(sec3);
            Assert.IsTrue(dataQueue.Remove<Security>(sec1));
            Assert.IsTrue(dataQueue.Count == 2);
            var itm = dataQueue.FirstOrDefault(x => x.Value == sec1.Value);
            return (null == itm);
        }

    
    }
}
