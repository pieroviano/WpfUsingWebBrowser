using System.Diagnostics;
using Microsoft.Win32;

namespace WebBrowserLib.WebBrowserControl
{
    public class VersionPatcher
    {
        public static void PatchInternetExplorerVersion()
        {
            decimal ieVersion = 0;
            int regVal;

            // get the installed IE version
            var openSubKey = Registry.LocalMachine
                .OpenSubKey(@"Software\Microsoft\Internet Explorer");
            if (openSubKey != null)
            {
                var value = (string) openSubKey.GetValue("Version");
                var strings = value.Split('.');
                value = $"{strings[0]}.{strings[1]}";
                ieVersion = decimal.Parse(value);
            }

            //// set the appropriate IE version
            if (ieVersion >= 11)
            {
                regVal = 11001;
            }
            else if (ieVersion >= 10)
            {
                regVal = 10001;
            }
            else if (ieVersion >= 9)
            {
                regVal = 9999;
            }
            else if (ieVersion >= 8)
            {
                regVal = 8888;
            }
            else
            {
                regVal = 7000;
            }

            // set the actual key
            using (var key =
                Registry.CurrentUser.CreateSubKey(
                    @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                    RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (key != null && key.GetValue(Process.GetCurrentProcess().ProcessName + ".exe") == null)
                {
                    key.SetValue(Process.GetCurrentProcess().ProcessName + ".exe", regVal,
                        RegistryValueKind.DWord);
                }
            }
        }
    }
}