using System;
using TroyStevens.Market.Data;
using TroyStevens.Market.Providers;

namespace MarketDataProvidersLib.UnitTest.Providers
{
    #region TestFixture Mock Object
    public class MockOperationContext : IMarketDataProviderCallback
    {
        public DataReceivedHandler OnPublishMaketData { get; set; }
        public MockOperationContext()
        {
            SessionId = Guid.NewGuid().ToString();
        }

        public string SessionId { get; set; }

        public IMarketDataProviderCallback GetCallbackChannel()
        {
            return (IMarketDataProviderCallback)this;
        }

        public void PublishMarketData(Security data)
        {
            // MockPublish
            if (null != OnPublishMaketData)
                OnPublishMaketData(data);
        }
    }

    #endregion
}
