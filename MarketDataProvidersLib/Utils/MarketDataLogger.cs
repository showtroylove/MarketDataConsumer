using System.Diagnostics;
using TroyStevens.Market.Data;
using TroyStevens.Market.Extensions;

namespace TroyStevens.Market.Utils
{
    internal class MarketDataLogger
    {
        static int ListnerIndex { get; set; }
        static bool _loggingenabled;        

        static MarketDataLogger()
        {
            _loggingenabled = Properties.Settings.Default.LoggingFeatureEnabled;
        }

        public static bool EnableFileLogging(string suffix = "Market Data")
        {
            if (_loggingenabled)
            {
                var logfile = string.Empty;
                ListnerIndex = Trace.Listeners.Add(new TextWriterTraceListener(logfile.GenerateUniqueLogFileName(suffix: suffix), suffix));
                Trace.AutoFlush = true;
                Trace.UseGlobalLock = true;
            }
            return _loggingenabled;
        }

        public static void LogSymbolUpdate(Security s)
        {
            if (!_loggingenabled)
                return;

            Trace.WriteLine(s.ToString());
        }

        public static void Dispose()
        {
            if (!_loggingenabled)
                return;

            Trace.Listeners[ListnerIndex].Dispose();            
        }
    }
}
