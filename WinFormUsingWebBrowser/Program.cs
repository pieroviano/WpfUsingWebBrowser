using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using UsingWebBrowserLib.Model;
using UsingWebBrowserLib.WebServer;
using WebBrowserLib.mshtml.WebBrowserControl;
using WinFormUsingWebBrowser.Properties;

namespace WinFormUsingWebBrowser
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
            Application.Run(new MainWindow());
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
