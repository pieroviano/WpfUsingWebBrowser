using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using HostAppInPanelLib.Controls;
using OpenQA.Selenium;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.ChromeSelenium.WebBrowserControl;
using WebBrowserLib.Interfaces;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingSeleniumFromWpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TextBlock _statusBar;
        public BrowserWrapperControl WebBrowser;

        public MainWindow(Type webBrowserType)
        {
            Width = 800;
            Height = 600;
            var dockPanel = new DockPanel();
            var menu = new Menu();
            var statusBarControl = new StatusBar();
            statusBarControl.Height = 20;
            statusBarControl.Margin = new Thickness(0, 10, 0, 0);
            var statusBarItem = new StatusBarItem();
            _statusBar = new TextBlock();
            statusBarItem.Content = _statusBar;
            statusBarControl.Items.Add(statusBarItem);
            menu.Margin = new Thickness(0, 0, 0, 10);
            var menuItem = new MenuItem {Header = "_File"};
            var callApi = new MenuItem {Header = "_Call API"};
            callApi.Click += MenuItemCallApi_Click;
            var logout = new MenuItem {Header = "_Logout"};
            logout.Click += MenuItemLogout_OnClick;
            var exit = new MenuItem {Header = "_Exit"};
            exit.Click += MenuItemExit_Click;
            menuItem.Items.Add(callApi);
            menuItem.Items.Add(logout);
            menuItem.Items.Add(new Separator());
            menuItem.Items.Add(exit);
            menu.Items.Add(menuItem);
            dockPanel.Children.Add(menu);
            dockPanel.Children.Add(statusBarControl);
            DockPanel.SetDock(menu, Dock.Top);
            DockPanel.SetDock(statusBarControl, Dock.Bottom);
            Content = dockPanel;
            WebBrowser = (BrowserWrapperControl) webBrowserType.GetConstructor(new Type[0])?.Invoke(new object[0]);
            dockPanel.Children.Add(WebBrowser);

            _model = MainWindowModel.Instance;
            _model.StartupJavascript =
                "if(document.all['login']!=null)document.all['login'].style.display='none';if(document.all['logout']!=null)document.all['logout'].style.display='none';";
            _controller =
                new MainWindowController<IWebElement>(_model, WebBrowserExtensionSelenium.GetInstance(WebBrowser));

            _controller.WebBrowserExtension.DocumentReady += WebBrowser_LoadCompleted;
            Loaded += MainWindow_OnLoaded;
            Closing += Window_Closing;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.AppSettings["DontStartStop"]?.ToLower() != "true")
            {
                WebBrowser.LoadingAdornerControl.StartStopWait(WebBrowser);
            }
            _controller.WebBrowserExtension.Navigate(_model.UrlPrefix + _model.IndexPage);
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
            var targetUrl = _model.IdentityServerSite + "account/logout";
            documentWaiter?.WaitForDocumentReady(targetUrl);
            var javascriptExecutor = _controller.WebBrowserExtension as IJavascriptExecutor;
            javascriptExecutor?.ExecuteJavascript(_model.LogoutJavascript);
        }

        private void WebBrowser_LoadCompleted(object sender, EventArgs eventArgs)
        {
            bool isIdentityServer;
            var url = _controller.WebBrowserExtension.GetCurrentUrl();

            _statusBar.Text =
                _controller.HandleStatusAndGetUrl(out isIdentityServer, url);

            HandleScripts(isIdentityServer, url);
            if (url.ToLower().StartsWith(_model.IndexUrl.ToLower()))
            {
                _controller.WebBrowserExtension?
                    .ExecuteJavascript(_model.StartupJavascript);
            }
            if (url.ToLower().StartsWith(_model.CallBackUrl.ToLower()))
            {
                var documentWaiter = _controller.WebBrowserExtension as IDocumentWaiter;
                documentWaiter?.WaitForDocumentReady(_model.IndexUrl);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
    }
}