using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using HostAppInPanelLib;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;

namespace WebBrowserLib.ChromeSelenium.WebBrowserControl
{
    public class WebBrowserExtensionSelenium : IWebBrowserExtensionWithEventBase<IWebElement>, IWebBrowserExtensionJavascript, IDocumentWaiter
    {
        private static readonly Dictionary<ChromeWrapperControl, WebBrowserExtensionSelenium>
            WebBrowserExtensionSeleniums =
                new Dictionary<ChromeWrapperControl, WebBrowserExtensionSelenium>();

        private readonly ChromeWrapperControl _webBrowser;

        private WebBrowserExtensionSelenium(ChromeWrapperControl webBrowser)
        {
            _webBrowser = webBrowser;
        }

        public TimeSpan TimeToWaitPageLoad { get; set; } = TimeSpan.FromSeconds(60*5);

        public bool JavascriptInjectionEnabled { get; set; } = true;

        public void WaitForDocumentReady(string targetUrl)
        {
            if (!Enabled)
            {
                return;
            }
            var thread = new Thread(WaitUntilDocumentIsReady);
            thread.Start(targetUrl);
        }

        public bool Enabled { get; set; } = true;

        public void AddJQueryElement()
        {
            if (!Enabled)
            {
                return;
            }
            var scriptUrl = "http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";
            AddScriptByUrlInSelenium(scriptUrl);
        }

        public void AddScriptElement(string scriptBody)
        {
            if (!Enabled)
            {
                return;
            }
            var replace = scriptBody.Replace("\\", "\\\\").Replace("'", "\\'");
            ExecuteJavascriptinSelenium(
                "var script=document.createElement('script');" +
                "script.setAttribute('type','text/javascript');" +
                $"script.innerHTML='{replace}';" +
                "document.getElementsByTagName('head')[0].appendChild(script);", false);
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

        public void DisableEventOnControl(string controlId, string eventName, string customFunctionBody = "return false")
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptinSelenium($"document.getElementById('{controlId}').{eventName}=function()" +
                                        "{"+customFunctionBody+"}", false);
        }

        public void DisableEventOnDocument(string eventName, string customFunctionBody = "return false")
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptinSelenium($"document.{eventName}=function()" + "{"+customFunctionBody+"}", false);
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

        public event EventHandler DocumentReady;

        public void EnableEventOnControl(string controlId, string eventName)
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptinSelenium($"document.getElementById('{controlId}').{eventName}=null;", false);
        }

        public void EnableEventOnDocument(string eventName)
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptinSelenium($"document.{eventName}=null", false);
        }

        public void EnableOnContextMenuToDocument()
        {
            if (!Enabled)
            {
                return;
            }
            EnableEventOnDocument("oncontextmenu");
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            return ExecuteJavascriptinSelenium(javascriptToExecute, false);
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

        public dynamic InjectAndExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled || !JavascriptInjectionEnabled)
            {
                return null;
            }
            return ExecuteJavascript(javascriptToExecute);
        }

        public void AddJavascriptByUrl(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            AddScriptByUrlInSelenium(scriptUrl);
        }

        private void AddScriptByUrlInSelenium(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptinSelenium("var script=document.createElement('script');" +
                                        "script.setAttribute('type','text/javascript');" +
                                        $"script.setAttribute('src','{scriptUrl}');" +
                                        "document.getElementsByTagName('head')[0].appendChild(script);", false);
        }

        private dynamic ExecuteJavascriptinSelenium(string script, bool waitForDocumentReady)
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

        public string GetCurrentUrl()
        {
            if (!Enabled)
            {
                return null;
            }
            var url = GetGlobalVariable("document.location");
            if (url == null)
                return null;
            return url["href"];
        }

        public static WebBrowserExtensionSelenium GetInstance(ChromeWrapperControl webBrowser)
        {
            if (!WebBrowserExtensionSeleniums.ContainsKey(webBrowser))
            {
                WebBrowserExtensionSeleniums.Add(webBrowser, new WebBrowserExtensionSelenium(webBrowser));
            }
            return WebBrowserExtensionSeleniums[webBrowser];
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
                var value = executeScript != null && (bool) executeScript;
                var currentUrl = GetCurrentUrl();
                var returnValue = value && targetUrl != null && currentUrl != null && currentUrl.ToLower().StartsWith(targetUrl.ToLower());
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