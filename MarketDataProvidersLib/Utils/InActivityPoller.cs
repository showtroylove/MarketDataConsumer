using System;
using System.Threading;

namespace TroyStevens.Market.Utils
{
    public delegate bool KeepPolling();
    public class InActivityPoller<T> where T : EventWaitHandle
    {
        KeepPolling _keepPolling;
        T _waitHndl;
        bool _stoppoll;
        int _pollinterval;        

        public InActivityPoller(T wait, KeepPolling func, int pollinterval = 15000)
        {
            if (null == wait || null == func)
                throw new ArgumentNullException("wait and / or pollingfunc are null.");

            _waitHndl = wait;
            _keepPolling = func;
            _pollinterval = pollinterval;
        }

        public void Poll()
        {
            //Poll for completion or inactivity
            _waitHndl.WaitOne(_pollinterval);
            
            while (_keepPolling())
            {
                if (_stoppoll || _waitHndl.WaitOne(_pollinterval)) // Signaled
                    break;                
            }       
        }

        public void StopPolling()
        {
            lock (_waitHndl)
            {
                _stoppoll = true;
            }
        }

        public void SignaledStopPolling()
        {
            _waitHndl.Set();
        }

    }
}
