using System;
using System.Collections.Concurrent;
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
        /// <summary>
        /// New Properties and Fields
        /// </summary>
        protected ConcurrentDictionary<string, SecurityTicksNoSkip> securities;

        /// <summary>
        /// This callback is used to communicate with Provider
        /// </summary>
        protected NewSecurityTicksNoSkipEvent _onNewSecurityTicksEvent;
        /// <summary>
        /// Allows the Security objects to be passed directly to each SecurityTicks.
        /// The SecurityTicks object will process their own symbols and ignore the rest.
        /// </summary> 
        protected DataReceivedHandler OnDataReceived;
        /// <summary>
        /// Signals All the SecurityTicks the adding is complete.
        /// </summary>
        protected AddingComplete _onAddComplete;  
        ///////////////////////////////////////////


        protected MarketDataConsumer client;
        //protected DataReceivedHandler OnDataReceived;
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

            InitializeReader();

        }

        private void InitializeReader()
        {
            securities = new ConcurrentDictionary<string, SecurityTicksNoSkip>();
            ProviderInactivtyTimeOutInMilliseconds = Properties.Settings.Default.ProviderInactivtyTimeOutInMilliseconds;
            waitHandle = new ManualResetEvent(false);
            synchObj = new object();

            AddSecurityTick(Miscellaneous.END_OF_FEED);
            AddSecurityTick(Miscellaneous.INACTIVITY_TIMEOUT);
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

            PartitionSecurity(data);
            if (null != OnDataReceived)
                OnDataReceived(data);
        }

        protected virtual void PartitionSecurity(Security data)
        {
            if (securities.ContainsKey(data.Name))
                return;

            // Should ONLY perform adding
            var tick = securities.AddOrUpdate(data.Name, AddSecurityTick, UpdateSecurityTick);
            if(null != _onNewSecurityTicksEvent)
                _onNewSecurityTicksEvent(tick);
        }

        protected SecurityTicksNoSkip UpdateSecurityTick(string symbol, SecurityTicksNoSkip existingVal)
        {
            return existingVal;
        }

        protected SecurityTicksNoSkip AddSecurityTick(string symbol)
        {
            var tick = new SecurityTicksNoSkip(symbol);
            _onAddComplete += tick.CompleteAdding;
            OnDataReceived += tick.OnDataReceived;

            return tick;
        }

        protected void OnMarketClosedEvent(Security data)
        {                  
            if (null != OnDataReceived)
                OnDataReceived(data);
            EndSession();
        }

        protected void OnMarketInActivityEvent()
        {
            // Market closed event or user requested cancellation
            if (null != OnDataReceived)
                OnDataReceived(new Security("RealTime", Miscellaneous.INACTIVITY_TIMEOUT, 0D));
            EndSession();
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
                    SessionClosedEvent();
                }
            }
        }

        protected void SessionClosedEvent()
        {
            client = null;
            
            if (null != _onAddComplete)
                _onAddComplete();

            if(null != _onNewSecurityTicksEvent)
            {
                SecurityTicksNoSkip value = null;
                if(securities.TryGetValue(Miscellaneous.END_OF_FEED, out value))
                    _onNewSecurityTicksEvent(value);

                if(securities.TryGetValue(Miscellaneous.INACTIVITY_TIMEOUT, out value))
                    _onNewSecurityTicksEvent(value);
            }
            
            UnRegisterSymbolTicksDelegates();

            if (!waitHandle.WaitOne(0))
                waitHandle.Set();
        }

        protected void UnRegisterSymbolTicksDelegates()
        {
            foreach (var tick in securities.Values)
            {
                OnDataReceived -= tick.OnDataReceived;
                _onAddComplete -= tick.CompleteAdding;
            }
        }

        //public void RetrieveMarketData(string providerid, DataReceivedHandler datareceivedhandler, CancellationToken token)
        //{
        //    if (null == datareceivedhandler || string.IsNullOrEmpty(providerid))
        //        throw new ArgumentNullException();

        //    ProviderId = providerid;
        //    _token = token;
        //    OnDataReceived = datareceivedhandler;            
        //    InitializeDatafeed();
        //    EstablishSession();

        //    //Poll for completion or inactivity
        //    var poller = new InActivityPoller<ManualResetEvent>(waitHandle, KeepPolling, ProviderInactivtyTimeOutInMilliseconds);
        //    poller.Poll();
        //}

        public void RetrieveMarketData(string providerid, NewSecurityTicksNoSkipEvent datareceivedhandler, CancellationToken token)
        {
            if (null == datareceivedhandler || string.IsNullOrEmpty(providerid))
                throw new ArgumentNullException();

            ProviderId = providerid;
            _token = token;
            _onNewSecurityTicksEvent = datareceivedhandler;
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
