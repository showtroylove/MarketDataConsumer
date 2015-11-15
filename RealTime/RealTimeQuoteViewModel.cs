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
            OnViewClosing = new DelegateCommand<System.ComponentModel.CancelEventArgs>(this.OnApplicationExitHandler); 
            ConnectOrDisconnect = new DelegateCommand<string>( this.OneConnectOrDisconnectHandler);
            PauseOrResume = new DelegateCommand(this.OnPauseOrResumeHandler);
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

        public ObservableCollection<string> Providers
        {
            get { return new ObservableCollection<string>() { "Aggregator", "Orion", "Polaris" }; }
        }

        private string _connectionStatus;

        public string ConnectionStatusText
        {
            get { return _connectionStatus; }
            private set 
            {
                SetProperty<string>(ref _connectionStatus, value);
            }
        }

        private string _pauseStatus;
        public string PauseStatusText
        {
            get { return _pauseStatus; }
            private set
            {
                SetProperty<string>(ref _pauseStatus, value);
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
                SetProperty<string>(ref _recordsupdateText, value);
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

        protected void OneConnectOrDisconnectHandler(string providerid)
        {
            if (IsConnected)
            {
                OnEndSessionEvent();                    
            }
            else
            {
                OnOpenSession(providerid);
            }
        }

        protected void OnOpenSession(string providerid)
        {            
            if(null == _provider)
            {
                ProviderId = providerid;
                _provider = new MarketData(providerid);
                var task = Task.Factory.StartNew(() =>
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
            if (null != _provider)
                _provider.Disconnect();

            if (IsConnected)
            {
                Disconnect();
                Resume();
                _provider = null;
            }
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
                if (data.Symbol.ToUpper() == Miscellaneous.END_OF_FEED)
                    RecordsUpdateText = string.Format("Market Closed! Updates: {0}", RecordsUpdateCount++);
                else
                    RecordsUpdateText = string.Format("Market Inactivity! Updates: {0}", RecordsUpdateCount++);

                OnEndSessionEvent();
                return;
            }            

            RecordsUpdateText = string.Format("Record updates: {0}", RecordsUpdateCount++); 
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
            }                         
        }

        #region View Helper Functions

        protected void ResetCounter()
        {
            UpdateCount(0);
        }

        protected void UpdateCount(int count)
        {
            RecordsUpdateText = string.Format("Record updates: {0}", count);
        }

        protected bool IsConnected
        {
            get
            {
                return (ConnectionStatusText.StartsWith("Disconnect"));
            }
        }

        protected void Connect()
        {
            ConnectionStatusText = "Disconnect";
        }

        protected void Disconnect()
        {
            ConnectionStatusText = "Connect";
        }

        protected bool IsPaused
        {
            get
            {
                return (PauseStatusText.StartsWith("Resume"));
            }
        }

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
