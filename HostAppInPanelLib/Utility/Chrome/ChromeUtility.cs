using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HostAppInPanelLib.Utility.Win32;

namespace HostAppInPanelLib.Utility.Chrome
{
    public class ChromeUtility
    {
        public static Process GetChromeProcess(WrapperWindow window)
        {
            Process processById;
            do
            {
                Process process;
                processById = GetChromeProcess(window, out process);
            } while (processById == null);
            return processById;
        }

        public static Process GetChromeProcess(WrapperWindow window, 
            out Process process)
        {
            process = new Process();
            var info = new ProcessStartInfo
            {
                FileName = @"chrome.exe",
                Arguments = "about:blank" + " --new-window",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            window.Process = process;

            uint processId;
            IntPtr firstOrDefault;
            do
            {
                var chromeWindowTitles = ChromeWindowFinder.ChromeWindows()
                    .FindWindowByTitle("about:blank - Google Chrome");
                firstOrDefault = chromeWindowTitles.FirstOrDefault();
                Win32Interop.GetWindowThreadProcessId(firstOrDefault, out processId);
            } while (processId == 0);
            var processById = Process.GetProcessById(unchecked((int)processId));
            try
            {
                while (processById.HasExited)
                {
                    Win32Interop.GetWindowThreadProcessId(firstOrDefault, out processId);
                    processById = Process.GetProcessById(unchecked((int)processId));
                }
                processById.EnableRaisingEvents = true;
            }
            catch
            {
                var processesByName = Process.GetProcessesByName("chrome");
                foreach (var processbyname in processesByName)
                {
                    if (!processbyname.HasExited)
                        processbyname.Kill();
                }
                return null;
            }
            return processById;
        }
    }
}
