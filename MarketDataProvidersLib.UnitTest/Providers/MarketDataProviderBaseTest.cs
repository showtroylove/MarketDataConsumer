using System;
using System.Linq;
using MarketDataProvidersLib.UnitTest.Providers;
using NUnit.Framework;
using TroyStevens.Market.Data;
using TroyStevens.Market.Providers;

namespace TroyStevens.Market.UnitTest
{   

    [TestFixture]
    internal class MarketDataProviderBaseTest : MarketDataProviderBase
    {
        MockOperationContext Context = new MockOperationContext();

        public MarketDataProviderBaseTest() : base("MockProvider")
        {

        }
        protected override string ContextSessionId
        {
            get
            {
                return Context.SessionId;
            }            
        }

        protected override IMarketDataProviderCallback ContextCallbackChannel
        {
            get
            {
                return Context.GetCallbackChannel();
            }
        }

        [TestCase(null, Result=true)]        
        public bool PublicMarketDataNullDataTest(Security data)
        {
            var totalbeforepublish = base.TotalDataPublished;
            PublishMarketData(data);

            // Should be the same after for null data argument
            return (totalbeforepublish == base.TotalDataPublished);
        }

        [TestCase(Result=true)]
        public bool PublicMarketDataNullClientsTest()
        {
            // You have to call GetAllProvderEvents to establish a client 
            // session. 
            var totalbeforepublish = base.TotalDataPublished;
            PublishMarketData(new Security("Orion", "AAA", 0D));

            // Should be the same after for null data argument
            return (totalbeforepublish == base.TotalDataPublished);
        }

        [TestCase(Result = true)]
        public bool PublishMarketDataTest()
        {
            // You have to call GetAllProvderEvents to establish a client 
            // session.             
            var totalbeforepublish = base.TotalDataPublished;
            // Establishes a client session using the mock object
            GetAllProviderEvents();
            PublishMarketData(new Security("Orion", "AAA", 0D));

            // Should be the same after for null data argument
            return (totalbeforepublish < base.TotalDataPublished);
        }


        [TestCase(Result = true)]
        public bool PublishInternalMarketDataExceptionTest()
        {
            // You have to call GetAllProvderEvents to establish a client 
            // session.             
            Context.OnPublishMaketData = MockTestPublishMarketData;
            // Establishes a client session using the mock object
            GetAllProviderEvents();
            PublishMarketData(new Security("Orion", "AAA", 0D));

            // The client should be inactive
            return !clients.First().IsActive;
        }

        [TestCase(Result = true)]
        public bool RemoveClientBySessionIdTest()
        {            
            // Establishes a client session using the mock object
            GetAllProviderEvents();
            RemoveClient(clients.First().SessionId);

            // The client should be inactive
            return !clients.First().IsActive;
        }

        [TestCase(Result = true)]
        public bool RemoveClientByClientSessionTest()
        {
            // Establishes a client session using the mock object
            GetAllProviderEvents();
            RemoveClient(clients.First());

            // The client should be inactive
            return !clients.First().IsActive;
        }

        private void MockTestPublishMarketData(Security dataItem)
        {
            throw new NotImplementedException();
        }

        public override void Publish()
        {
            throw new NotImplementedException();
        }
    }  
}
