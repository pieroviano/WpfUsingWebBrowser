﻿using System;
using System.Configuration;
using System.Diagnostics;
using UsingWebBrowserLib.Controllers;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.CefSharp.WebBrowserControl;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;
using WpfAdornedControl.WpfControls.Extensions;

namespace UsingCefSharpFromWpf
{
    public partial class MainWindowWpf
    {
        private readonly MainWindowController<object> _controller;
        private readonly MainWindowModel _model;

        private bool _alreadyEntered;

        private void AttachEventHandlerToControl()
        {
            var comVisibleClass = new ComVisibleClass();
            comVisibleClass.HitBreakpoint = false;
            var javascriptExecutor = _controller.WebBrowserExtension as IJavascriptExecutor;
            comVisibleClass.EventFromComVisibleClass += (sender, args) =>
            {
                IdentityServerLogic.SetAuthorization(null);
                javascriptExecutor?.ExecuteJavascript(_model.LogoutJavascript);
            };
            var registerCsCodeCallableFromJavascript =
                ((_controller.WebBrowserExtension as IJavascriptExecutor) as WebBrowserExtensionCefSharp)?
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
            _controller.GetAuthenticationDictionary(WebBrowser.Address, out hasToLogin, out hasToNavigate);
            LoginOrNavigateIfNecessary(hasToLogin, hasToNavigate);
        }

        private string GetCurrentUrl()
        {
            var url = WebBrowser.Address;
            return url;
        }

        private void HandleScripts(bool isIdentityServer, string url)
        {
            var javascriptExecutor = _controller.WebBrowserExtension as IJavascriptExecutor;
            if (!isIdentityServer)
            {
#if DEBUG
                var customComVisibleClass = new CustomClassWpf(WebBrowser);
                customComVisibleClass.RaisedEvent += CustomComVisibleClass_RaisedEvent;
                Func<bool> customEventDelegate = customComVisibleClass.CodeToExecute;
                var functionHash = customEventDelegate.GetFullNameHashCode();
                var webBrowserExtensionCefSharp = _controller.WebBrowserExtension as WebBrowserExtensionCefSharp;
                webBrowserExtensionCefSharp?.AttachCustomFunctionOnDocument(
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
                    if (ConfigurationManager.AppSettings["DontStartStop"]?.ToLower() != "true")
                    {
                        LoadingAdorner.StartStopWait(WebBrowser);
                    }
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
                _controller.WebBrowserExtension?.ExecuteJavascript(_model.LoginJavascript);
            }
            else if (hasToNavigate)
            {
                _controller.WebBrowserExtension.Navigate(_model.UrlPrefix + _model.IndexPage);
            }
        }
    }
}