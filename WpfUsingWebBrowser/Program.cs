using System;
using System.Net;
using UsingWebBrowserLib.Model;
using UsingWebBrowserLib.WebServer;
using WebBrowserLib.mshtml.WebBrowserControl;
using WebBrowserLib.WebBrowserControl;
using WpfUsingWebBrowser.Properties;

namespace WpfUsingWebBrowser
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            VersionPatcher.PatchInternetExplorerVersion();

            var ws = new EmbeddedWebServer(SendResponse, MainWindowModel.UrlPrefix);
            ws.Run();
            var app = new App();
            app.InitializeComponent();
            app.Run();
            ws.Stop();
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            var substring = request.Url.ToString().Substring(MainWindowModel.UrlPrefix.Length).ToLower()
                .Replace(".", "_").Replace("-", "_");
            var propertyInfo = typeof(UsingWebBrowserLib.Properties.Resources).GetProperty(substring);
            var value = (string) propertyInfo?.GetValue(typeof(UsingWebBrowserLib.Properties.Resources));
            return value;
        }
    }
}