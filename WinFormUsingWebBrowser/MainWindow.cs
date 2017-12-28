using System;
using System.Windows.Forms;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Model;
using WebBrowserLib.WinForms.WebBrowserControl;

namespace UsingWebBrowserFromWinForm
{
    public partial class MainWindow
    {
        private readonly LoadingAdorner _loadingAdorner = new LoadingAdorner();

        public MainWindow()
        {
            InitializeComponent();
            _model = new MainWindowModel();
            var webBrowserExtensionWinForm = WebBrowserExtensionWinForm.GetInstance(WebBrowser);
            _controller =
                new MainWindowController<IHTMLElement>(_model, webBrowserExtensionWinForm);

            webBrowserExtensionWinForm.RemoveHandlersOnNavigating(_model.GetCustomEventHandler,
                _model.SetCustomEventHandler);
        }

        private async void callAPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tuple = await _controller.DoCallApi(WebBrowser.Url.ToString(),
                new Func<string, DialogResult>(MessageBox.Show));
            var hasToLogin = tuple.Item1;
            var hasToNavigate = tuple.Item2;
            LoginOrNavigateIfNecessary(hasToLogin, hasToNavigate);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Controls.Add(_loadingAdorner);
            _loadingAdorner.StartStopWait(WebBrowser);
            _controller.WebBrowserExtensionWithEvent.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            bool isIdentityServer;
            var url = GetCurrentUrl();

            StatusBar.Text =
                _controller.HandleStatusAndGetUrl(out isIdentityServer, url);

            HandleScripts(isIdentityServer, url);
        }
    }
}