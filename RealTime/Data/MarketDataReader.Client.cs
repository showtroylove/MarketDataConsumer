using System;
using System.ServiceModel;
using System.Threading;
using TroyStevens.Market.Data;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;
using TroyStevens.Market.Utils;

namespace TroyStevens.Market.Client.Data
{
    [CallbackBehaviorAttribute(IncludeExceptionDetailInFaults = true, UseSynchronizationContext = true,
                               ValidateMustUnderstand = true, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MarketDataReader : IMarketDataProviderCallback
    {        
        protected MarketDataConsumer client;
        protected DataReceivedHandler OnDataReceived;
        protected ManualResetEvent waitHandle;
        protected object synchObj;
        protected int ProviderInactivtyTimeOutInMilliseconds { get; set; }
        private long _recordCount;
        private long _lastPollCount;
        protected CancellationToken _token;
        protected string ProviderId {get; set;}
        protected long LastPollCount
        {
            get { return _lastPollCount; }
        }
        public long RecordCount
        {
            get { return _recordCount; }
        }       
        
        protected bool IsCancellationRequested { get { return (null != _token && _token.IsCancellationRequested); } }

        public MarketDataReader() : this("Aggregator")
        {            
        }
        public MarketDataReader(string providerid)
        {
            if(string.IsNullOrEmpty(providerid))
            {
                ProviderId = "Aggregator";
            }

            ProviderInactivtyTimeOutInMilliseconds = Properties.Settings.Default.ProviderInactivtyTimeOutInMilliseconds;
            waitHandle = new ManualResetEvent(false);
            synchObj = new object();
        }

        public void PublishMarketData(Security data)
        {
            if (null == data)
                return;

            Interlocked.Increment(ref _recordCount);

            // Market closed event or user requested cancellation
            if (data.Name == Miscellaneous.END_OF_FEED)
            {
                OnMarketClosedEvent(data);
                return;
            }
            else if (IsCancellationRequested)
            {
                EndSession();
                return;
            }
            
            if (null != OnDataReceived)
                OnDataReceived(data);
        }

        protected void OnMarketClosedEvent(Security data)
        {
            EndSession();            
            if (null != OnDataReceived)
                OnDataReceived(data);
        }

        protected void OnMarketInActivityEvent()
        {
            EndSession();

            // Market closed event or user requested cancellation
            if (null != OnDataReceived)
                OnDataReceived(new Security("RealTime", Miscellaneous.INACTIVITY_TIMEOUT, 0D));
        }

        protected virtual void EndSession()
        {
            if (null == client)
                return;

            lock (synchObj)
            {                
                try
                {
                    if (client.State == CommunicationState.Opened ||
                        client.State == CommunicationState.Opening)
                    {
                        client.EndSession();
                        client.Close();
                    }
                    else
                        client.Abort();
                }
                catch (Exception)
                {
                    client = null;
                }
                finally
                {
                    client = null;
                    if (!waitHandle.WaitOne(0))
                        waitHandle.Set();
                }
            }
        }

        public void RetrieveMarketData(string providerid, DataReceivedHandler datareceivedhandler, CancellationToken token)
        {
            if (null == datareceivedhandler || string.IsNullOrEmpty(providerid))
                throw new ArgumentNullException();

            ProviderId = providerid;
            _token = token;
            OnDataReceived = datareceivedhandler;            
            InitializeDatafeed();
            EstablishSession();

            //Poll for completion or inactivity
            var poller = new InActivityPoller<ManualResetEvent>(waitHandle, KeepPolling, ProviderInactivtyTimeOutInMilliseconds);
            poller.Poll();
        }

        protected bool KeepPolling()
        {
            if (RecordCount > LastPollCount)
            {
                Interlocked.Exchange(ref _lastPollCount, RecordCount);
                return true;
            }

            // Inactivity too long
            OnMarketInActivityEvent();
            return true;
        }

        protected virtual void EstablishSession()
        {
            try
            {
                client.GetAllProviderEvents();
            }
            catch(Exception ex)
            {
                EndSession();
                throw ex;
            }
        }

        protected virtual void InitializeDatafeed()
        {
            try
            {
                client = new TroyStevens.Market.Data.MarketDataConsumer(new InstanceContext(this), ProviderId);
                client.Open();
            }
            catch (Exception ex)
            {
                EndSession();
                throw ex;
            }
        }
    }
}
