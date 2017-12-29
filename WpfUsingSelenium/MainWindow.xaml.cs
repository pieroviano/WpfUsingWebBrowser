using System;
using System.ComponentModel;
using System.Windows;
using HostAppInPanelLib;
using OpenQA.Selenium;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.ChromeSelenium.WebBrowserControl;
using WebBrowserLib.Interfaces;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingChromeSeleniumFromWpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ChromeWrapperControl WebBrowser;

        public MainWindow()
        {
            InitializeComponent();
            WebBrowser = new ChromeWrapperControl();
            DockPanel.Children.Add(WebBrowser);

            _model = new MainWindowModel();
            MainWindowModel.StartupJavascript =
                "debugger;if(document.all['login']!=null)document.all['login'].style.display='none';if(document.all['logout']!=null)document.all['logout'].style.display='none';";
            _controller =
                new MainWindowController<IWebElement>(_model, WebBrowserExtensionSelenium.GetInstance(WebBrowser));

            _controller.WebBrowserExtension.DocumentReady += WebBrowser_LoadCompleted;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WebBrowser.LoadingAdornerControl.StartStopWait(WebBrowser);
            _controller.WebBrowserExtension.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
        }

        private async void MenuItemCallApi_Click(object sender, RoutedEventArgs e)
        {
            var tuple = await _controller.DoCallApi(WebBrowser.WebDriver.Url,
                new Func<string, MessageBoxResult>(MessageBox.Show));
            var hasToLogin = tuple.Item1;
            var hasToNavigate = tuple.Item2;
            LoginOrNavigateIfNecessary(hasToLogin, hasToNavigate);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItemLogout_OnClick(object sender, RoutedEventArgs e)
        {
            IdentityServerLogic.SetAuthorization(null);
            var documentWaiter = _controller.WebBrowserExtension as IDocumentWaiter;
            var targetUrl = MainWindowModel.IdentityServerSite + "account/logout";
            documentWaiter?.WaitForDocumentReady(targetUrl);
            var javascriptExecutor = _controller.WebBrowserExtension as IJavascriptExecutor;
            javascriptExecutor?.ExecuteJavascript(_model.LogoutJavascript);
        }

        private void WebBrowser_LoadCompleted(object sender, EventArgs eventArgs)
        {
            bool isIdentityServer;
            var url = _controller.WebBrowserExtension.GetCurrentUrl();

            StatusBar.Text =
                _controller.HandleStatusAndGetUrl(out isIdentityServer, url);

            HandleScripts(isIdentityServer, url);
            if (url.ToLower().StartsWith(MainWindowModel.IndexUrl.ToLower()))
            {
                _controller.WebBrowserExtension?
                    .ExecuteJavascript(MainWindowModel.StartupJavascript);
            }
            if (url.ToLower().StartsWith(MainWindowModel.CallBackUrl.ToLower()))
            {
                var documentWaiter = _controller.WebBrowserExtension as IDocumentWaiter;
                documentWaiter?.WaitForDocumentReady(MainWindowModel.IndexUrl);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
    }
}