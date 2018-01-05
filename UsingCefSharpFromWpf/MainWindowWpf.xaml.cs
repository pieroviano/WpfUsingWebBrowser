﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using CefSharp.Wpf;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Model;
using WebBrowserLib.CefSharp.WebBrowserControl;
using WebBrowserLib.Interfaces;
using WpfAdornedControl;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingCefSharpFromWpf
{
    /// <summary>
    ///     Interaction logic for MainWindowWpf.xaml
    /// </summary>
    public partial class MainWindowWpf
    {
        public MainWindowWpf()
        {
            InitializeComponent();
            WebBrowser = new ChromiumWebBrowser();
            _model = MainWindowModel.Instance;
            _controller =
                new MainWindowController<object>(_model, WebBrowserExtensionCefSharp.GetInstance(WebBrowser));

            (_controller.WebBrowserExtension as WebBrowserExtensionCefSharp)?.RemoveHandlersOnNavigating(
                _model.GetCustomEventHandler,
                _model.SetCustomEventHandler);
            LoadingAdorner = AdornedControlWithLoadingWait.AdornControl(DockPanel, WebBrowser);
            var webBrowserExtensionWithEventBase =
                WebBrowserExtensionCefSharp.GetInstance(WebBrowser) as IWebBrowserExtensionWithEventBase<object>;
            if (webBrowserExtensionWithEventBase != null)
            {
                webBrowserExtensionWithEventBase
                    .DocumentReady += WebBrowser_LoadCompleted;
            }
        }

        public ChromiumWebBrowser WebBrowser { get; }
        private AdornedControlWithLoadingWait LoadingAdorner { get; }

        private async void MenuItemCallApi_Click(object sender, RoutedEventArgs e)
        {
            var tuple = await _controller.DoCallApi(WebBrowser.Address,
                new Func<string, MessageBoxResult>(MessageBox.Show));
            var hasToLogin = tuple.Item1;
            var hasToNavigate = tuple.Item2;
            LoginOrNavigateIfNecessary(hasToLogin, hasToNavigate);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WebBrowser_LoadCompleted(object sender, EventArgs eventArgs)
        {
            bool isIdentityServer;
            var url = GetCurrentUrl();

            StatusBar.Text =
                _controller.HandleStatusAndGetUrl(out isIdentityServer, url);

            HandleScripts(isIdentityServer, url);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.AppSettings["DontStartStop"]?.ToLower() != "true")
            {
                LoadingAdorner.StartStopWait(WebBrowser);
            }
            _controller.WebBrowserExtension.Navigate(_model.UrlPrefix + _model.IndexPage);
        }
    }
}