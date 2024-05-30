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
        public static event TimerTickDelegate TimerTick;
        public static event TimerTickDelegate TimerForDiscretsTick;

        public static void Start()
        {
            setTimer();
            timer.Start();
        }

        public static void StartTimerForDiscrets()
        {
            setTimerForDiscretSignals();
            timerForDiscretSignals.Start();
        }

        public static void Stop()
        {
            timer.Stop();
        }

        public static void StopTimerForDiscrets()
        {
            timerForDiscretSignals.Stop();
        }

        private static System.Timers.Timer timer;
        private static System.Timers.Timer timerForDiscretSignals;

        private static void setTimer()
        {
            timer = new System.Timers.Timer(1500);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private static void setTimerForDiscretSignals()
        {
            timerForDiscretSignals = new System.Timers.Timer(1000);
            timerForDiscretSignals.Elapsed += OnTimedEventForDiscrets;
            timerForDiscretSignals.AutoReset = true;
            timerForDiscretSignals.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            TimerTick();
        }

        private static void OnTimedEventForDiscrets(Object source, ElapsedEventArgs e)
        {
            TimerForDiscretsTick();
        }
    }
}
