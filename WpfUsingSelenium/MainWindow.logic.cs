using OpenQA.Selenium;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Model;
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
            var url = WebBrowser.WebDriver.Url;
            return url;
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
                        .InjectAndExecuteJavascript("$(function(){$('#login').hide();$('#logout').hide();})");
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