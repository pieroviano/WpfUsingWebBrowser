using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBrowserLib.WebBrowserControl;
using WpfUsingWebBrowser.Controllers.Logic;
using WpfUsingWebBrowser.Model;

namespace WpfUsingWebBrowser.Controllers
{
    public class MainWindowController
    {
        private readonly MainWindowModel _model;


        public MainWindowController(MainWindowModel model)
        {
            _model = model;
            WebBrowserExtension.Enabled = _model.WebBrowserExtensionEnabled;
            WebBrowserExtension.JavascriptInjectionEnabled = _model.WebBrowserExtensionJavascriptInjectionEnabled;
        }

        public async Task DoCallApi(string url, Action<string> navigate,
            Func<Dictionary<string, string>> getAuthenticationDictionary)
        {
            var newSlotData = getAuthenticationDictionary();
            if (newSlotData == null)
                return;
            var lower = url.ToLower();
            if (!lower.Contains(_model.IndexPage))
            {
                navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
                return;
            }
            var accessToken = newSlotData[_model.AccessToken];
            await IdentityServerLogic.CallApi(accessToken);
        }

        public void DoWorkOnPage(string url,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler,
            Action<Func<CustomWebBrowserControlEventHandler>, Action<CustomWebBrowserControlEventHandler>> attachEventHandlerToControl,
            Action<string> injectAndExecuteJavascript, string javascript,
            Func<Dictionary<string, string>> getAuthenticationDictionary,
            Action<Func<CustomWebBrowserControlEventHandler>, Action<CustomWebBrowserControlEventHandler>> disableOnContextMenuToDocument)
        {
            if (_model.DisableOnContextMenuToDocument)
                disableOnContextMenuToDocument(getCustomEventHandler, setCustomEventHandler);
            injectAndExecuteJavascript(javascript);
            var lower = url.ToLower();
            if (lower.Contains(_model.IndexPage))
            {
                attachEventHandlerToControl(getCustomEventHandler, setCustomEventHandler);
                getAuthenticationDictionary();
            }
            else
            {
                if (lower.Contains(_model.CallbackPage) && !lower.Contains(_model.RedirectUri))
                {
                    lower = url.Substring(lower.IndexOf('#') + 1);
                    var dict = lower.Split('&').Select(s =>
                        {
                            var strings = s.Split('=');
                            return new { Key = strings[0], Value = strings[1] };
                        })
                        .ToDictionary(t => t.Key, t => t.Value);
                    IdentityServerLogic.SetAuthorization(dict);
                }
            }
        }

        public Dictionary<string, string> GetAuthenticationDictionary(string url, Action<string> navigate,
            Action<string> injectAndExecuteJavascript, string javascript)
        {
            var newSlotData = IdentityServerLogic.GetAuthorization();
            if (newSlotData == null)
            {
                var lower = url.ToLower();
                if (!lower.Contains(_model.IndexPage))
                {
                    navigate(MainWindowModel.UrlPrefix + _model.IndexPage);
                    return null;
                }
                injectAndExecuteJavascript(
                    javascript);
            }
            return newSlotData;
        }
    }
}