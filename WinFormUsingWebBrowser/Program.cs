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

            var resourceResponseSender = new ResourceResponseSender(
                MainWindowModel.UrlPrefix,
                new[]
                {
                    typeof(Resources),
                    typeof(UsingWebBrowserLib.Properties.Resources)
                });
            var ws = new EmbeddedWebServer(resourceResponseSender.SendResponse, MainWindowModel.UrlPrefix);
            ws.Run();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UsingWebBrowserFromWinForm.MainWindow());
            ws.Stop();
        }

    }
}
