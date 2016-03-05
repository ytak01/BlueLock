using System;
using System.Windows.Forms;

namespace BlueLock.Extensions
{
    /// <summary>
    /// Extensions for the Control element.
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Perform a safe action on this control.
        /// </summary>
        /// <param name="target">This control.</param>
        /// <param name="action">The action to perform.</param>
        public static void PerformSafely(this Control target, Action action)
        {
            if (target.InvokeRequired)
            {
                target.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}