using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WebBrowserLib.WebBrowserControl;
using WpfUsingWebBrowser.Controllers;
using WpfUsingWebBrowser.Controllers.Logic;
using WpfUsingWebBrowser.Model;

namespace WpfUsingWebBrowser
{
    public partial class MainWindow
    {
        private readonly MainWindowController _controller;
        private readonly MainWindowModel _model;

        private void AttachEventHandlerToControl(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler, Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomOnClickFunction;
            var functionHash = (IntPtr)customEventDelegate.GetHashCode();
            WebBrowser.AttachEventHandlerToControl("logout", "onclick", customEventDelegate, functionHash, getCustomEventHandler, setCustomEventHandler, true);
        }

        private bool CustomOnClickFunction()
        {
            IdentityServerLogic.SetAuthorization(null);
            WebBrowser.InjectAndExecuteJavascript(_model.LogoutJavascript);
            return false;
        }

        private void DisableOnContextMenuToDocument(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler, Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            WebBrowser.DisableOnContextMenuToDocument(getCustomEventHandler, setCustomEventHandler);
        }

        private void EnableOnContextMenuToDocument()
        {
            WebBrowser.EnableOnContextMenuToDocument(_model.GetCustomEventHandler, _model.SetCustomEventHandler);
        }

        private Dictionary<string, string> GetAuthenticationDictionary()
        {
            return _controller.GetAuthenticationDictionary(WebBrowser.Source.ToString(), Navigate,
                InjectAndExecuteJavascript,
                _model.LoginJavascript);
        }

        private void InjectAndExecuteJavascript(string javascript)
        {
            WebBrowser.InjectAndExecuteJavascript(javascript);
        }

        private void Navigate(string url)
        {
            WebBrowser.Navigate(url);
        }
    }
}