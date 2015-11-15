using System.Threading;

namespace TroyStevens.Market.Data.Processors
{
    public class ThirdPartyDelayProcessor : ISecurityProcessor
    {
        public int ProcessDelayInMilliseconds { get; set; }
        public bool EnableThirdPartyDelay { get; set; }

        public ThirdPartyDelayProcessor(bool enabledelay = true, int sleep = 1000)
        {
            // TODO: Consider placing this at a higher level to make this determination.
            ProcessDelayInMilliseconds = sleep; //Properties.Settings.Default.ThirdPartyProcessingDelayInMilliseconds;
            EnableThirdPartyDelay = enabledelay; //Properties.Settings.Default.IsThirdPartyProcessingDelayActive;
        }

        public bool ProcessSecurity(Security data)
        {
            // See FR5 for more info
            if (EnableThirdPartyDelay)
                Thread.Sleep(ProcessDelayInMilliseconds);
            return true;
        }
    }
}
