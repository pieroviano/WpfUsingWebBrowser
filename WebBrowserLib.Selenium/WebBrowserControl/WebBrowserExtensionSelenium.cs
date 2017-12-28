using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using HostAppInPanelLib;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Interfaces;

namespace WebBrowserLib.Selenium.WebBrowserControl
{
    public class WebBrowserExtensionSelenium : IWebBrowserExtensionWithEventBase<IWebElement>
    {
        private static readonly Dictionary<ChromeWrapperControl, WebBrowserExtensionSelenium>
            WebBrowserExtensionSeleniums =
                new Dictionary<ChromeWrapperControl, WebBrowserExtensionSelenium>();

        private readonly ChromeWrapperControl _webBrowser;

        private WebBrowserExtensionSelenium(ChromeWrapperControl webBrowser)
        {
            _webBrowser = webBrowser;
        }

        public TimeSpan TimeToWaitPageLoad { get; set; } = TimeSpan.FromSeconds(60);

        public bool JavascriptInjectionEnabled { get; set; } = true;

        public bool Enabled { get; set; } = true;

        public void AddJQueryElement()
        {
            var scriptUrl = "http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";
            ExecuteJavascriptInSelenium(scriptUrl);
        }

        public void AddScriptElement(string scriptBody)
        {
            ExecuteJavascriptinSelenium(
                "var script=document.createElement('script');" +
                "script.setAttribute('type','text/javascript')" +
                $"script.innerHTML='{scriptBody}';" +
                "document.getElementsByTagName('head')[0].appendChild(script);");
        }

        public void Navigate(string targetUrl)
        {
            _webBrowser.WebDriver.Url = targetUrl;
            var thread = new Thread(WaitUntilDocumentIsReady);
            thread.Start();
        }

        public void DisableEventOnControl(string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            ExecuteJavascriptinSelenium($"document.getElementById''{controlId}).{eventName}=function()" +
                                        "{return false;}");
        }

        public void DisableEventOnDocument(string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            ExecuteJavascriptinSelenium($"document.{eventName}=function()" + "{return false;}");
        }

        public void DisableOnContextMenuOnDocument(Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler)
        {
            DisableEventOnDocument("oncontextmenu", getControlEventHandler, setControlEventHandler);
        }

        public event EventHandler DocumentReady;

        public void EnableEventOnControl(string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            ExecuteJavascriptinSelenium($"document.getElementById''{controlId}).{eventName}=null;");
        }

        public void EnableEventOnDocument(string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            ExecuteJavascriptinSelenium($"document.{eventName}=null");
        }

        public void EnableOnContextMenuToDocument(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            EnableEventOnDocument("oncontextmenu", getCustomEventHandler, setCustomEventHandler);
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            return ExecuteJavascriptinSelenium(javascriptToExecute);
        }

        public dynamic FindElementByAttributeValue(string tagName, string attribute, string value)
        {
            return _webBrowser.WebDriver.FindElement(By.CssSelector($"{attribute}={value}"));
        }

        public IEnumerable<IWebElement> FindElementsByAttributeValue(string tagName, string attribute, string value)
        {
            return _webBrowser.WebDriver.FindElements(By.CssSelector($"{attribute}={value}"));
        }

        public IWebElement GetElementById(string controlId)
        {
            return _webBrowser.WebDriver.FindElement(By.Id(controlId));
        }

        public IEnumerable<IWebElement> GetElementsByCssQuery(string cssQuery)
        {
            return _webBrowser.WebDriver.FindElements(By.CssSelector(cssQuery));
        }

        public dynamic GetGlobalVariable(string variable)
        {
            if (!Enabled)
            {
                return null;
            }
            var variablePath = variable.Split('.');
            var i = 0;
            object result = null;
            var variableName = "window";
            while (i < variablePath.Length)
            {
                variableName = variableName + "." + variablePath[i];
                result = InjectAndExecuteJavascript(variableName);
                if (result == null)
                {
                    return null;
                }
                i++;
            }
            return result;
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
            ExecuteJavascriptInSelenium(scriptUrl);
        }

        private dynamic ExecuteJavascriptinSelenium(string script)
        {
            var javaScriptExecutor = _webBrowser.WebDriver as IJavaScriptExecutor;
            return javaScriptExecutor?.ExecuteScript(
                script);
        }

        private void ExecuteJavascriptInSelenium(string scriptUrl)
        {
            ExecuteJavascriptinSelenium("var script=document.createElement('script');" +
                                        "script.setAttribute('type','text/javascript')" +
                                        $"script.setAttribute('src','{scriptUrl}')" +
                                        "document.getElementsByTagName('head')[0].appendChild(script);");
        }

        public static WebBrowserExtensionSelenium GetInstance(ChromeWrapperControl webBrowser)
        {
            if (!WebBrowserExtensionSeleniums.ContainsKey(webBrowser))
            {
                WebBrowserExtensionSeleniums.Add(webBrowser, new WebBrowserExtensionSelenium(webBrowser));
            }
            return WebBrowserExtensionSeleniums[webBrowser];
        }

        private void WaitUntilDocumentIsReady()
        {
            var javaScriptExecutor = _webBrowser.WebDriver as IJavaScriptExecutor;
            var wait = new WebDriverWait(_webBrowser.WebDriver, TimeToWaitPageLoad);

            // Check if document is ready
            var readyCondition = new Func<IWebDriver, bool>(webDriver =>
            {
                var executeScript = javaScriptExecutor?.ExecuteScript(
                    "return (document.readyState == 'complete' && jQuery.active == 0)");
                return executeScript != null && (bool) executeScript;
            });
            wait.Until(readyCondition);
            Application.Current.Dispatcher.Invoke(() => { DocumentReady?.Invoke(this, EventArgs.Empty); });
        }
    }
}