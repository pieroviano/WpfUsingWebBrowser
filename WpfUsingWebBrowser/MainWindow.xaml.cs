using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ComInteropLib;
using WebBrowserLib.WebBrowserControl;
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
            WebBrowser.RemoveHandlersOnNavigating(_model.GetCustomEventHandler);
            WebBrowser.Navigating += WebBrowser_Navigating;
        }

        private async void MenuItemCallApi_Click(object sender, RoutedEventArgs e)
        {
            await _controller.DoCallApi(WebBrowser.Source.ToString(), Navigate, GetAuthenticationDictionary);
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            Func<bool> codeToExecuteDelegate = new ComVisibleClass(WebBrowser).CodeToExecute;
            WebBrowser.RunCsFromJavascript(codeToExecuteDelegate);
            MainWindowModel.CsBreakpoint = WebBrowser.RegisterCsBreakpoint();
#if DEBUG
            Func<bool> customEventDelegate = new ComVisibleClass(WebBrowser).CodeToExecute;
            var functionHash = (IntPtr)customEventDelegate.GetHashCode();
            WebBrowser.AttachFunctionOnClickPlusShift(customEventDelegate, functionHash, _model.GetCustomEventHandler, _model.SetCustomEventHandler);
#endif
            var url = WebBrowser.Source.ToString();
            _controller.DoWorkOnPage(url,
                _model.GetCustomEventHandler, _model.SetCustomEventHandler,
                AttachEventHandlerToControl, InjectAndExecuteJavascript, _model.RemoveSelectionJavascript,
                GetAuthenticationDictionary, DisableOnContextMenuToDocument);
            var substring = new Uri(MainWindowModel.IdentityServerUrl);
            var indexOf = substring.AbsoluteUri.IndexOf(substring.LocalPath, StringComparison.Ordinal);
            var value = substring.AbsoluteUri.Substring(0, indexOf + 1);
            if (!url.Contains(value))
            {
                StatusBar.Text = "";
            }
            else
            {
                StatusBar.Text = url.Substring(0, url.LastIndexOf('?'));
            }
        }

        private void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_model.GetCustomEventHandler() != null)
                _model.SetCustomEventHandler(null);
            if (_model.GetCustomEventHandler() != null)
                _model.SetCustomEventHandler(null);
        }
    }
}