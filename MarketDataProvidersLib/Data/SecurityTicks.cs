using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;
using TroyStevens.Market.Utils;
using System.Threading;

namespace TroyStevens.Market.Data
{
    public interface ISecurityProcessor
    {
        bool ProcessSecurity(Security data);        
    }

    public class SecurityTicks
    {
        private bool _addComplete;
        public virtual BlockingCollection<Security> SymbolTicks { get; protected set; }
        public List<ISecurityProcessor> Processors { get; private set; }

        public CancellationToken CancelToken { get; set; }
        
        public string Symbol { get; private set; }
        //public AddingComplete OnAddComplete { get { return CompleteAdding; } }

        public SecurityTicks(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentNullException("symbol is required.");

            Symbol = symbol;            
            SymbolTicks = new BlockingCollection<Security>();
            Processors = new List<ISecurityProcessor>();            
        }

        public void CompleteAdding()
        {
            _addComplete = true;
        }
        
        public void OnDataReceived(Security data)
        {
            if (null == data || data.Name != Symbol)
                return;
            SymbolTicks.Add(data);
        }

        public virtual void PublishSymbolTicks(DataReceivedHandler publisher)
        {
            if(null == publisher)
                throw new ArgumentNullException("publisher callback required");            
            
            while(!SymbolTicks.IsCompleted)
            {
                // See FR6 for more info
                var latest = SymbolTicks.OrderByDescending(t => t.TimeStamp)
                                        .TakeWhile(x => x.TimeStamp <= SymbolTicks.Max(m => m.TimeStamp));
                if (!latest.Any() && _addComplete)
                    break;
                else if (!latest.Any())
                    continue;

                var data = TakeAllAndReturnMax(latest.ToList());                
                if (Processors.Any())
                {
                    for (var i = 0; i < Processors.Count(); i++)
                        if (!Processors[i].ProcessSecurity(data))
                            break;
                }

                MarketDataLogger.LogSymbolUpdate(data);
                publisher(data);
            }

            SymbolTicks.CompleteAdding();
        }

        private Security TakeAllAndReturnMax(List<Security> latest)
        {
            var timestamp = latest.Max(t => t.TimeStamp);            
            Security max = null;
            foreach(var itm in latest)
            {
                SymbolTicks.Remove(itm);
                if (itm.TimeStamp == timestamp)
                    max = itm;
            }
            return max;
        }
    }
}
