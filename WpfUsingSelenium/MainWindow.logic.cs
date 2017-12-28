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

        private string GetCurrentUrl()
        {
            var url = _controller.WebBrowserExtensionWithEvent.GetGlobalVariable("document.location");
            if (url == null)
                return null;
            return url["href"];
        }

        private void HandleScripts(bool isIdentityServer, string url)
        {
            if (!isIdentityServer)
            {
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
                    _controller.WebBrowserExtensionWithEvent
                        .InjectAndExecuteJavascript(_model.IgnoreOnSelectStart);
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
                    _controller.WebBrowserExtensionWithEvent
                        .InjectAndExecuteJavascript(_model.IgnoreOnContextMenu);
                }
                bool isIndexPage;
                _controller.ProcessIndexOrCallbackFromidentityServer(url,
                    _model.GetCustomEventHandler, _model.SetCustomEventHandler,
                    out isIndexPage);
                if (isIndexPage)
                {
                    _controller.WebBrowserExtensionWithEvent
                        .InjectAndExecuteJavascript("login();");
                    var documentWaiter = _controller.WebBrowserExtensionWithEvent as IDocumentWaiter;
                    documentWaiter?.WaitForDocumentReady(MainWindowModel.IdentityServerUrl);
                }
            }
            else
            {
                if (!_alreadyEntered)
                {
                    WebBrowser.LoadingAdornerControl.StartStopWait(WebBrowser);
                    _alreadyEntered = true;
                }
                _controller.WebBrowserExtensionWithEvent.InjectAndExecuteJavascript(_model.IgnoreOnSelectStart);
                _controller.WebBrowserExtensionWithEvent.InjectAndExecuteJavascript(_model.IgnoreOnContextMenu);
                var documentWaiter = _controller.WebBrowserExtensionWithEvent as IDocumentWaiter;
                documentWaiter?.WaitForDocumentReady(MainWindowModel.CallBackUrl);
            }
        }

        private void LoginOrNavigateIfNecessary(bool hasToLogin, bool hasToNavigate)
        {
            if (hasToLogin)
            {
                _controller.WebBrowserExtensionWithEvent.InjectAndExecuteJavascript(_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                _controller.WebBrowserExtensionWithEvent.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
            }
        }
    }
}