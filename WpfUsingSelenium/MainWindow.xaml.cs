using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using HostAppInPanelLib;
using mshtml;
using OpenQA.Selenium;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Selenium.WebBrowserControl;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingSeleniumFromWpf
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
            WebBrowser= new ChromeWrapperControl();
            DockPanel.Children.Add(WebBrowser);

            _model = new MainWindowModel();
            _controller =
                new MainWindowController<IWebElement>(_model, WebBrowserExtensionSelenium.GetInstance(WebBrowser));

            _controller.WebBrowserExtensionWithEvent.DocumentReady += WebBrowser_LoadCompleted;
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
            _controller.WebBrowserExtensionWithEvent.InjectAndExecuteJavascript(_model.LogoutJavascript);
        }

        private void WebBrowser_LoadCompleted(object sender, EventArgs eventArgs)
        {
            bool isIdentityServer;
            var url = GetCurrentUrl();

            StatusBar.Text =
                _controller.HandleStatusAndGetUrl(out isIdentityServer, url);

            HandleScripts(isIdentityServer, url);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //WebBrowser.LoadingAdornerControl.StartStopWait(WebBrowser);
            _controller.WebBrowserExtensionWithEvent.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}