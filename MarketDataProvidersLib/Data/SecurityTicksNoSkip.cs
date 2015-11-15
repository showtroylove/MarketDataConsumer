using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TroyStevens.Market.Client.Data;
using TroyStevens.Market.Data.Processors;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;
using TroyStevens.Market.Utils;

namespace TroyStevens.Market.Data
{
    public delegate void NewSecurityTicksNoSkipEvent(SecurityTicksNoSkip data);

    public class SecurityTicksNoSkip : SecurityTicks
    {      
        
        public SecurityTicksNoSkip(string symbol) : base(symbol)
        {
        }

        public void PublishSymbolTicks(NewDataReceivedEvent publisher, CancellationToken token)
        {
            if (null == publisher)
                throw new ArgumentNullException("publisher callback required");            

            foreach (var data in SymbolTicks.GetConsumingEnumerable())
            {
                if (token.IsCancellationRequested)
                    break;
                
                if (Processors.Any())
                {
                    foreach (var processor in Processors)
                        if (!processor.ProcessSecurity(data))
                            break;
                }

                if (null != publisher)
                    publisher((SymbolTick)data);
            }
           
            SymbolTicks.CompleteAdding();            
        }      
    }
}
