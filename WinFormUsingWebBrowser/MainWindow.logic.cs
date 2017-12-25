using System;
using System.Diagnostics;
using System.Windows.Forms;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Extensions;
using WebBrowserLib.WebBrowserControl;
using WebBrowserLib.WebBrowserControl.Helpers;
using WebBrowserLib.WinForms.WebBrowserControl;

namespace WinFormUsingWebBrowser
{
    public partial class MainWindow
    {
        private readonly MainWindowController<WebBrowser, object, IHTMLElement> _controller;
        private readonly MainWindowModel _model;

        private bool _alreadyEntered;

        private void AttachEventHandlerToControl(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var comVisibleClass = new ComVisibleClass();
            comVisibleClass.HitBreakpoint = false;
            comVisibleClass.EventFromComVisibleClass += (sender, args) =>
            {
                IdentityServerLogic.SetAuthorization(null);
                WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,_model.LogoutJavascript);
            };
            var registerCsCodeCallableFromJavascript = WebBrowserExtensionWinForm.Instance.RegisterCsCodeCallableFromJavascript(WebBrowser,ref comVisibleClass);
            var javascriptToExecute = "document.all['logout'].onclick = function(){" +
                                      registerCsCodeCallableFromJavascript + "}";
            WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,javascriptToExecute);
        }

        private void GetAuthenticationDictionary()
        {
            bool hasToLogin;
            bool hasToNavigate;
            _controller.GetAuthenticationDictionary(WebBrowser.Url.ToString(), out hasToLogin, out hasToNavigate);
            LoginOrNavigateIfNecessary(hasToLogin, hasToNavigate);
        }

        private string GetCurrentUrl()
        {
            var url = WebBrowser.Url.ToString();
            return url;
        }

        private void HandleScripts(bool isIdentityServer, string url)
        {
            if (!isIdentityServer)
            {
#if DEBUG
                var customComVisibleClass = new CustomComVisibleClassWinForm(WebBrowser);
                customComVisibleClass.RaisedEvent += CustomComVisibleClass_RaisedEvent;
                Func<bool> customEventDelegate = customComVisibleClass.CodeToExecute;
                var functionHash = customEventDelegate.GetFullNameHashCode();
                WebBrowserExtensionWinForm.Instance.AttachCustomFunctionOnDocument(WebBrowser, "onclick",customEventDelegate, functionHash,
                    _model.GetCustomEventHandler,
                    _model.SetCustomEventHandler);
#endif
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
#if !DONTUSEJAVASCRIPT
                    WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnSelectStart);
#else
                    WebBrowser.DisableEventOnDocument("onselectstart", _model.GetCustomEventHandler,
                        _model.SetCustomEventHandler);
#endif
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
#if !DONTUSEJAVASCRIPT
                    WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnContextMenu);
#else
                    WebBrowser.DisableOnContextMenuOnDocument(_model.GetCustomEventHandler,
                        _model.SetCustomEventHandler);
#endif
                }
                bool isIndexPage;
                _controller.ProcessIndexOrCallbackFromidentityServer(url,
                    _model.GetCustomEventHandler, _model.SetCustomEventHandler,
                    out isIndexPage);
                if (isIndexPage)
                {
                    WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,"$(function(){$('#login').hide();})");
                    AttachEventHandlerToControl(_model.GetCustomEventHandler, _model.SetCustomEventHandler);
                    GetAuthenticationDictionary();
                }
            }
            else
            {
                if (!_alreadyEntered)
                {
                    _loadingAdorner.StartStopWait(WebBrowser);
                    _alreadyEntered = true;
                }
                WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnSelectStart);
                WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,_model.IgnoreOnContextMenu);
            }
        }

        private void CustomComVisibleClass_RaisedEvent(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private void LoginOrNavigateIfNecessary(bool hasToLogin, bool hasToNavigate)
        {
            if (hasToLogin)
            {
                WebBrowserExtensionWinForm.Instance.InjectAndExecuteJavascript(WebBrowser,_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                WebBrowser.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
            }
        }

    }
}