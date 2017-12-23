using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ComInteropLib;
using mshtml;
using WebBrowserLib.Extensions;
using WebBrowserLib.WebBrowserControl;
using WebBrowserLib.WebBrowserControl.Helpers;
using WpfAdornedControl.WpfControls.Extensions;
using WpfUsingWebBrowser.Controllers;
using WpfUsingWebBrowser.Controllers.Logic;
using WpfUsingWebBrowser.Model;

namespace WpfUsingWebBrowser
{
    public partial class MainWindow
    {
        private readonly MainWindowController _controller;
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
                WebBrowser.InjectAndExecuteJavascript(_model.LogoutJavascript);
            };
            var registerCsCodeCallableFromJavascript = WebBrowser.RegisterCsCodeCallableFromJavascript(ref comVisibleClass);
            var javascriptToExecute = "document.all['logout'].onclick = function(){" +
                                      registerCsCodeCallableFromJavascript + "}";
            WebBrowser.InjectAndExecuteJavascript(javascriptToExecute);
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
#if DEBUG
                var customComVisibleClass = new CustomComVisibleClass(WebBrowser);
                customComVisibleClass.RaisedEvent += CustomComVisibleClass_RaisedEvent;
                Func<bool> customEventDelegate = customComVisibleClass.CodeToExecute;
                var functionHash = customEventDelegate.GetFullNameHashCode();
                WebBrowser.AttachCustomFunctionOnDocument("onclick",customEventDelegate, functionHash,
                    _model.GetCustomEventHandler,
                    _model.SetCustomEventHandler);
#endif
                if (!_model.DontDisableOnSelectionStartToDocument)
                {
#if !DONTUSEJAVASCRIPT
                    WebBrowser.InjectAndExecuteJavascript(_model.IgnoreOnSelectStart);
#else
                    WebBrowser.DisableEventOnDocument("onselectstart", _model.GetCustomEventHandler,
                        _model.SetCustomEventHandler);
#endif
                }
                if (!_model.DontDisableOnContextMenuToDocument)
                {
#if !DONTUSEJAVASCRIPT
                    WebBrowser.InjectAndExecuteJavascript(_model.IgnoreOnContextMenu);
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
                    WebBrowser.InjectAndExecuteJavascript("$(function(){$('#login').hide();})");
                    AttachEventHandlerToControl(_model.GetCustomEventHandler, _model.SetCustomEventHandler);
                    GetAuthenticationDictionary();
                }
            }
            else
            {
                if (!_alreadyEntered)
                {
                    LoadingAdorner.StartStopWait(WebBrowser);
                    _alreadyEntered = true;
                }
                WebBrowser.InjectAndExecuteJavascript(_model.IgnoreOnSelectStart);
                WebBrowser.InjectAndExecuteJavascript(_model.IgnoreOnContextMenu);
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
                WebBrowser.InjectAndExecuteJavascript(_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                WebBrowser.Navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
            }
        }

    }
}