using System;
using System.Diagnostics;
using mshtml;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;
using WebBrowserLib.Wpf.WebBrowserControl;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingWebBrowserFromWpf
{
    public partial class MainWindowWpf
    {
        private readonly MainWindowController<IHTMLElement> _controller;
        private readonly MainWindowModel _model;

        private bool _alreadyEntered;

        private void AttachEventHandlerToControl()
        {
            var comVisibleClass = new ComVisibleClass();
            comVisibleClass.HitBreakpoint = false;
            var javascriptExecutor = ((_controller.WebBrowserExtension as IJavascriptExecutor) as IJavascriptExecutor);
            comVisibleClass.EventFromComVisibleClass += (sender, args) =>
            {
                IdentityServerLogic.SetAuthorization(null);
                javascriptExecutor?.ExecuteJavascript(_model.LogoutJavascript);
            };
            var registerCsCodeCallableFromJavascript = ((_controller.WebBrowserExtension as IJavascriptExecutor) as WebBrowserExtensionWpf)?
                .RegisterCsCodeCallableFromJavascript(ref comVisibleClass);
            var javascriptToExecute = "document.all['logout'].onclick = function(){" +
                                      registerCsCodeCallableFromJavascript + "}";
            javascriptExecutor?.ExecuteJavascript(javascriptToExecute);
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
            var javascriptExecutor = (_controller.WebBrowserExtension as IJavascriptExecutor);
            if (!isIdentityServer)
            {
#if DEBUG
                var customComVisibleClass = new CustomComVisibleClassWpf(WebBrowser);
                customComVisibleClass.RaisedEvent += CustomComVisibleClass_RaisedEvent;
                Func<bool> customEventDelegate = customComVisibleClass.CodeToExecute;
                var functionHash = customEventDelegate.GetFullNameHashCode();
                var webBrowserExtensionWpf = _controller.WebBrowserExtension  as WebBrowserExtensionWpf;
                webBrowserExtensionWpf?.AttachCustomFunctionOnDocument(
                    "onclick",
                    customEventDelegate, functionHash,
                    _model.GetCustomEventHandler,
                    _model.SetCustomEventHandler);
#endif
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
                    javascriptExecutor?
                        .ExecuteJavascript(_model.IgnoreOnSelectStart);
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
                    javascriptExecutor?
                        .ExecuteJavascript(_model.IgnoreOnContextMenu);
                }
                bool isIndexPage;
                _controller.ProcessIndexOrCallbackFromidentityServer(url,
                    _model.GetCustomEventHandler, _model.SetCustomEventHandler,
                    out isIndexPage);
                if (isIndexPage)
                {
                    javascriptExecutor?
                        .ExecuteJavascript("$(function(){$('#login').hide();})");
                    AttachEventHandlerToControl();
                    GetAuthenticationDictionary();
                }
            }
            else
            {
                if (!_alreadyEntered)
                {
                    LoadingAdornerExtension.StartStopWait(LoadingAdorner,WebBrowser);
                    _alreadyEntered = true;
                }
                javascriptExecutor.ExecuteJavascript(_model.IgnoreOnSelectStart);
                javascriptExecutor.ExecuteJavascript(_model.IgnoreOnContextMenu);
            }
        }

        private void LoginOrNavigateIfNecessary(bool hasToLogin, bool hasToNavigate)
        {
            if (hasToLogin)
            {
                (_controller.WebBrowserExtension as IJavascriptExecutor)?.ExecuteJavascript(_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                _controller.WebBrowserExtension.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
            }
        }
    }
}