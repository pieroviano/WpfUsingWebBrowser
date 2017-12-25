using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Wpf.WebBrowserControl;
using WpfAdornedControl.WpfControls.Extensions;

namespace WpfUsingWebBrowser
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            _model = new MainWindowModel();
            _controller =
                new MainWindowController<WebBrowser, object, IHTMLElement>(_model, WebBrowserExtensionWpf.Instance);

            WebBrowserExtensionWpf.Instance.RemoveHandlersOnNavigating(WebBrowser, _model.GetCustomEventHandler,
                _model.SetCustomEventHandler);
        }

        private async void MenuItemCallApi_Click(object sender, RoutedEventArgs e)
        {
            var tuple = await _controller.DoCallApi(WebBrowser.Source.ToString(),
                new Func<string, MessageBoxResult>(MessageBox.Show));
            var hasToLogin = tuple.Item1;
            var hasToNavigate = tuple.Item2;
            LoginOrNavigateIfNecessary(hasToLogin, hasToNavigate);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            bool isIdentityServer;
            var url = GetCurrentUrl();
            var item = (WebBrowser.Document as HTMLDocument)?.getElementsByTagName("head").item(0) as HTMLHeadElement;

            StatusBar.Text =
                _controller.HandleStatusAndGetUrl(item, out isIdentityServer, url);

            HandleScripts(isIdentityServer, url);
        }

        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingAdorner.StartStopWait(WebBrowser);
            WebBrowser.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}