using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WebBrowserLib.Helpers
{
    [ComVisible(true)]
    public class ComVisibleClass
    {
        public event EventHandler EventFromComVisibleClass;

        public bool HitBreakpoint { get; set; } = true;

        public bool CodeToExecute()
        {
            if (HitBreakpoint && Debugger.IsAttached)
                Debugger.Break();
            EventFromComVisibleClass?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}