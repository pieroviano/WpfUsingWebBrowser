using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using mshtml;
using WebBrowserLib.WebBrowserControl;
using WpfAdornedControl.WpfControls.Extensions;
using WpfUsingWebBrowser.Controllers;
using WpfUsingWebBrowser.Model;

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
            _controller = new MainWindowController(_model);

            WebBrowser.RemoveHandlersOnNavigating(_model.GetCustomEventHandler, _model.SetCustomEventHandler);
        }

        private async void MenuItemCallApi_Click(object sender, RoutedEventArgs e)
        {
            var tuple = await _controller.DoCallApi(WebBrowser.Source.ToString());
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
                _controller.HandleStatusAndGetUrl(WebBrowser.Document as HTMLDocument, out isIdentityServer, url);

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