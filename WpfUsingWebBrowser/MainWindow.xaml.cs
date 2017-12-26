using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Wpf.WebBrowserControl;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingWebBrowserFromWpf
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
                new MainWindowController<IHTMLElement>(_model, WebBrowserExtensionWpf.GetInstance(WebBrowser));

            _controller.WebBrowserExtensionWithEvent.RemoveHandlersOnNavigating(_model.GetCustomEventHandler,
                _model.SetCustomEventHandler);

            WebBrowser.LoadCompleted += WebBrowser_LoadCompleted;
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
            LoadingAdorner.StartStopWait(WebBrowser);
            WebBrowser.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
        }
    }
}