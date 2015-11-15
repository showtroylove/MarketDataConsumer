using System;
using System.Collections.Concurrent;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using TroyStevens.Market.Data;

namespace TroyStevens.Market.Providers
{
    internal abstract class MarketDataProviderBase : IMarketDataProvider
    {
        public string ProviderId { get; protected set; }    
        protected virtual string ContextSessionId 
        {
            get
            {
                return OperationContext.Current.SessionId;
            }
        }
        protected virtual IMarketDataProviderCallback ContextCallbackChannel
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel < IMarketDataProviderCallback>();
            }
        }
        public long TotalDataPublished { get; set; }
        protected ConcurrentBag<ClientSession<IMarketDataProviderCallback>> clients;
        
        public MarketDataProviderBase(string providerid)
        {
            ProviderId = providerid;
            clients = new ConcurrentBag<ClientSession<IMarketDataProviderCallback>>();            
        }
        
        public abstract void Publish();

        public void PublishMarketData(Security data)
        {
            if (null == clients || !clients.Any() || null == data)
                return;

            TotalDataPublished++;
            Parallel.ForEach(clients, callback => InternalPublishMarketData(data, callback));
        }

        protected void InternalPublishMarketData(Security data, ClientSession<IMarketDataProviderCallback> callback)
        {
            if (null == callback || !callback.IsActive)
                return;

            try
            {   
                callback.ClientCallback.PublishMarketData(data);                
            }
            catch (Exception ex)
            {
                RemoveClient(callback);
                Console.WriteLine(
                    $"Failed to communicate with client with sessionId [{callback.SessionId}] causing exception [ {ex.Message} ].");                
            }
        }

        public virtual void GetAllProviderEvents()
        {
            AddClient();
        }

        public virtual void EndSession()
        {
            RemoveClient(ContextSessionId);
        }

        protected void AddClient()
        {
            var client = new ClientSession<IMarketDataProviderCallback>(ContextSessionId,
                ContextCallbackChannel);

            Console.WriteLine("New client session [{0}] subscribed at [{1}].", client.SessionId, client.SessionStart); 
            clients.Add(client);
        }

        protected void RemoveClient(string sessionId)
        {
            var client = clients.FirstOrDefault(x => x.SessionId == sessionId);
            RemoveClient(client);                   
        }

        protected void RemoveClient(ClientSession<IMarketDataProviderCallback> client)
        {
            if (null == client || !client.IsActive)
                return;

            client.EndSession();

            Console.WriteLine("End client session [{0}] ended at [{1}].", client.SessionId, client.SessionEnd); 
        }
    }
}
