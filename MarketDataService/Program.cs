using System;
using System.Linq;
using System.ServiceModel;
using TroyStevens.Market.Extensions;

namespace TroyStevens.Market.DataService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!Miscellaneous.ValidCommandLineArguments(args))
            {
                Console.WriteLine("Provider host name required. Valid names are Orion, Polaris, Aggregator.");
                Console.ReadLine();
                return;
            }

            ServiceHost provider = null;
            string name = args[0];

            try
            {
                provider = provider.MarketDataServiceFactory(name);
                provider.Faulted += new EventHandler(ProviderHost_FaultHandler);
                provider.Open();
                var endpoint = provider.Description.Endpoints.FirstOrDefault(x => x.Name.ToUpper() == name.ToUpper());

                Console.WriteLine(string.Format("The {0} Provider service is running and is listening on:", name));
                if (null != endpoint)
                    Console.WriteLine("{0} ({1})", endpoint.Address.ToString(), endpoint.Binding.Name);

                Console.WriteLine(string.Format("\nPress [ENTER] to exit the {0} service.", name));
                Console.ReadKey();
            }
            finally
            {
                if (provider.State == CommunicationState.Faulted)
                {
                    provider.Abort();
                }
                else if (provider.State == CommunicationState.Opened ||
                         provider.State == CommunicationState.Opening)
                {
                    provider.Close();
                }
            }
        }
             
        public static void ProviderHost_FaultHandler(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("The provider host has faulted."));
        }
    }
}