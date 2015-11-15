using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using TroyStevens.Market.Data;
using TroyStevens.Market.Data.Processors;
using TroyStevens.Market.Utils;

namespace TroyStevens.Market.Providers
{
    public delegate void NewSecurityTicksEvent(SecurityTicks data);
    public delegate void AddingComplete();    

    [ServiceBehavior(ConfigurationName="TroyStevens.Market.Providers.Aggregator", 
                     Name ="TroyStevens.Market.Providers.Aggregator", 
                     ConcurrencyMode = ConcurrencyMode.Multiple, 
                     InstanceContextMode = InstanceContextMode.Single)]
    internal class AggregatorProvider : MarketDataProvider
    {
        protected BlockingCollection<SecurityTicks> dataQueueAggregator;        
    
        public AggregatorProvider() : base("Aggregator")
        {
            dataQueueAggregator = new BlockingCollection<SecurityTicks>();
        }

        protected override void SymbolReaderTaskWork()
        {
            MarketDataReader dataconsumer = new MarketDataReader();
            dataconsumer.StreamFromProvider(OnNewSecurityTicksEvent);
            dataQueueAggregator.CompleteAdding();
        }

        protected void OnNewSecurityTicksEvent(SecurityTicks data)
        {
            dataQueueAggregator.Add(data);
        }

        protected override void SymbolProcessorTaskWork()
        {
            MarketDataLogger.EnableFileLogging(ProviderId);                 
            var thirdpartyprocessor = new ThirdPartyDelayProcessor();

            try
            {
                Parallel.ForEach(dataQueueAggregator.GetConsumingEnumerable(), data =>
                {
                    data.Processors.Add(new AverageProcessor(data.Symbol));
                    data.Processors.Add(thirdpartyprocessor);                   
                    
                    data.PublishSymbolTicks(PublishMarketData);                                       
                });                
            }
            catch (AggregateException e)
            {
                foreach (var t in e.InnerExceptions)
                {
                    Trace.WriteLine("\n-------------------------------------------------\n{0}", t.ToString());
                }
            }
            finally
            {
                MarketDataLogger.Dispose(); 
            }            
        }     
    }
}
