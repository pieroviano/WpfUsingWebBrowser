using System.Configuration;
using OpenQA.Selenium;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Interfaces;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingSeleniumFromWpf
{
    public partial class MainWindow
    {
        private readonly MainWindowController<IWebElement> _controller;
        private readonly MainWindowModel _model;

        private bool _alreadyEntered;

        private void HandleScripts(bool isIdentityServer, string url)
        {
            var browserExtensionJavascript = _controller.WebBrowserExtension as IWebBrowserExtensionJavascript;
            if (!isIdentityServer)
            {
                var javascriptExecutor = _controller.WebBrowserExtension as IJavascriptExecutor;
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
                    javascriptExecutor?
                        .ExecuteJavascript(_model.IgnoreOnSelectStart);
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
                    var webBrowserExtensionJavascript = browserExtensionJavascript;
                    webBrowserExtensionJavascript?.DisableOnContextMenuOnDocument();
                }
                bool isIndexPage;
                _controller.ProcessIndexOrCallbackFromidentityServer(url,
                    _model.GetCustomEventHandler, _model.SetCustomEventHandler,
                    out isIndexPage);
                if (isIndexPage)
                {
                    if (!_alreadyEntered)
                    {
                        javascriptExecutor?
                            .ExecuteJavascript("login();");
                    }
                    var documentWaiter = _controller.WebBrowserExtension as IDocumentWaiter;
                    documentWaiter?.WaitForDocumentReady(_model.IdentityServerUrl);
                }
            }
            else
            {
                if (!_alreadyEntered)
                {
                    if (ConfigurationManager.AppSettings["DontStartStop"]?.ToLower() != "true")
                    {
                        WebBrowser.LoadingAdornerControl.StartStopWait(WebBrowser);
                    }
                    _alreadyEntered = true;
                }
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
                    browserExtensionJavascript?.ExecuteJavascript(_model.IgnoreOnSelectStart);
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
                    browserExtensionJavascript?.DisableOnContextMenuOnDocument();
                }
                var documentWaiter = _controller.WebBrowserExtension as IDocumentWaiter;
                documentWaiter?.WaitForDocumentReady(_model.CallBackUrl);
            }
        }

        private void LoginOrNavigateIfNecessary(bool hasToLogin, bool hasToNavigate)
        {
            if (hasToLogin)
            {
                _controller.WebBrowserExtension?.ExecuteJavascript(_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                _controller.WebBrowserExtension.Navigate(_model.UrlPrefix + _model.IndexPage);
            }
        }
    }
}