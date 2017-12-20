using System.Configuration;
using WebBrowserLib.WebBrowserControl;
using WebBrowserLib.WebBrowserControl.Helpers;

namespace WpfUsingWebBrowser.Model
{
    public class MainWindowModel
    {

        private CustomWebBrowserControlEventHandler _customWebBrowserControlEventHandler;

        public CustomWebBrowserControlEventHandler GetCustomEventHandler()
        {
            return _customWebBrowserControlEventHandler;
        }

        public void SetCustomEventHandler(CustomWebBrowserControlEventHandler value)
        {
            _customWebBrowserControlEventHandler = value;
        }

        public static string CsBreakpoint { get; set; }
        public string IndexPage { get; } = "index.html";
        public string CallbackPage { get; } = "callback.html";
        public string RedirectUri { get; } = "redirect_uri";
        public string AccessToken { get; } = "access_token";
        public string LogoutJavascript { get; } = "logout();";

        public bool DisableOnContextMenuToDocument { get; set; } =
            ConfigurationManager.AppSettings["DisableOnContextMenuToDocument"]?.ToLower() == "true";

        public bool WebBrowserExtensionEnabled { get; set; } =
            ConfigurationManager.AppSettings["WebBrowserExtensionEnabled"]?.ToLower() == "true";

        public bool WebBrowserExtensionJavascriptInjectionEnabled { get; set; } =
            ConfigurationManager.AppSettings["WebBrowserExtensionJavascriptInjectionEnabled"]?.ToLower() == "true";


        public string LoginJavascript =>
                @"var tid = setInterval( function () { if ( document.readyState !== 'complete' ) {return;} clearInterval( tid );" +
                ScriptHelper.JavascriptBreakIfDebuggerIsAttached()
                + CsBreakpoint + "login();}, 100 );";

        public string RemoveSelectionJavascript { get; } = @"document.onselectstart=function(){return false;}";

        public static string UrlPrefix { get; } = "http://localhost:5003/";


        public static string IdentityServerUrl => ConfigurationManager.AppSettings["IdentityServerUrl"];

        public static string ApiServerUrl => ConfigurationManager.AppSettings["ApiServerUrl"];
    }
}