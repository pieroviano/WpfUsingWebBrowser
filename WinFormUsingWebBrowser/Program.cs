using System;
using System.Net;
using System.Windows.Forms;
using UsingWebBrowserFromWinForm.Properties;
using UsingWebBrowserLib.Model;
using UsingWebBrowserLib.WebServer;
using WebBrowserLib.mshtml.WebBrowserControl;

namespace UsingWebBrowserFromWinForm
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            VersionPatcher.PatchInternetExplorerVersion();
            MainWindowModel.Port = 5004;

            var ws = new EmbeddedWebServer(SendResponse, MainWindowModel.UrlPrefix);
            ws.Run();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UsingWebBrowserFromWinForm.MainWindow());
            ws.Stop();
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            var substring = request.Url.ToString().Substring(MainWindowModel.UrlPrefix.Length).ToLower()
                .Replace(".", "_").Replace("-", "_");
            var type = typeof(Resources);
            var propertyInfo = type.GetProperty(substring);
            if(propertyInfo==null)
            {
                type = typeof(UsingWebBrowserLib.Properties.Resources);
                propertyInfo=type.GetProperty(substring);
            }
            var value = (string)propertyInfo?.GetValue(type);
            return value;
        }
    }
}
