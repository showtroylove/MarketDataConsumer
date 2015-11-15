using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TroyStevens.Market.Extensions;
using TroyStevens.Market.Providers;
using TroyStevens.Market.Utils;

namespace TroyStevens.Market.Data
{
    internal class MarketDataCompuBox
    {        
        public string ProviderId { get; private set; }
        public int MaxiumSymbolUpdates { get; set; }
        public List<string> ProviderSymbols { get; set; }

        public MarketDataCompuBox(string providerId)
        {
            ProviderId = providerId;
            MaxiumSymbolUpdates = Properties.Settings.Default.MaxNumberOfSymbolUpdates;
            ProviderSymbols = GetProviderSymbols();
        }

        public void GenerateMockData(DataReceivedHandler datareceivedHandler)
        {
            if(null == datareceivedHandler)
                throw new ArgumentNullException("DataReceivedEventHandler is null");

            MarketDataLogger.EnableFileLogging(ProviderId);
            var max = Properties.Settings.Default.MaxiumNumberOfSymbolBots;

            try
            {
                Parallel.For(0, max, task => ComputeSymbolValue(datareceivedHandler));
            }
            catch (AggregateException e)
            {
                for (int j = 0; j < e.InnerExceptions.Count; j++)
                {
                    Trace.WriteLine("\n-------------------------------------------------\n{0}", e.InnerExceptions[j].ToString());
                }
            }
            finally
            {
                MarketDataLogger.Dispose();
            }
        }

        internal void ComputeSymbolValue(DataReceivedHandler datareceivedHandler)
        {
            // Total Number of Symbols created / updated EOV = 
            // MaxiumSymbolUpdates X MaxiumNumberOfSymbolBots X GetProviderSymbols.Count
            List<string> symbols = ProviderSymbols;
            Random rnd = new Random();
            int MAXUPDATES = MaxiumSymbolUpdates;

            for (var i = 0; i < MAXUPDATES; i++)
            {
                Parallel.ForEach(symbols, name =>
                {
                    var value = rnd.Next(55, 106) / 10D;
                    var s = new Security(ProviderId, name, value);

                    MarketDataLogger.LogSymbolUpdate(s);
                    if (null != datareceivedHandler)
                        datareceivedHandler(s);
                });
            }
        }  

        internal List<string> GetProviderSymbols()
        {
            var symbols = string.Empty;
            if (ProviderId.ToUpper() == Miscellaneous.ORION)
                symbols = Properties.Settings.Default.CommaSeperatedProviderSymbolsOrion;
            else if (ProviderId.ToUpper() == Miscellaneous.POLARIS)
                symbols = Properties.Settings.Default.CommaSeperatedProviderSymbolsPolaris;
            else
                throw new ArgumentException("Invalid Provider.");

            return symbols.Split(',').ToList();
        }
    }
}
