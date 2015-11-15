using System.ServiceModel;

namespace TroyStevens.Market.Providers
{
    [ServiceBehavior(ConfigurationName = "TroyStevens.Market.Providers.Polaris",
                     Name = "TroyStevens.Market.Providers.Polaris",
                     ConcurrencyMode = ConcurrencyMode.Multiple,
                     InstanceContextMode = InstanceContextMode.Single)]
    internal class PolarisProvider : MarketDataProvider
    {
        public PolarisProvider(): base("Polaris")
        {
        }
    }
}
