using System;
using NUnit.Framework;
using TroyStevens.Market.Data.Processors;

namespace TroyStevens.Market.Data.Tests
{
    [TestFixture()]
    public class AverageProcessorTests
    {
        [TestCase("Orion", "AAA", 5.50D, "Polaris", 6.60D, Result = 6.05D)]
        [TestCase("Orion", "AAA", 5.50D, "Orion", 6.60D, Result = 6.60D)]        
        public double AverageProcessorProcessSecurityTest(string provider1, 
                                                          string name,
                                                          double val1, 
                                                          string provider2,
                                                          double val2)
        {
            ISecurityProcessor processor = new AverageProcessor(name);
            var sec1 = new Security(provider1, name, val1);
            processor.ProcessSecurity(sec1);

            var sec2 = new Security(provider2, name, val2);
            processor.ProcessSecurity(sec2);

            return (Math.Round(sec2.Value,2));
        }

        [TestCase(null, Result = true)]
        public bool AverageProcessorProcessSecurityNullDataTest(Security data)
        {
            ISecurityProcessor processor = new AverageProcessor("AAA");            
            processor.ProcessSecurity(data);

            return (null == data);
        }

        [TestCase(null, ExpectedException=typeof(ArgumentNullException))]
        public void AverageProcessorProcessSecurityNullSymbolTest(string data)
        {
            ISecurityProcessor processor = new AverageProcessor(data);            
        }        
    }
}
