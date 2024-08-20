using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace BLL
{
    public class TimerManager
    {
        public delegate void TimerTickDelegate();
        public event TimerTickDelegate TimerTick;
        public event TimerTickDelegate TimerForDiscretsTick;

        public void Start()
        {
            setTimer();
            timer.Start();
        }

        public void StartTimerForDiscrets()
        {
            setTimerForDiscretSignals();
            timerForDiscretSignals.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public void StopTimerForDiscrets()
        {
            timerForDiscretSignals.Stop();
        }

        private System.Timers.Timer timer;
        private System.Timers.Timer timerForDiscretSignals;

        private void setTimer()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void setTimerForDiscretSignals()
        {
            timerForDiscretSignals = new System.Timers.Timer(1000);
            timerForDiscretSignals.Elapsed += OnTimedEventForDiscrets;
            timerForDiscretSignals.AutoReset = true;
            timerForDiscretSignals.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            TimerTick();
        }

        private void OnTimedEventForDiscrets(Object source, ElapsedEventArgs e)
        {
            TimerForDiscretsTick();
        }
    }
}
