using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TroyStevens.Market.Client.Data;
using NUnit.Framework;
using System.Windows.Media;
using TroyStevens.Market.Data;

namespace TroyStevens.Market.Client.Data.Tests
{
    [TestFixture()]
    public class SymbolTickTests
    {
        [TestCase("AAA", 5.6D, 6.1D, Result = "#FF3CB371")]
        [TestCase("AAA", 8.5D, 5.3D, Result = "#FFFF0000")]
        [TestCase("AAA", 10.1D, 10.5D, Result = "#FF3CB371")]
        [TestCase("AAA", 5.6D, 5.6, Result = "#FF000000")]
        public string DeltaColorTest(string name, double previouslast, double last)
        {
            // Base line
            var tick = new SymbolTick(name, previouslast);
            tick.Last = last;

            var color = tick.DeltaColor.ToString();
            return color;
        }

        [TestCase("Orion", "TROY", 6.5D, Result= true)]
        [TestCase("Orion", "STEVENS", 5.35D, Result = true)]
        [TestCase("Orion", "SOLUTIONS", 9.95D, Result = true)]
        public bool SecuritytoSymbolTickExplicitOperatorTest(string providerid, string name, double value)
        {
            SymbolTick tick = (SymbolTick)new Security(providerid, name, value);
            return (tick.Symbol == name && tick.Last == value);
        }
    }
}
