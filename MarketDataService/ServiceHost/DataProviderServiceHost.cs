using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using TroyStevens.Market.Providers;

namespace TroyStevens.Market.ServiceHost
{
    public class DataProviderServiceHost : System.ServiceModel.ServiceHost
    {
        #region Properties
        public string Name { get; private set; }
        public int Port
        {
            get
            {
                int port = 0;
                if (Name.ToUpper() == "ORION")
                    port = 9010;
                else if (Name.ToUpper() == "POLARIS")
                    port = 9011;
                else if (Name.ToUpper() == "AGGREGATOR")
                    port = 9012;
                return port;                    
            }
        }
        #endregion

        public DataProviderServiceHost(string name)
            : base(new MarketDataProvider(name), new Uri("http://localhost:8081/SelfHostedProvider/mex"))
        {
            Name = name;            
        }

        public ServiceEndpoint AddDefaultServiceEndpoint()
        {   
            string addr = string.Format("net.tcp://localhost:{0}/{1}", Port, Name);           
            //OptionalReliableSession ors = new OptionalReliableSession( new ReliableSessionBindingElement(true) );
            //ors.InactivityTimeout = new TimeSpan(0,10,0);
            return AddServiceEndpoint(typeof(IMarketDataProvider), new NetTcpBinding(SecurityMode.None, true), addr);
        }

        public void EnableMetaDataExchange()
        {
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;            
            Description.Behaviors.Add(smb);            
            AddServiceEndpoint(typeof(IMetadataExchange),MetadataExchangeBindings.CreateMexHttpBinding(),
                               "http://localhost:8081/SelfHostedProvider/mex");
        }
    }
}
