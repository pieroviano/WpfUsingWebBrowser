using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using HostAppInPanelLib;
using HostAppInPanelLib.Utility;
using HostAppInPanelLib.Utility.Chrome;
using HostAppInPanelLib.Utility.Win32;

namespace HostAppInPanel
{
    public static class Program
    {
        private static Process _process;

        private static void Application_Startup(object sender, StartupEventArgs e)
        {
            var window = new WrapperWindow();
            var processById = ChromeUtility.GetChromeProcess(window);
            window.Process = processById;
            window.KillProcessOnClose = true;
            window.Show();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            //ChromeDriverService chromeDriverService;
            //var driver = SeleniumUtility.GetChromeDriverHidden(out chromeDriverService);

            //var driverProcessIds = SeleniumUtility.GetChromeProcesses(chromeDriverService);
            //_process = driverProcessIds.FirstOrDefault();
            ////foreach (var process in SeleniumUtility.GetChromeProcesses(chromeDriverService))
            ////{
            ////    IntPtr handle = process.Handle;
            ////    Win32Interop.ShowWindowAsync(handle, ShowInfo.ShowMinimized);
            ////}
            //driver.Url = "http://www.google.it";
            var application = new Application();
            application.Startup += Application_Startup;
            application.Run();
            //var seleniumProcesses = SeleniumUtility.GetSeleniumProcesses(chromeDriverService);
            //foreach (var seleniumProcess in seleniumProcesses)
            //{
            //    if (!seleniumProcess.HasExited)
            //    {
            //        seleniumProcess.Kill();
            //    }
            //}
        }
    }
}