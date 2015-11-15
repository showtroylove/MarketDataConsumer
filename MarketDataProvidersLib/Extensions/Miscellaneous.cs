using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using TroyStevens.Market.Data;
using TroyStevens.Market.Providers;

namespace TroyStevens.Market.Extensions
{
    public static class Miscellaneous
    {
        public const string END_OF_FEED = "-0-";
        public const string INACTIVITY_TIMEOUT = "-1-";
        public const string ORION = "ORION";
        public const string POLARIS = "POLARIS";
        public const string AGGREGATOR = "AGGREGATOR";
        public static string ExePath
        {
            get
            {
                var dirName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                if (dirName.StartsWith("file:\\"))
                    dirName = dirName.Remove(0, 6);
                return dirName;
            }
        } 

        public static string GenerateUniqueLogFileName(this string f, 
                                                       string suffix = "Market Data", 
                                                       string ext = "log", 
                                                       bool exeDirectory = true)
        {
            var lsuffix = suffix;
            if(string.IsNullOrEmpty(lsuffix))
                lsuffix = "Market Data";

            var lext = ext;
            if (string.IsNullOrEmpty(lext))
                lext = "Market Data";
            
            var logfiledir = string.Empty;
            if(exeDirectory)
                logfiledir = ExePath;
            var logfile = $"{DateTime.Now.ToString("yyyy-MM-dd-hhmmss")} - {lsuffix}.{lext}";

            return Path.Combine(logfiledir, logfile);
        }

        public static ServiceHost MarketDataServiceFactory(this ServiceHost host, string name)
        {
            ServiceHost svc = null;
            if (name.ToUpper() == ORION)
                svc = new ServiceHost(new MarketDataProvider());
            else if(name.ToUpper() == POLARIS)
                svc = new ServiceHost(new PolarisProvider());
            else if (name.ToUpper() == AGGREGATOR)
                svc = new ServiceHost(new AggregatorProvider());
            return svc;
        }

        public static bool AverageIfRequired(this Security prevSec, Security newSec)
        {
            if (null == prevSec || prevSec.ProviderId == newSec.ProviderId || prevSec.Name != newSec.Name)
            {   // Averaging NOT required...bail
                prevSec = newSec;
                return false;
            }
            
            newSec.Value = new double[] { prevSec.Value, newSec.Value }.Average();
            prevSec = newSec;
            return true;            
        }     

        public static bool ValidCommandLineArguments(string[] args)
        {
            var providers = new List<string>() { "Orion", "Polaris", "Aggregator" };
            if (null == args || !args.Any() || !providers.Any(x => x.ToUpper() == args[0].ToUpper()))
                return false;           

            return true;
        }

        public static bool Remove<T>(this BlockingCollection<T> self, T itemToRemove)
        {
            lock (self)
            {
                T comparedItem;
                var itemsList = new List<T>();
                do
                {
                    var result = self.TryTake(out comparedItem);
                    if (!result)
                        return false;
                    if (!comparedItem.Equals(itemToRemove))
                    {
                        itemsList.Add(comparedItem);
                    }
                } while (!(comparedItem.Equals(itemToRemove)));
                Parallel.ForEach(itemsList, t => self.Add(t));
            }
            return true;
        }
    }
}
