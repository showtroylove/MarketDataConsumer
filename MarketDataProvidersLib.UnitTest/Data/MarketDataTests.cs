using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TroyStevens.Market.Client.Data;
using NUnit.Framework;
using System.Collections.Concurrent;
using TroyStevens.Market.Data;
namespace TroyStevens.Market.Client.Data.Tests
{
    [TestFixture()]
    public class MarketDataTests : MarketData
    {
        BlockingCollection<SymbolTick> _data;
        private bool _dataReceived = false;
        
        public MarketDataTests()
            : base("MarketDataTests")
        {
            _data = new BlockingCollection<SymbolTick>();
        }

        protected override void SymbolReaderTaskWork()
        {
            // Tested independantly. See MarketDataReaderTest.         
        }
        
        [TestCase(ExpectedException = typeof(ArgumentNullException))]
        public void StreamMarketDataTestWithNullHandler()
        {
            StreamMarketData(null);            
        }

        [TestCase(Result = false)]
        public bool SymbolProcessorTaskWorkDiconnectCancellationTest()
        {
            if (null == _cts)
                _cts = new System.Threading.CancellationTokenSource();
            Disconnect();
            base.OnNewDataReceived = SymbolProcessorTaskWorkCancellationTest;
            dataQueue.CompleteAdding();
            SymbolProcessorTaskWork();
            return _dataReceived;
        }
        
        [TestCase(Result = true)]
        public bool DiconnectNullTest()
        {
            if (null != _cts)
                _cts = null;            
            Disconnect();           
            
            return !IsCancellationRequested;
        }
        private void SymbolProcessorTaskWorkCancellationTest(SymbolTick dataItem)
        {
            _dataReceived = true;
        }
   
    }

    [TestFixture()]
    public class MarketDataTests2 : MarketData
    {
        BlockingCollection<SymbolTick> _data;        

        public MarketDataTests2()
            : base("MarketDataTests2")
        {
            _data = new BlockingCollection<SymbolTick>();
        }
        private void DataReceivedHandler(SymbolTick data)
        {
            _data.Add(data);
        }

        protected override void SymbolReaderTaskWork()
        {
            // Tested independantly. See MarketDataReaderTest.
            var tick = new SecurityTicksNoSkip("AAA");
            dataQueue.Add(tick);
            dataQueue.CompleteAdding();
        }

        [TestCase(ExpectedException=typeof(ArgumentNullException))]
        public void StreamMarketDataNullTest()
        {
            StreamMarketData(null);            
        }        
    }

    [TestFixture()]
    public class MarketDataTests3 : MarketData
    {
        BlockingCollection<SymbolTick> _data;
        private bool _dataReceived;

        public MarketDataTests3()
            : base("MarketDataTests3")
        {
            _data = new BlockingCollection<SymbolTick>();
        }
        private void DataReceivedHandler(SymbolTick data)
        {
            _data.Add(data);
        }

        protected override void SymbolReaderTaskWork()
        {
            // Tested independantly. See MarketDataReaderTest.
            var tick = new SecurityTicksNoSkip("AAA");
            dataQueue.Add(tick);
            dataQueue.CompleteAdding();
        }

        [TestCase(Result = true)]
        public bool SymbolProcessorTaskWorkCancellationTest()
        {
            var tick = new SecurityTicksNoSkip("AAA");
            dataQueue.Add(tick);
            if (null == _cts)
                _cts = new System.Threading.CancellationTokenSource();
            _cts.Cancel();
            base.OnNewDataReceived = Handler;
            tick.CompleteAdding();
            dataQueue.CompleteAdding();
            SymbolProcessorTaskWork();
            
            return (!_dataReceived && IsCancellationRequested);
        }

        private void Handler(SymbolTick dataItem)
        {
            _dataReceived = true;
        }

    }
}
