using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using HostAppInPanelLib.Controls;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;

namespace WebBrowserLib.ChromeSelenium.WebBrowserControl
{
    public class WebBrowserExtensionSelenium : IWebBrowserExtensionWithEventBase<IWebElement>,
        IWebBrowserExtensionJavascript, IDocumentWaiter
    {
        private static readonly Dictionary<BrowserWrapperControl, WebBrowserExtensionSelenium>
            WebBrowserExtensionSeleniums =
                new Dictionary<BrowserWrapperControl, WebBrowserExtensionSelenium>();

        private readonly BrowserWrapperControl _webBrowser;

        private WebBrowserExtensionSelenium(BrowserWrapperControl webBrowser)
        {
            _webBrowser = webBrowser;
        }

        public bool JavascriptInjectionEnabled { get; set; } = true;

        public TimeSpan TimeToWaitPageLoad { get; set; } = TimeSpan.FromSeconds(60 * 5);

        public void WaitForDocumentReady(string targetUrl)
        {
            if (!Enabled)
            {
                return;
            }
            var thread = new Thread(WaitUntilDocumentIsReady);
            thread.Start(targetUrl);
        }

        public void DisableEventOnControl(string controlId, string eventName,
            string customFunctionBody = "return false")
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptInSelenium($"document.getElementById('{controlId}').{eventName}=function()" +
                                        "{" + customFunctionBody + "}", false);
        }

