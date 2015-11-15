using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TroyStevens.Market.Data;
using TroyStevens.Market.Extensions;

namespace TroyStevens.Market.Client.Data
{
    public delegate void NewDataReceivedEvent(SymbolTick dataItem);

    public class MarketData
    {
        protected CancellationTokenSource _cts;
        protected bool IsCancellationRequested { get { return (null != _cts && _cts.Token.IsCancellationRequested); } }
        protected BlockingCollection<SymbolTick> dataQueue;         
        protected NewDataReceivedEvent OnNewDataReceived;
        public bool IsThirdpartyProcessingDelayEnabled { get; private set; }
        public int ThirdpartyProcessingDelayInMilliseconds { get; private set; }
        public string ProviderId { get; set; }

        public MarketData(string providerid) 
        {
            _cts = new CancellationTokenSource();
            ProviderId = providerid;
            dataQueue = new BlockingCollection<SymbolTick>();
            IsThirdpartyProcessingDelayEnabled = Properties.Settings.Default.ThirdpartyProcessingDelayEnabled;
            ThirdpartyProcessingDelayInMilliseconds = TroyStevens.Market.Client.Properties.Settings.Default.ThirdpartyProcessingDelayInMilliseconds;
        }

        public void StreamMarketData(NewDataReceivedEvent newdatahandler)
        {
            if (null == newdatahandler)
                throw new ArgumentNullException();

            OnNewDataReceived = newdatahandler;

            var symbolReaderTask = new Task(SymbolReaderTaskWork);
            symbolReaderTask.Start();

            var symbolProcessorTask = new Task(SymbolProcessorTaskWork);
            symbolProcessorTask.Start();

            symbolReaderTask.Wait();
            dataQueue.CompleteAdding();                     

            symbolProcessorTask.Wait();

            _cts.Dispose();
            _cts = null;
        }

        protected virtual void SymbolReaderTaskWork()
        {
            MarketDataReader reader = new MarketDataReader(ProviderId);
            reader.RetrieveMarketData(ProviderId, DataReceived, _cts.Token);
        }

        protected virtual void SymbolProcessorTaskWork()
        {                 
            foreach(var data in dataQueue.GetConsumingEnumerable())
            {
                if (IsCancellationRequested)
                    break;

                SimulateThirdPartyProcessingDelay(data);

                if (null != OnNewDataReceived)
                    OnNewDataReceived(data);
            }

            //Parallel.ForEach(dataQueue.GetConsumingEnumerable(), (data, loopState) =>
            //    {
            //        if (IsCancellationRequested)
            //            loopState.Stop();

            //        SimulateThirdPartyProcessingDelay(data);

            //        if (null != OnNewDataReceived)
            //            OnNewDataReceived(data);
                    
            //    });                     
        }

        private void SimulateThirdPartyProcessingDelay(SymbolTick data)
        {                
            if (!IsThirdpartyProcessingDelayEnabled)
                return;

            // See FR5 for more details
            Thread.Sleep(ThirdpartyProcessingDelayInMilliseconds);
        } 

        internal void DataReceived(Security data)
        {
            dataQueue.Add((SymbolTick)data);
        }

        public void Disconnect()
        {
            if (null == _cts || _cts.IsCancellationRequested)
                return;

            _cts.Cancel();
            _cts.Token.WaitHandle.WaitOne(1500);
        }
    }
}
