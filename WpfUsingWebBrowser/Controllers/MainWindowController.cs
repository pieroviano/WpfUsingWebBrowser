using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mshtml;
using WebBrowserLib.WebBrowserControl;
using WebBrowserLib.WebBrowserControl.Helpers;
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
            WebBrowserExtension.Instance.Enabled = _model.WebBrowserExtensionEnabled;
            WebBrowserExtension.Instance.JavascriptInjectionEnabled = _model.WebBrowserExtensionJavascriptInjectionEnabled;
        }


        public async Task<Tuple<bool, bool>> DoCallApi(string url)
        {
            bool hasToLogin;
            bool hasToNavigate;
            var newSlotData = GetAuthenticationDictionary(url, out hasToLogin, out hasToNavigate);
            if (hasToNavigate || hasToLogin)
            {
                return new Tuple<bool, bool>(hasToLogin, hasToNavigate);
            }
            if (newSlotData == null)
            {
                return new Tuple<bool, bool>(false, false);
            }
            var lower = url.ToLower();
            if (!lower.Contains(_model.IndexPage))
            {
                hasToNavigate = true;
                return new Tuple<bool, bool>(false, false);
            }
            var accessToken = newSlotData[_model.AccessToken];
            await IdentityServerLogic.CallApi(accessToken);
            return new Tuple<bool, bool>(false, false);
        }

        public Dictionary<string, string> GetAuthenticationDictionary(string url, out bool hasToLogin,
            out bool hasToNavigate)
        {
            hasToNavigate = false;
            var newSlotData = IdentityServerLogic.GetAuthorization();
            if (newSlotData == null)
            {
                var lower = url.ToLower();
                if (!lower.Contains(_model.IndexPage))
                {
                    hasToNavigate = true;
                    hasToLogin = false;
                    return null;
                }
                hasToLogin = true;
                return null;
            }
            hasToLogin = false;
            return newSlotData;
        }

        public string HandleStatusAndGetUrl(HTMLDocument document, out bool isIdentityServer, string url)
        {
            isIdentityServer = false;
            string returnValue;
            if (!MainWindowModel.IsIdentityServerUrl(url))
            {
                var item = document?.getElementsByTagName("head").item(0) as HTMLHeadElement;
                WebBrowserExtension.Instance.AddJQueryElement(item);
                returnValue = "";
            }
            else
            {
                isIdentityServer = true;
                returnValue = url.Substring(0, url.LastIndexOf('?'));
            }
            return returnValue;
        }

        public void ProcessIndexOrCallbackFromidentityServer(string url,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler,
            out bool isIndexPage)
        {
            var lower = url.ToLower();
            isIndexPage = lower.Contains(_model.IndexPage);
            var isCallbackFromIdentityServer =
                lower.Contains(_model.CallbackPage) && !lower.Contains(_model.RedirectUri);
            if (isIndexPage)
            {
                return;
            }
            if (isCallbackFromIdentityServer)
            {
                lower = url.Substring(lower.IndexOf('#') + 1);
                var dict = lower.Split('&').Select(s =>
                    {
                        var strings = s.Split('=');
                        return new {Key = strings[0], Value = strings[1]};
                    })
                    .ToDictionary(t => t.Key, t => t.Value);
                IdentityServerLogic.SetAuthorization(dict);
            }
        }
    }
}