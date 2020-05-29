using System;
using System.Collections.Generic;
using System.Text;

namespace rJordanBot.Timers
{
    [Serializable]
    class NullTimerIntervalException : Exception
    { 
        public NullTimerIntervalException() : base("Timer interval is set to zero")
        {
            
        }
    }
}
