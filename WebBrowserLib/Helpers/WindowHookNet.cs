using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WebBrowserLib.Helpers
{
    /// <summary>
    ///     stores data about the raised event
    /// </summary>
    public class WindowHookEventArgs
    {
        public IntPtr Handle = IntPtr.Zero;
        public string WindowClass;
        public string WindowTitle;

        public override string ToString()
        {
            return "[WindowHookEventArgs|Title:" + WindowTitle + "|Class:"
                   + WindowClass + "|Handle:" + Handle + "]";
        }
    }

    /// <summary>
    ///     allows you to get information about the creation and /or the destruction of
    ///     windows
    /// </summary>
    public class WindowHookNet
    {
        /// <summary>
        ///     use this to get informed about window creation / destruction events
        /// </summary>
        /// <param name="aSender"></param>
        /// <param name="aArgs">contains information about the window</param>
        public delegate void WindowHookDelegate(object aSender, WindowHookEventArgs aArgs);

        private const int MAXTITLE = 255;
        private static WindowHookNet cInstance;
        private readonly List<WindowHookEventArgs> iEventsToFire = new List<WindowHookEventArgs>();

        private readonly Dictionary<IntPtr, WindowHookEventArgs> iNewWindowList =
            new Dictionary<IntPtr, WindowHookEventArgs>();

        private readonly Dictionary<IntPtr, WindowHookEventArgs> iOldWindowList =
            new Dictionary<IntPtr, WindowHookEventArgs>();

        private bool iRun;

        private readonly Thread iThread;

        private WindowHookNet()
        {
            ThreadStart tStart = Run;
            iThread = new Thread(tStart);
        }

        #region properties

        /// <summary>
        ///     made singleton to save up CPU cycles
        /// </summary>
        public static WindowHookNet Instance
        {
            get
            {
                if (null == cInstance)
                {
                    cInstance = new WindowHookNet();
                }

                return cInstance;
            }
        }

        #endregion

        private void EnumerateWindows()
        {
            EnumDelegate enumfunc = EnumWindowsProc;
            var hDesktop = IntPtr.Zero; // current desktop
            var success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);

            if (!success)
            {
                // Get the last Win32 error code
                var errorCode = Marshal.GetLastWin32Error();

                var errorMessage = string.Format(
                    "EnumDesktopWindows failed with code {0}.", errorCode);
                throw new Exception(errorMessage);
            }
        }

        private bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {
            var tArgument = new WindowHookEventArgs();

            tArgument.Handle = hWnd;
            tArgument.WindowTitle = GetWindowText(hWnd);
            tArgument.WindowClass = GetClassName(hWnd);

            iNewWindowList.Add(tArgument.Handle, tArgument);
            return true;
        }

        private void FireClosedWindows()
        {
            iEventsToFire.Clear();
            foreach (var tPtr in iOldWindowList.Keys)
            {
                // if the old list contains a key that is not
                // in the new list, that window has been destroyed
                // add it into the fire list
                if (!iNewWindowList.ContainsKey(tPtr))
                {
                    iEventsToFire.Add(iOldWindowList[tPtr]);
                }
            }

            // you need to remove / add things later, because
            // you are not allowed to alter the dictionary during iteration
            foreach (var tArg in iEventsToFire)
            {
                iOldWindowList.Remove(tArg.Handle);
                onWindowDestroyed(tArg);
            }
        }

        private void FireCreatedWindows()
        {
            iEventsToFire.Clear();
            foreach (var tPtr in iNewWindowList.Keys)
            {
                // if the new list contains a key that is not
                // in the old list, that window has been created
                // add it into the fire list and to the "old" list
                if (!iOldWindowList.ContainsKey(tPtr))
                {
                    iEventsToFire.Add(iNewWindowList[tPtr]);
                }
            }

            // you need to remove / add things later, because
            // you are not allowed to alter the dictionary during iteration
            foreach (var tArg in iEventsToFire)
            {
                iOldWindowList.Add(tArg.Handle, tArg);
                onWindowCreated(tArg);
            }
        }

        public static string GetClassName(IntPtr hWnd)
        {
            var title = new StringBuilder(MAXTITLE);
            var titleLength = _GetClassName(hWnd, title, title.Capacity + 1);
            title.Length = titleLength;

            return title.ToString();
        }

        /// <summary>
        ///     Returns the caption of a window by given HWND identifier.
        /// </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            var title = new StringBuilder(MAXTITLE);
            var titleLength = _GetWindowText(hWnd, title, title.Capacity + 1);
            title.Length = titleLength;

            return title.ToString();
        }

        private event WindowHookDelegate InnerWindowCreated;
        private event WindowHookDelegate InnerWindowDestroyed;

        private void onWindowCreated(WindowHookEventArgs aArgs)
        {
            if (null != InnerWindowCreated)
            {
                InnerWindowCreated(this, aArgs);
            }
        }

        private void onWindowDestroyed(WindowHookEventArgs aArgs)
        {
            if (null != InnerWindowDestroyed)
            {
                InnerWindowDestroyed(this, aArgs);
            }
        }

        private void Run()
        {
            try
            {
                while (iRun)
                {
                    iNewWindowList.Clear();
                    EnumerateWindows();
                    // if the hook has been freshly installed
                    // simply copy the new list to the "old" one
                    if (0 == iOldWindowList.Count)
                    {
                        foreach (var tKVP in iNewWindowList)
                        {
                            iOldWindowList.Add(tKVP.Key, tKVP.Value);
                        }
                    }
                    else // the hook has been running for some time
                    {
                        FireClosedWindows();
                        FireCreatedWindows();
                    }
                    Thread.Sleep(500);
                }

                // if the hook has been uninstalled
                // delete the list of old windows
                // because when it is restarted you do not want to get a whole 
                // lot of events for windows that where already there

                iOldWindowList.Clear();
            }
            catch (Exception aException)
            {
                Console.Out.WriteLine("exception in thread:" + aException);
            }
        }

        public void Shutdown()
        {
            if (iRun)
            {
                iRun = false;
            }
        }

        /// <summary>
        ///     register to this event if you want to be informed about
        ///     the creation of a window
        /// </summary>
        public event WindowHookDelegate WindowCreated
        {
            add
            {
                InnerWindowCreated += value;
                if (!iRun)
                {
                    iRun = true;
                    iThread.Start();
                }
            }
            remove
            {
                InnerWindowCreated -= value;

                // if no more listeners for the events
                if (null == InnerWindowCreated &&
                    null == InnerWindowDestroyed)
                {
                    iRun = false;
                }
            }
        }

        /// <summary>
        ///     register to this event, if you want to be informed about
        ///     the destruction of a window
        /// </summary>
        public event WindowHookDelegate WindowDestroyed
        {
            add
            {
                InnerWindowDestroyed += value;
                if (!iRun)
                {
                    iRun = true;
                    iThread.Start();
                }
            }
            remove
            {
                InnerWindowDestroyed -= value;

                // if no more listeners for the events
                if (null == InnerWindowCreated &&
                    null == InnerWindowDestroyed)
                {
                    iRun = false;
                }
            }
        }

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        #region DLLImport

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool _EnumDesktopWindows(IntPtr hDesktop,
            EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int _GetWindowText(IntPtr hWnd,
            StringBuilder lpWindowText, int nMaxCount);


        // GetClassName
        [DllImport("user32.dll", EntryPoint = "GetClassName", ExactSpelling = false,
            CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int _GetClassName(IntPtr hwnd, StringBuilder lpClassName,
            int nMaxCount);

        #endregion
    }
}