using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using TroyStevens.Market.Client.Data;
using TroyStevens.Market.Extensions;

namespace TroyStevens.Market.Client
{
    public interface INotifyViewClosing
    {
        ICommand OnViewClosing { get; }
    }

    public class RealTimeQuoteViewModel : BindableBase, INotifyViewClosing
    {
        ConcurrentDictionary<string, SymbolTick> dictSecurities;
        ObservableCollection <SymbolTick> symbolticks;
        MarketData _provider;
        public ICommand ConnectOrDisconnect { get; private set; }
        public ICommand PauseOrResume { get; private set; }
        public ICommand OnViewClosing { get; private set; }
        public string ProviderId { get; private set; }
        public RealTimeQuoteViewModel()
        {
            Initialize();
        }

        protected void Initialize()
        {
            dictSecurities = new ConcurrentDictionary<string, SymbolTick>();
            symbolticks = new ObservableCollection <SymbolTick> ();
            ResetCounter();
            ConnectionStatusText = "Connect";
            PauseStatusText = "Pause Updates";

            // Commands
            OnViewClosing = new DelegateCommand<System.ComponentModel.CancelEventArgs>(OnApplicationExitHandler); 
            ConnectOrDisconnect = new DelegateCommand<string>(OneConnectOrDisconnectHandler);
            PauseOrResume = new DelegateCommand(OnPauseOrResumeHandler);
        }

        public ObservableCollection<SymbolTick> MarketData
        {
            get
            {
                return symbolticks;
            }
            private set
            {
                symbolticks = value;
                OnPropertyChanged("MarketData");
            }
        }

        public ObservableCollection<string> Providers => new ObservableCollection<string> { "Aggregator", "Orion", "Polaris" };

        private string _connectionStatus;

        public string ConnectionStatusText
        {
            get { return _connectionStatus; }
            private set 
            {
                SetProperty(ref _connectionStatus, value);
            }
        }

        private string _pauseStatus;
        public string PauseStatusText
        {
            get { return _pauseStatus; }
            private set
            {
                SetProperty(ref _pauseStatus, value);
            }
        }

        private string _recordsupdateText;
        public string RecordsUpdateText
        {
            get { return _recordsupdateText; }
            private set 
            {
                if (value == _recordsupdateText)
                    return;
                SetProperty(ref _recordsupdateText, value);
            }
        }
        
        public int RecordsUpdateCount { get; private set; }

        //Commands Handlers
        protected void OnApplicationExitHandler(System.ComponentModel.CancelEventArgs e)
        {
            OnEndSessionEvent();           
        }

        protected void OnPauseOrResumeHandler()
        {
            if(IsPaused)
            {
                Resume();
            }
            else 
            {
                Pause();                
            }
        }

        protected void OneConnectOrDisconnectHandler(string providerid = "Aggregator")
        {
            var providername = providerid;

            if (IsConnected)
            {
                OnEndSessionEvent();                    
            }
            else
            {
                if (string.IsNullOrEmpty(providername))
                    providername = "Aggregator";

                OnOpenSession(providername);
            }
        }

        protected void OnOpenSession(string providerid)
        {            
            if(null == _provider)
            {                
                _provider = new MarketData(providerid);
                ProviderId = _provider.ProviderId;
                Task.Factory.StartNew(() =>
                {
                    _provider.StreamMarketData(OnNewDataReceivedHandler);
                });
            }

            ResetCounter();
            Connect();
            Resume();
        }

        protected void OnEndSessionEvent()
        {
            _provider?.Disconnect();

            if (!IsConnected) return;

            Disconnect();
            Resume();
            _provider = null;
        }
        
        //Incoming Data Event Handler
        public void OnNewDataReceivedHandler(SymbolTick data)
        {
            // See FR12 for more info
            if (null == data || IsPaused)
                return;

            if (data.Symbol.ToUpper() == Miscellaneous.END_OF_FEED ||
                data.Symbol.ToUpper() == Miscellaneous.INACTIVITY_TIMEOUT)
            {
                RecordsUpdateText = 
                    data.Symbol.ToUpper() == Miscellaneous.END_OF_FEED ? 
                                             $"Market Closed! Updates: {RecordsUpdateCount++}" : 
                                             $"Market Inactivity! Updates: {RecordsUpdateCount++}";
                OnEndSessionEvent();
                return;
            }            

            RecordsUpdateText = $"Record updates: {RecordsUpdateCount++}"; 
            // See FR18 for more info
            dictSecurities.AddOrUpdate(data.Symbol, data, 
                                        (key, existingVal) =>
                                        {                        
                                            existingVal.Last = data.Last;
                                            return existingVal;
                                        });
            try
            {
                MarketData = new ObservableCollection<SymbolTick>(dictSecurities.Values);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #region View Helper Functions

        protected void ResetCounter()
        {
            UpdateCount(0);
        }

        protected void UpdateCount(int count)
        {
            RecordsUpdateText = $"Record updates: {count}";
        }

        protected bool IsConnected => (ConnectionStatusText.StartsWith("Disconnect"));

        protected void Connect()
        {
            ConnectionStatusText = "Disconnect";
        }

        protected void Disconnect()
        {
            ConnectionStatusText = "Connect";
        }

        protected bool IsPaused => (PauseStatusText.StartsWith("Resume"));

        protected void Pause()
        {
            PauseStatusText = "Resume Updates";
        }

        protected void Resume()
        {
            PauseStatusText = "Pause Updates";
        }
        
        #endregion
    }
}
