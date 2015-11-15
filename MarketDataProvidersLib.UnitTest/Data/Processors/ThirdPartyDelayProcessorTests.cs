using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroyStevens.Market.Data.Processors;
using NUnit.Framework;
namespace TroyStevens.Market.Data.Tests
{
    [TestFixture()]
    public class ThirdPartyDelayProcessorTests
    {
        [TestCase(Result=true)]
        public bool ProcessSecurityDelayEnabledTest()
        {
            var delay = new ThirdPartyDelayProcessor(sleep: 0);
            return delay.ProcessSecurity(null);            
        }
    }
}
