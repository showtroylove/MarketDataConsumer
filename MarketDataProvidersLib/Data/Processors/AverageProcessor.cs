using System;
using System.Collections.Generic;
using System.Diagnostics;
using TroyStevens.Market.Extensions;

namespace TroyStevens.Market.Data.Processors
{
    public class AverageProcessor : ISecurityProcessor
    {
        List<string> _dualListedSymbols = new List<string>() { "AAA", "BBB", "CCC" };
        Security last;

        public string Symbol { get; private set; }

        public AverageProcessor(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException("symbol is required");
            //if(!_dualListedSymbols.Contains(symbol))
            //    throw new ArgumentException("dual listed symbol is required");
            Symbol = symbol;            
        }

        /// <summary>
        /// Provide the capability to Average two securities based upon business rules.
        /// </summary>
        /// <param name="data">income security to be averaged</param>
        /// <returns>True if the processing engine should continue additional processing for end (false) processing the given security.</returns>
        public bool ProcessSecurity(Security data)
        {
            // See FR3 / FR4 for more info
            if (null == data || !_dualListedSymbols.Contains(data.Name))
                return true;

            if(null == last)
            {
                last = data;
                return true;
            }

            // See FR4
            if (last.AverageIfRequired(data))
            {
                var hdr = "---------- Averaged Instrument -------------";
                Trace.WriteLine(string.Format("{0}{2}{1}{2}{0}", hdr, data.ToString(), Environment.NewLine));
            }
            return true;
        }
    }    
}
