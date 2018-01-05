﻿using System;
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

            MainWindowModel.Instance.Port = 5003;
            var resourceResponseSender = new ResourceResponseSender(
                MainWindowModel.Instance.UrlPrefix,
                new[]
                {
                    typeof(Resources),
                    typeof(UsingWebBrowserLib.Properties.Resources)
                });
            var ws = new EmbeddedWebServer(resourceResponseSender.SendResponse, MainWindowModel.Instance.UrlPrefix);
            ws.Run();
            var wpfRunner = new WpfRunner(typeof(MainWindowWpf));
            wpfRunner.RunWpfFromMain();
            ws.Stop();
        }

    }
}