using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BlueLock.Extensions
{
    public static class TimerExtensions
    {
        /// <summary>
        /// Ensure that the timer is currently running; if not: start it.
        /// </summary>
        public static bool EnsureTimerRunning(this System.Timers.Timer timer, int interval)
        {
            if (timer.Enabled)
            {
                return false;
            }

            timer.Interval = interval;
            timer.Start();

            return true;
        }

        /// <summary>
        /// Ensure that the timer is currently not running; if it is: stop it.
        /// </summary>
        public static bool EnsureTimerStopped(this System.Timers.Timer timer)
        {
            if (!timer.Enabled)
            {
                return false;
            }

            timer.Stop();
            return true;
        }
    }
}
