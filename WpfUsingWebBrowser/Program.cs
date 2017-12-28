using System;
using System.Net;
using HostAppInPanelLib.Utility;
using UsingWebBrowserFromWpf.Properties;
using UsingWebBrowserLib.Model;
using UsingWebBrowserLib.WebServer;
using WebBrowserLib.mshtml.WebBrowserControl;

namespace UsingWebBrowserFromWpf
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            VersionPatcher.PatchInternetExplorerVersion();

            var ws = new EmbeddedWebServer(SendResponse, MainWindowModel.UrlPrefix);
            ws.Run();
            var wpfRunner = new WpfRunner(typeof(MainWindowWpf));
            wpfRunner.RunWpfFromMain();
            ws.Stop();
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            var substring = request.Url.ToString().Substring(MainWindowModel.UrlPrefix.Length).ToLower()
                .Replace(".", "_").Replace("-", "_");
            var type = typeof(Resources);
            var propertyInfo = type.GetProperty(substring);
            if (propertyInfo == null)
            {
                type = typeof(UsingWebBrowserLib.Properties.Resources);
                propertyInfo = type.GetProperty(substring);
            }
            var value = (string)propertyInfo?.GetValue(type);
            return value;
        }
    }
}