using System;
using System.Windows.Forms;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Model;
using WebBrowserLib.WinForms.WebBrowserControl;

namespace WinFormUsingWebBrowser
{
    public partial class MainWindow : Form
    {
        private readonly LoadingAdorner _loadingAdorner = new LoadingAdorner();

        public MainWindow()
        {
            InitializeComponent();
            _model = new MainWindowModel();
            _controller =
                new MainWindowController<WebBrowser, object, IHTMLElement>(_model, WebBrowserExtensionWinForm.Instance);

            WebBrowserExtensionWinForm.Instance.RemoveHandlersOnNavigating(WebBrowser, _model.GetCustomEventHandler,
                _model.SetCustomEventHandler);
        }

        private void callAPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var doCallApi = _controller.DoCallApi(WebBrowser.Url.ToString(),
                new Func<string, DialogResult>(MessageBox.Show));
            doCallApi.Wait();
            var tuple = doCallApi.Result;
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
            WebBrowser.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            bool isIdentityServer;
            var url = GetCurrentUrl();
            var item =
                (WebBrowser.Document?.DomDocument as HTMLDocument)?.getElementsByTagName("head")
                .item(0) as HTMLHeadElement;

            StatusBar.Text =
                _controller.HandleStatusAndGetUrl(item, out isIdentityServer, url);

            HandleScripts(isIdentityServer, url);
        }
    }
}