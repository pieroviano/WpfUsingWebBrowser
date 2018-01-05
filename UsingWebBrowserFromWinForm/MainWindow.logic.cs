using System;
using System.Configuration;
using System.Diagnostics;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.WinForms.WebBrowserControl;

namespace UsingWebBrowserFromWinForm
{
    public partial class MainWindow
    {
        private readonly MainWindowController<IHTMLElement> _controller;
        private readonly MainWindowModel _model;

        private bool _alreadyEntered;

        private void AttachEventHandlerToControl()
        {
            var comVisibleClass = new ComVisibleClass();
            comVisibleClass.HitBreakpoint = false;
            comVisibleClass.EventFromComVisibleClass += (sender, args) =>
            {
                IdentityServerLogic.SetAuthorization(null);
                WebBrowserExtensionWinForm.GetInstance(WebBrowser).ExecuteJavascript(_model.LogoutJavascript);
            };
            var registerCsCodeCallableFromJavascript = WebBrowserExtensionWinForm.GetInstance(WebBrowser)
                .RegisterCsCodeCallableFromJavascript(ref comVisibleClass);
            var javascriptToExecute = "document.all['logout'].onclick = function(){" +
                                      registerCsCodeCallableFromJavascript + "}";
            WebBrowserExtensionWinForm.GetInstance(WebBrowser).ExecuteJavascript(javascriptToExecute);
        }

        private void CustomComVisibleClass_RaisedEvent(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
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
                var customComVisibleClass = new CustomClassWinForm(WebBrowser);
                customComVisibleClass.RaisedEvent += CustomComVisibleClass_RaisedEvent;
                Func<bool> customEventDelegate = customComVisibleClass.CodeToExecute;
                var functionHash = customEventDelegate.GetFullNameHashCode();
                WebBrowserExtensionWinForm.GetInstance(WebBrowser).AttachCustomFunctionOnDocument(WebBrowser, "onclick",
                    customEventDelegate, functionHash,
                    _model.GetCustomEventHandler,
                    _model.SetCustomEventHandler);
#endif
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
                    WebBrowserExtensionWinForm.GetInstance(WebBrowser)
                        .ExecuteJavascript(_model.IgnoreOnSelectStart);
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
                    WebBrowserExtensionWinForm.GetInstance(WebBrowser)
                        .ExecuteJavascript(_model.IgnoreOnContextMenu);
                }
                bool isIndexPage;
                _controller.ProcessIndexOrCallbackFromidentityServer(url,
                    _model.GetCustomEventHandler, _model.SetCustomEventHandler,
                    out isIndexPage);
                if (isIndexPage)
                {
                    WebBrowserExtensionWinForm.GetInstance(WebBrowser)
                        .ExecuteJavascript("$(function(){$('#login').hide();})");
                    AttachEventHandlerToControl();
                    GetAuthenticationDictionary();
                }
            }
            else
            {
                if (!_alreadyEntered)
                {
                    if (ConfigurationManager.AppSettings["DontStartStop"]?.ToLower() != "true")
                    {
                        _loadingAdorner.StartStopWait(WebBrowser);
                    }
                    _alreadyEntered = true;
                }
                WebBrowserExtensionWinForm.GetInstance(WebBrowser)
                    .ExecuteJavascript(_model.IgnoreOnSelectStart);
                WebBrowserExtensionWinForm.GetInstance(WebBrowser)
                    .ExecuteJavascript(_model.IgnoreOnContextMenu);
            }
        }

        private void LoginOrNavigateIfNecessary(bool hasToLogin, bool hasToNavigate)
        {
            if (hasToLogin)
            {
                WebBrowserExtensionWinForm.GetInstance(WebBrowser).ExecuteJavascript(_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                _controller.WebBrowserExtension.Navigate(_model.UrlPrefix + _model.IndexPage);
            }
        }
    }
}