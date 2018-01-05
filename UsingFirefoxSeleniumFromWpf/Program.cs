using System;
using HostAppInPanelLib.Controls;
using HostAppInPanelLib.Utility;
using UsingFirefoxSeleniumFromWpf.Properties;
using UsingWebBrowserLib.Model;
using UsingWebBrowserLib.WebServer;
using WebBrowserLib.mshtml.WebBrowserControl;

namespace UsingFirefoxSeleniumFromWpf
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            VersionPatcher.PatchInternetExplorerVersion();
            MainWindowModel.Instance.Port = 5006;

            var resourceResponseSender = new ResourceResponseSender(
                MainWindowModel.Instance.UrlPrefix,
                new[]
                {
                    typeof(Resources),
                    typeof(UsingWebBrowserLib.Properties.Resources)
                });
            var ws = new EmbeddedWebServer(resourceResponseSender.SendResponse, MainWindowModel.Instance.UrlPrefix);
            ws.Run();
            var wpfRunner = new WpfRunner(typeof(FirefoxMainWindow));
            wpfRunner.RunWpfFromMain();
            ws.Stop();
        }

    }
}