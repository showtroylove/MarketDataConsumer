using System.ServiceModel;
using System.ServiceModel.Channels;
using TroyStevens.Market.Providers;

namespace TroyStevens.Market.Data
{
    public partial class MarketDataConsumer : DuplexClientBase<IMarketDataProvider>, IMarketDataProvider
    {

        public MarketDataConsumer(InstanceContext callbackInstance) :
            base(callbackInstance)
        {
        }

        public MarketDataConsumer(InstanceContext callbackInstance, string endpointConfigurationName) :
            base(callbackInstance, endpointConfigurationName)
        {
        }

        public MarketDataConsumer(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public MarketDataConsumer(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress) :
            base(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public MarketDataConsumer(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) :
            base(callbackInstance, binding, remoteAddress)
        {
        }

        public void GetAllProviderEvents()
        {
            base.Channel.GetAllProviderEvents();
        }

        public void EndSession()
        {
            base.Channel.EndSession();
        }
    }
}
