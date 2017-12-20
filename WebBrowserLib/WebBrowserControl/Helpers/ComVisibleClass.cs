using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WebBrowserLib.WebBrowserControl.Helpers
{
    [ComVisible(true)]
    public class ComVisibleClass
    {

        public bool CodeToExecute()
        {
            Debugger.Break();
            return true;
        }
    }
}