using System;
using System.Diagnostics;
using System.Windows.Controls;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Extensions;
using WebBrowserLib.WebBrowserControl;
using WebBrowserLib.Wpf.WebBrowserControl;
using WpfAdornedControl.WpfControls.Extensions;

namespace WpfUsingSelenium
{
    public partial class MainWindow
    {
        private readonly MainWindowController<WebBrowser, object, IHTMLElement> _controller;
        private readonly MainWindowModel _model;

        private bool _alreadyEntered;

        private void AttachEventHandlerToControl(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
        }

        private void GetAuthenticationDictionary()
        {
            bool hasToLogin;
            bool hasToNavigate;
            _controller.GetAuthenticationDictionary(WebBrowser.Source.ToString(), out hasToLogin, out hasToNavigate);
            LoginOrNavigateIfNecessary(hasToLogin, hasToNavigate);
        }

        private string GetCurrentUrl()
        {
            var url = WebBrowser.Source.ToString();
            return url;
        }

        private void HandleScripts(bool isIdentityServer, string url)
        {
            if (!isIdentityServer)
            {
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
                    WebBrowserExtensionWpf.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnSelectStart);
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
                    WebBrowserExtensionWpf.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnContextMenu);
                }
                bool isIndexPage;
                _controller.ProcessIndexOrCallbackFromidentityServer(url,
                    _model.GetCustomEventHandler, _model.SetCustomEventHandler,
                    out isIndexPage);
                if (isIndexPage)
                {
                    WebBrowserExtensionWpf.Instance.InjectAndExecuteJavascript(WebBrowser, "$(function(){$('#login').hide();$('#logout').hide();})");
                    AttachEventHandlerToControl(_model.GetCustomEventHandler, _model.SetCustomEventHandler);
                    GetAuthenticationDictionary();
                }
            }
            else
            {
                if (!_alreadyEntered)
                {
                    LoadingAdornerxtension.StartStopWait(LoadingAdorner, WebBrowser);
                    _alreadyEntered = true;
                }
                WebBrowserExtensionWpf.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnSelectStart);
                WebBrowserExtensionWpf.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnContextMenu);
            }
        }

        private void LoginOrNavigateIfNecessary(bool hasToLogin, bool hasToNavigate)
        {
            if (hasToLogin)
            {
                WebBrowserExtensionWpf.Instance.InjectAndExecuteJavascript(WebBrowser,_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                WebBrowser.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
            }
        }

    }
}