        public void DisableEventOnDocument(string eventName, string customFunctionBody = "return false")
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptInSelenium($"document.{eventName}=function()" + "{" + customFunctionBody + "}", false);
        }

        public void DisableOnContextMenuOnDocument()
        {
            if (!Enabled)
            {
                return;
            }
            DisableEventOnDocument("oncontextmenu");
            DisableEventOnDocument("onmousedown", "if (e.which==3) {return false;}");
        }

        public void EnableEventOnControl(string controlId, string eventName)
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptInSelenium($"document.getElementById('{controlId}').{eventName}=null;", false);
        }

        public void EnableEventOnDocument(string eventName)
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptInSelenium($"document.{eventName}=null", false);
        }

        public void EnableOnContextMenuToDocument()
        {
            if (!Enabled)
            {
                return;
            }
            EnableEventOnDocument("oncontextmenu");
        }

        public bool Enabled { get; set; } = true;

        public void AddJQueryScript(string url)
        {
            if (!Enabled)
            {
                return;
            }
            var scriptUrl = "http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";
            GetScriptsElementsInSelenium(scriptUrl);
        }

        public void AddScriptElement(string scriptBody)
        {
            if (!Enabled)
            {
                return;
            }
            var replace = scriptBody.Replace("\\", "\\\\").Replace("'", "\\'");
            ExecuteJavascriptInSelenium(
                "var script=document.createElement('script');" +
                "script.setAttribute('type','text/javascript');" +
                $"script.innerHTML='{replace}';" +
                "document.getElementsByTagName('head')[0].appendChild(script);", false);
            while ((string)GetGlobalVariable("document.readyState") != "complete")
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
            }
        }

        public void AddScriptsElements(string scriptUrl)
        {
            EnsureScriptIsInCache(scriptUrl);
            GetScriptsElementsInSelenium(scriptUrl);
        }

        public void Navigate(string targetUrl)
        {
            if (!Enabled)
            {
                return;
            }
            _webBrowser.WebDriver.Url = targetUrl;
            WaitForDocumentReady(targetUrl);
        }

        public event EventHandler DocumentReady;

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            return ExecuteJavascriptInSelenium(javascriptToExecute, false);
        }

        public void EnsureScriptIsInCache(string url)
        {
            if (!Enabled)
            {
                return;
            }
            string scriptId = ExecuteJavascriptInSelenium("var scripts=document.getElementsByTagName('script');" +
                $"for(var i=0;i<scripts.length;i++)if (scripts[i].src=='{url}') return scripts[i].id;", false);
            //while (EvaluateExpression(
            //           $"alert(document.all['{scriptId}'].outerHTML);return eval(document.all['{scriptId}'].readyState == 'loaded' || document.all['{scriptId}'].readyState == 'complete')") !=
            //       true)
            //{
            //    Application.Current.Dispatcher.Invoke(
            //        DispatcherPriority.Background,
            //        new ThreadStart(delegate { }));
            //}
        }

        public dynamic FindElementByAttributeValue(string tagName, string attribute, string value)
        {
            if (!Enabled)
            {
                return null;
            }
            return _webBrowser.WebDriver.FindElement(By.CssSelector($"{attribute}={value}"));
        }

        public IEnumerable<IWebElement> FindElementsByAttributeValue(string tagName, string attribute, string value)
        {
            if (!Enabled)
            {
                return null;
            }
            return _webBrowser.WebDriver.FindElements(By.CssSelector($"{attribute}={value}"));
        }

        public IWebElement GetElementById(string controlId)
        {
            if (!Enabled)
            {
                return null;
            }
            return _webBrowser.WebDriver.FindElement(By.Id(controlId));
        }

        public IEnumerable<IWebElement> GetElementsByCssQuery(string cssQuery)
        {
            if (!Enabled)
            {
                return null;
            }
            return _webBrowser.WebDriver.FindElements(By.CssSelector(cssQuery));
        }

        public dynamic GetGlobalVariable(string variable)
        {
            if (!Enabled)
            {
                return null;
            }
            return ScriptHelper.GetGlobalVariable(variable, InjectAndExecuteJavascript);
        }

        public dynamic EvaluateExpression(string expression)
        {
            if (!Enabled)
            {
                return null;
            }
            return ScriptHelper.EvaluateExpression(expression, InjectAndExecuteJavascript);
        }

        public string GetCurrentUrl()
        {
            if (!Enabled)
            {
                return null;
            }
            var url = ExecuteJavascriptInSelenium("return (document.location.href);", false);
            return url;
        }

        public void AddJavascriptByUrl(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            var scriptId = GetScriptsElementsInSelenium(scriptUrl);
        }

        private string GetScriptsElementsInSelenium(string scriptUrl)
        {
            if (!Enabled)
            {
                return null;
            }
            var scriptId = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");
            ExecuteJavascriptInSelenium("var script=document.createElement('script');" +
                                        $"script.setAttribute('id','{scriptId}');" +
                                        "script.setAttribute('type','text/javascript');" +
                                        $"script.setAttribute('src','{scriptUrl}');" +
                                        "document.getElementsByTagName('head')[0].appendChild(script);", false);
            EnsureScriptIsInCache(scriptUrl);
            return scriptId;
        }

        private dynamic ExecuteJavascriptInSelenium(string script, bool waitForDocumentReady)
        {
            if (!Enabled)
            {
                return null;
            }
            var javaScriptExecutor = _webBrowser.WebDriver as IJavaScriptExecutor;
            var executeJavascriptinSelenium = javaScriptExecutor?.ExecuteScript(
                script);
            if (waitForDocumentReady)
            {
                WaitForDocumentReady(null);
            }
            return executeJavascriptinSelenium;
        }

        public static WebBrowserExtensionSelenium GetInstance(BrowserWrapperControl webBrowser)
        {
            if (!WebBrowserExtensionSeleniums.ContainsKey(webBrowser))
            {
                WebBrowserExtensionSeleniums.Add(webBrowser, new WebBrowserExtensionSelenium(webBrowser));
            }
            return WebBrowserExtensionSeleniums[webBrowser];
        }

        public dynamic InjectAndExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled || !JavascriptInjectionEnabled)
            {
                return null;
            }
            return ExecuteJavascript(javascriptToExecute);
        }

        private void WaitUntilDocumentIsReady(object param)
        {
            var targetUrl = param as string;
            var javaScriptExecutor = _webBrowser.WebDriver as IJavaScriptExecutor;
            var wait = new WebDriverWait(_webBrowser.WebDriver, TimeToWaitPageLoad);

            // Check if document is ready
            var readyCondition = new Func<IWebDriver, bool>(webDriver =>
            {
                var executeScript = javaScriptExecutor?.ExecuteScript(
                    "return (document.readyState == 'complete');");
                var value = executeScript != null && (bool)executeScript;
                var currentUrl = GetCurrentUrl();
                var returnValue = value && targetUrl != null && currentUrl != null &&
                                  currentUrl.ToLower().StartsWith(targetUrl.ToLower());
                return returnValue;
            });
            try
            {
                wait.Until(readyCondition);
                Application.Current.Dispatcher.Invoke(() => { DocumentReady?.Invoke(this, EventArgs.Empty); });
            }
            catch (Exception e)
            {
                Debug.Write($"Exception waitning document ready: {e}");
            }
        }
    }
}