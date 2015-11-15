using System;

namespace TroyStevens.Market.Providers
{
    internal class ClientSession<T> where T: IMarketDataProviderCallback
    {
        public string SessionId { get; private set; }
        public T ClientCallback { get; private set; }
        public bool IsActive
        {
            get
            {
                return (SessionEnd == DateTime.MinValue);
            }
        }
        public DateTime SessionStart { get; private set; }
        public DateTime SessionEnd { get; private set; }

        public void EndSession()
        {
            lock (ClientCallback)
            {
                SessionEnd = DateTime.Now;
            }
        }
        public ClientSession(string sessionId, T callback)
        {
            SessionId = sessionId;
            ClientCallback = callback;
            SessionStart = DateTime.Now;
            SessionEnd = DateTime.MinValue;
        }
    }
}
