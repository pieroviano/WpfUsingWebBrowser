using System;
using HostAppInPanelLib.Utility;
using UsingChromeSeleniumFromWpf.Properties;
using UsingWebBrowserLib.Model;
using UsingWebBrowserLib.WebServer;
using WebBrowserLib.mshtml.WebBrowserControl;

namespace UsingChromeSeleniumFromWpf
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            VersionPatcher.PatchInternetExplorerVersion();
            MainWindowModel.Port = 5005;

            var resourceResponseSender = new ResourceResponseSender(
                MainWindowModel.UrlPrefix,
                new[]
                {
                    typeof(Resources),
                    typeof(UsingWebBrowserLib.Properties.Resources)
                });
            var ws = new EmbeddedWebServer(resourceResponseSender.SendResponse, MainWindowModel.UrlPrefix);
            ws.Run();
            var wpfRunner = new WpfRunner(typeof(UsingChromeSeleniumFromWpf.MainWindow));
            wpfRunner.RunWpfFromMain();
            ws.Stop();
        }

    }
}