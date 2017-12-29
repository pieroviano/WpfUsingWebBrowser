using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsingWebBrowserLib.Controllers.Logic;
using UsingWebBrowserLib.Model;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Interfaces;

namespace UsingWebBrowserLib.Controllers
{
    public class MainWindowController<THtmlElementType>
    {
        private readonly MainWindowModel _model;


        public MainWindowController(MainWindowModel model,
            IWebBrowserExtensionWithEventBase<THtmlElementType> instance)
        {
            _model = model;
            instance.Enabled = _model.WebBrowserExtensionEnabled;
            WebBrowserExtension = instance;
        }

        public IWebBrowserExtensionWithEventBase<THtmlElementType> WebBrowserExtension { get; }


        public async Task<Tuple<bool, bool>> DoCallApi(string url, Delegate messageBoxShow)
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
                return new Tuple<bool, bool>(false, false);
            }
            var accessToken = newSlotData[_model.AccessToken];
            await IdentityServerLogic.CallApi(accessToken, messageBoxShow);
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

        public string HandleStatusAndGetUrl(out bool isIdentityServer, string url)
        {
            isIdentityServer = false;
            string returnValue = null;
            if (!MainWindowModel.IsIdentityServerUrl(url))
            {
                WebBrowserExtension.AddJQueryElement();
                returnValue = "";
            }
            else
            {
                isIdentityServer = true;
                if (url != null)
                {
                    returnValue = url.Substring(0, url.LastIndexOf('?'));
                    var documentWaiter = WebBrowserExtension as IDocumentWaiter;
                    var targetUrl = MainWindowModel.IdentityServerSite + "consent";
                    var otherTargetUrl = MainWindowModel.IdentityServerSite + "account/logout";
                    if (url.ToLower().StartsWith(otherTargetUrl.ToLower()))
                    {
                        var modelIndexPage = MainWindowModel.IndexUrl;
                        documentWaiter?.WaitForDocumentReady(modelIndexPage);
                    }
                    else if (!url.ToLower().StartsWith(targetUrl.ToLower()))
                    {
                        documentWaiter?.WaitForDocumentReady(targetUrl);
                    }
                }
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