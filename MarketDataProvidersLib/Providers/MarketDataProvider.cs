using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading.Tasks;
using TroyStevens.Market.Data;
using TroyStevens.Market.Extensions;

namespace TroyStevens.Market.Providers
{
    public delegate void DataReceivedHandler(Security dataItem);

    [ServiceBehavior(ConfigurationName = "TroyStevens.Market.Providers.Orion", 
                     Name = "TroyStevens.Market.Providers.Orion", 
                     ConcurrencyMode = ConcurrencyMode.Multiple,
                     InstanceContextMode = InstanceContextMode.Single)]
    internal class MarketDataProvider : MarketDataProviderBase
    {          
        protected BlockingCollection<Security> dataQueue;
        private object synchLock;        
        private bool MarketDataInitialized { get; set; }        

        public MarketDataProvider() : this("Orion")
        {
        }

        public MarketDataProvider(string providerid) : base(providerid)
        {
            if (string.IsNullOrEmpty(ProviderId))
                throw new ArgumentNullException("ProviderId must be supplied.");

            MarketDataInitialized = false;
            dataQueue = new BlockingCollection<Security>();
            synchLock = new object();
        }

        public override void Publish()
        {           
            
            var symbolReaderTask = new Task(SymbolReaderTaskWork);
            symbolReaderTask.Start();

            var symbolProcessorTask = new Task(SymbolProcessorTaskWork);
            symbolProcessorTask.Start();

            symbolReaderTask.Wait();
            dataQueue.CompleteAdding();

            symbolProcessorTask.Wait();

            // Send Market Closed Security (Cheesy, I know, but this is only a POC).
            // This will automatically Close session between the Aggregator and Orion / Polaris
            PublishMarketData(new Security(ProviderId, Miscellaneous.END_OF_FEED, 0D));
        }   

        public override void GetAllProviderEvents()
        {
            base.GetAllProviderEvents();

            lock (synchLock)
            {
                if (!MarketDataInitialized)
                    MarketDataInitialized = true;
                else
                    return;
            }

            Publish();  
        }

        protected virtual void SymbolReaderTaskWork()
        {
            MarketDataCompuBox box = new MarketDataCompuBox(ProviderId);
            box.GenerateMockData(DataReceived);
        }

        protected virtual void SymbolProcessorTaskWork()
        {            
            Parallel.ForEach(dataQueue.GetConsumingEnumerable(), data =>
            {                
                PublishMarketData(data);
            });
        }

        internal virtual void DataReceived(Security data)
        {
            dataQueue.Add(data);
        }
    }    
}
