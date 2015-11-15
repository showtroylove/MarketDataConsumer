using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;
using TroyStevens.Market.Utils;

namespace TroyStevens.Market.Data
{
    [CallbackBehaviorAttribute(IncludeExceptionDetailInFaults = true, UseSynchronizationContext = true,
                               ValidateMustUnderstand = true, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    internal class MarketDataReader : IMarketDataProviderCallback
    {
        protected ConcurrentDictionary<string, SecurityTicks> securities;
        
        /// <summary>
        /// This callback is used to communicate with Provider
        /// </summary>
        protected NewSecurityTicksEvent _onNewSecurityTicksEvent;
        /// <summary>
        /// Allows the Security objects to be passed directly to each SecurityTicks.
        /// The SecurityTicks object will process their own symbols and ignore the rest.
        /// </summary> 
        protected DataReceivedHandler _onNewSecurityEvent; 
        /// <summary>
        /// Signals All the SecurityTicks the adding is complete.
        /// </summary>
        protected AddingComplete _onAddComplete;  
        protected bool IsMarketsClosed { get { return (null == Orion && null == Polaris); } }
        protected MarketDataConsumer Orion { get;  set; }
        protected MarketDataConsumer Polaris {  get; set; }
        
        private ManualResetEvent waitHandle;
        private int ProviderInactivtyTimeOutInMilliseconds { get; set; }
        private long _lastPollCount;
        private long LastPollCount
        {
            get { return _lastPollCount; }           
        }

        private long _recordCount;
        public long RecordCount
        {
            get { return _recordCount; }            
        }

        public MarketDataReader()
        {
            _recordCount = 0;
            waitHandle = new ManualResetEvent(false);
            ProviderInactivtyTimeOutInMilliseconds = Properties.Settings.Default.ProviderInactivtyTimeOutInMilliseconds;
            securities = new ConcurrentDictionary<string, SecurityTicks>();
        }

        public void PublishMarketData(Security data)
        {
            if (null == data)
                return;

            Interlocked.Increment(ref _recordCount);

            // Market closed event
            if (data.Name == Miscellaneous.END_OF_FEED)
            {
                // This shouldn't create a SecurityTick obj
                OnMarketClosedEvent(data);
                return;
            }

            PartitionSecurity(data);
            if(null != _onNewSecurityEvent)
                _onNewSecurityEvent(data);

            //if (null != dataProcessingHandler)
            //    dataProcessingHandler(data);
        }

        protected virtual void PartitionSecurity(Security data)
        {
            if (securities.ContainsKey(data.Name))
                return;            

            // Should ONLY perform adding
            var tick = securities.AddOrUpdate(data.Name, AddSecurityTick, UpdateSecurityTick);            
            _onNewSecurityTicksEvent(tick);
        }

        protected SecurityTicks UpdateSecurityTick(string symbol, SecurityTicks existingVal)
        {
            return existingVal;
        }

        protected SecurityTicks AddSecurityTick(string symbol)
        {               
            var tick = new SecurityTicks(symbol);
            _onAddComplete += tick.CompleteAdding;
            _onNewSecurityEvent += tick.OnDataReceived;
            
            return tick;
        }

        protected void OnMarketClosedEvent(Security data)
        {
            if (null == data)
                return;

            if (data.ProviderId.ToUpper() == Miscellaneous.ORION)
                EndSession(Orion);
            else if (data.ProviderId.ToUpper() == Miscellaneous.POLARIS)
                EndSession(Polaris);
        }

        protected void EndSession(MarketDataConsumer client)
        {
            if (null == client)
                return;
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
            catch (Exception ex)
            {
                Console.WriteLine("Close error caused exception [{0}]", ex.Message);
            }
            finally
            {
                client = null;
                SessionCloseEvent();
            }
        }

        protected void SessionCloseEvent()
        {
            if (IsMarketsClosed && !waitHandle.WaitOne(0))
            {
                if (null != _onAddComplete)
                    _onAddComplete();
                UnRegisterSymbolTicksDelegates();
                waitHandle.Set();
            }
        }

        protected void UnRegisterSymbolTicksDelegates()
        {
            foreach(var tick in securities.Values)
            {
                _onNewSecurityEvent -= tick.OnDataReceived; 
                _onAddComplete -= tick.CompleteAdding;
            }
        }

        public void StreamFromProvider(NewSecurityTicksEvent dataprocessingHandler)
        {
            if (null == dataprocessingHandler)
                // Typically we'd throw an exception, but this is a POC.
                return;

            _onNewSecurityTicksEvent = dataprocessingHandler;
            EstablishDatafeed();

            var poller = 
                new InActivityPoller<ManualResetEvent>(waitHandle, KeepPolling, 2 * ProviderInactivtyTimeOutInMilliseconds);
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
            EndSession(Orion);
            EndSession(Polaris);
            return false;            
        }

        protected void EstablishDatafeed()
        {
            InitializeDatafeed();
            EstablishSession();
        }

        protected void InitializeDatafeed()
        {
            try
            {
                Orion = new MarketDataConsumer(new InstanceContext(this), "Orion");
                Orion.InnerDuplexChannel.Closed += new System.EventHandler(OrionOnSessionClosed);
                Orion.Open();
            }
            catch (Exception oex)
            {
                Console.WriteLine("Failed to establish a connection with Orion Provider. Exception [{0}]", oex.Message);
                Orion = null;
            }

            try
            {
                Polaris = new MarketDataConsumer(new InstanceContext(this), "Polaris");
                Polaris.InnerDuplexChannel.Closed += new System.EventHandler(PolarisOnSessionClosed);
                Polaris.Open();
            }
            catch (Exception pex)
            {
                Console.WriteLine("Failed to establish a connection with Polaris Provider. Exception [{0}]", pex.Message);
                Polaris = null;
            }

            if (IsMarketsClosed)
                SessionCloseEvent();
        }

        void OrionOnSessionClosed(object sender, System.EventArgs e)
        {
            Orion = null;
            SessionCloseEvent();
        }

        void PolarisOnSessionClosed(object sender, System.EventArgs e)
        {
            Polaris = null;
            SessionCloseEvent();
        }

        protected void EstablishSession()
        {
            try
            {
                if (null != Orion)
                    Orion.GetAllProviderEvents();
            }
            catch (Exception oex)
            {
                Console.WriteLine("Failed to establish a session with Orion Provider. Exception [{0}]", oex.Message);
                Orion = null;
            }

            try
            {
                if (null != Polaris)
                    Polaris.GetAllProviderEvents();
            }
            catch (Exception pex)
            {
                Console.WriteLine("Failed to establish a session with Polaris Provider. Exception [{0}]", pex.Message);
                Polaris = null;
            }

            if (IsMarketsClosed)
                SessionCloseEvent();
        }
    }
}
