using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace rJordanBot.Timers
{
    public delegate Task TimerCompleted(Timer timer);

    public class Timer
    {
        public event TimerCompleted TimerDone;

        public int Interval;
        public int Elapsed = 0;
        public ulong ID;
        public string Name;
        public List<Timer> list;
        public Timer(int interval = 0, string name = "")
        {
            Interval = interval;
            Name = name;
        }

        public async Task Start()
        {
            if (Interval == 0)
            {
                throw new NullTimerIntervalException();
            }

            list.Add(this);
            Elapsed = 0;
            while (Elapsed > Interval)
            {
                await Task.Delay(1);
                Elapsed++;
                list.Add(this);
            }

            list.Remove(this);
            OnTimerDone();
        }

        protected virtual void OnTimerDone()
        {
            TimerDone?.Invoke(this);
        }
    }
}
