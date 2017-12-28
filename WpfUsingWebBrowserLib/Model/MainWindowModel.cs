using System;
using System.Configuration;
using WebBrowserLib.EventHandling;

namespace UsingWebBrowserLib.Model
{
    public class MainWindowModel
    {
        private CustomWebBrowserControlEventHandler _customWebBrowserControlEventHandler;
        public static int Port { get; set; } = 5003;

        public static string StartupJavascript { get; set; }
        public string IndexPage { get; } = "index.html";
        public string CallbackPage { get; } = "callback.html";
        public string RedirectUri { get; } = "redirect_uri";
        public string AccessToken { get; } = "access_token";
        public string LogoutJavascript { get; } = "logout();";
        public string IgnoreOnSelectStart { get; } = "document.onselectstart=function(){return false;}";
        public string IgnoreOnContextMenu { get; } = "document.oncontextmenu=function(){return false;}";


        public bool DontDisableOnSelectionStartToDocument { get; set; } =
            ConfigurationManager.AppSettings["DontDisableOnSelectionStartToDocument"]?.ToLower() == "true";

        public bool DontDisableOnContextMenuToDocument { get; set; } =
            ConfigurationManager.AppSettings["DontDisableOnContextMenuOnDocument"]?.ToLower() == "true";

        public bool WebBrowserExtensionEnabled { get; set; } =
            ConfigurationManager.AppSettings["WebBrowserExtensionEnabled"]?.ToLower() == "true";

        public bool WebBrowserExtensionJavascriptInjectionEnabled { get; set; } =
            ConfigurationManager.AppSettings["WebBrowserExtensionJavascriptInjectionEnabled"]?.ToLower() == "true";


        public string LoginJavascript =>
            @"$(function () {" + StartupJavascript + "login();});";

        public static string UrlPrefix => $"http://localhost:{Port}/";


        public static string IdentityServerUrl => ConfigurationManager.AppSettings["IdentityServerUrl"];

        public static string ApiServerUrl => ConfigurationManager.AppSettings["ApiServerUrl"];

        public CustomWebBrowserControlEventHandler GetCustomEventHandler()
        {
            return _customWebBrowserControlEventHandler;
        }

        public static bool IsIdentityServerUrl(string url)
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