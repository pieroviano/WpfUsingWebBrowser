using System;
using HostAppInPanelLib.Utility;
using UsingCefSharpFromWpf.Properties;
using UsingWebBrowserLib.Model;
using UsingWebBrowserLib.WebServer;
using WebBrowserLib.mshtml.WebBrowserControl;

namespace UsingCefSharpFromWpf
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            VersionPatcher.PatchInternetExplorerVersion();
            MainWindowModel.Instance.Port = 5007;
            var resourceResponseSender = new ResourceResponseSender(
                MainWindowModel.Instance.UrlPrefix,
                new[]
                {
                    typeof(Resources),
                    typeof(UsingWebBrowserLib.Properties.Resources)
                });
            var ws = new EmbeddedWebServer(resourceResponseSender.SendResponse, MainWindowModel.Instance.UrlPrefix);
            ws.Run();
            var wpfRunner = new WpfRunner(typeof(UsingCefSharpFromWpf.MainWindowWpf));
            wpfRunner.RunWpfFromMain();
            ws.Stop();
        }

    }
}