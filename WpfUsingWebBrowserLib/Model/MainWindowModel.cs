using System;
using System.Configuration;
using WebBrowserLib.EventHandling;

namespace UsingWebBrowserLib.Model
{
    public class MainWindowModel
    {
        private CustomWebBrowserControlEventHandler _customWebBrowserControlEventHandler;

        private string _dontDisableOnContextMenuOnDocument =
            ConfigurationManager.AppSettings["DontDisableOnContextMenuOnDocument"];

        private string _dontDisableOnSelectionStartToDocument =
            ConfigurationManager.AppSettings["DontDisableOnSelectionStartToDocument"];

        private string _webBrowserExtensionEnabled =
            ConfigurationManager.AppSettings["WebBrowserExtensionEnabled"];

        private string _webBrowserExtensionJavascriptInjectionEnabled =
            ConfigurationManager.AppSettings["WebBrowserExtensionJavascriptInjectionEnabled"];

        private MainWindowModel()
        {
        }

        public static MainWindowModel Instance { get; } = new MainWindowModel();

        public int Port { get; set; } = 5003;

        public string StartupJavascript { get; set; }

        public string IndexPage { get; } = "index.html";

        public string CallbackPage { get; } = "callback.html";

        public string RedirectUri { get; } = "redirect_uri";

        public string AccessToken { get; } = "access_token";

        public string LogoutJavascript { get; } = "logout();";

        public string IgnoreOnSelectStart { get; } = "document.onselectstart=function(){return false;}";

        public string IgnoreOnContextMenu { get; } = "document.oncontextmenu=function(){return false;}";


        public bool DontDisableOnSelectionStartToDocument
        {
            get { return _dontDisableOnSelectionStartToDocument?.ToLower() == "true"; }
            set { _dontDisableOnSelectionStartToDocument = value.ToString(); }
        }

        public bool DontDisableOnContextMenuToDocument
        {
            get => _dontDisableOnContextMenuOnDocument?.ToLower() == "true";
            set => _dontDisableOnContextMenuOnDocument = value.ToString();
        }


        public bool WebBrowserExtensionEnabled
        {
            get => _webBrowserExtensionEnabled?.ToLower() == "true";
            set => _webBrowserExtensionEnabled = value.ToString();
        }


        public bool WebBrowserExtensionJavascriptInjectionEnabled
        {
            get => _webBrowserExtensionJavascriptInjectionEnabled?.ToLower() == "true";
            set => _webBrowserExtensionJavascriptInjectionEnabled = value.ToString();
        }


        public string LoginJavascript =>
            @"$(function () {" + StartupJavascript + "login();});";

        public string UrlPrefix => $"http://localhost:{Port}/";

        public string CallBackUrl => $"{UrlPrefix}{CallbackPage}";

        public string IndexUrl => $"{UrlPrefix}{IndexPage}";


        public string IdentityServerUrl { get; } = ConfigurationManager.AppSettings["IdentityServerUrl"];

        public string IdentityServerSite => IdentityServerUrl.Substring(0, IdentityServerUrl.IndexOf("/",
                                                                               IdentityServerUrl.LastIndexOf("//",
                                                                                   StringComparison.Ordinal) + 3,
                                                                               StringComparison.Ordinal) + 1);

        public string ApiServerUrl { get; } = ConfigurationManager.AppSettings["ApiServerUrl"];

        public CustomWebBrowserControlEventHandler GetCustomEventHandler()
        {
            return _customWebBrowserControlEventHandler;
        }

        public bool IsIdentityServerUrl(string url)
        {
            var substring = new Uri(IdentityServerUrl);
            var indexOf = substring.AbsoluteUri.IndexOf(substring.LocalPath, StringComparison.Ordinal);
            var value = substring.AbsoluteUri.Substring(0, indexOf + 1);
            var isIdentityServerUrl = url.Contains(value);
            return isIdentityServerUrl;
        }

        public void SetCustomEventHandler(CustomWebBrowserControlEventHandler value)
        {
            _customWebBrowserControlEventHandler = value;
        }
    }
}