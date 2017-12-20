using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Wpf = System.Windows;
using WinForms = System.Windows.Forms;

namespace WebBrowserLib.Windows
{
    public class FocusDetector
    {
        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false; // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        private static void DoEvents()
        {
            Wpf.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(delegate { }));
        }

        public static void EnsureFocus(WinForms.Form mainWindow)
        {
            var n = 0;
            while (!ApplicationIsActivated() && n < 50)
            {
                mainWindow.BringToFront();
                mainWindow.Activate();
                WinForms.Application.DoEvents();
                n++;
            }
        }

        public static void EnsureFocus(Wpf.Window mainWindow)
        {
            var n = 0;
            while (!ApplicationIsActivated() && n < 50)
            {
                mainWindow.Activate();
                mainWindow.Topmost = true;
                mainWindow.Activate();
                mainWindow.Topmost = false;
                DoEvents();
                n++;
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}