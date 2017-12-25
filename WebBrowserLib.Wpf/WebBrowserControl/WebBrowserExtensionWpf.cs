using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using mshtml;
using WebBrowserLib.Extensions;
using WebBrowserLib.WebBrowserControl;
using WebBrowserLib.WebBrowserControl.Helpers;
using WebBrowserLib.WebBrowserControl.Interfaces;

namespace WebBrowserLib.Wpf.WebBrowserControl
{
    public partial class WebBrowserExtensionWpf : IWebBrowserExtension
    {
        private WebBrowserExtensionWpf()
        {
        }

        public static WebBrowserExtensionWpf Instance { get; } = new WebBrowserExtensionWpf();

        public void AddJQueryElement(object head)
        {
            WebBrowserExtension.Instance.AddJQueryElement(head as HTMLHeadElement);
        }

        public void AddScriptElement(object head, string scriptBody)
        {
            WebBrowserExtension.Instance.AddScriptElement(head as HTMLHeadElement, scriptBody);
        }

        public void AttachCustomFunctionOnControl(WebBrowser webBrowser, string controlId, string eventName,
            Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();

            WebBrowserExtension.Instance.AttachEventHandlerToControl(webBrowser.Document as HTMLDocument, controlId,
                eventName, codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void AttachCustomFunctionOnDocument(WebBrowser webBrowser, string eventName, Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();

            WebBrowserExtension.Instance.AttachEventHandlerToDocument(webBrowser.Document as HTMLDocument, eventName,
                codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void AttachEventHandlerToControl(WebBrowser browser, string controlId, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            WebBrowserExtension.Instance.AttachEventHandlerToControl(htmlDocument, controlId, eventName, firstArgument,
                customEventDelegate,
                functionHash, getCustomEventHandler, setCustomEventHandler, removeHandlers);
        }

        public void AttachEventHandlerToDocument(WebBrowser browser, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            WebBrowserExtension.Instance.AttachEventHandlerToDocument(htmlDocument, eventName, firstArgument,
                customEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void CauseCsBreakpoint(WebBrowser browser, ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
            {
                comVisibleClass = new ComVisibleClass();
            }
            RunCsFromJavascript(browser, comVisibleClass.CodeToExecute);
        }

        public void DetachEventHandlersFromControl(WebBrowser browser, string controlId,
            bool removeHandlers = false,
            params string[] eventNames)
        {
            var cleanHandlers = ScriptHelper.PrepareCleanHandlers(eventNames);
            if (!removeHandlers)
            {
                if (!string.IsNullOrEmpty(cleanHandlers))
                {
                    InjectAndExecuteJavascript(browser, cleanHandlers);
                }
                return;
            }
            InjectAndExecuteJavascript(browser,
                ScriptHelper.GetJavascriptToExecuteToRemoveHandlers(controlId, cleanHandlers));
        }

        public void DisableEventOnControl(WebBrowser browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToControl(browser, controlId, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableEventOnDocument(WebBrowser browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(browser, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableOnContextMenuOnDocument(WebBrowser browser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(browser, "oncontextmenu",
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void EnableEventOnControl(WebBrowser browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToControl(browser, controlId, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableEventOnDocument(WebBrowser browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(browser, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableOnContextMenuToDocument(WebBrowser browser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(browser,
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public dynamic ExecuteJavascript(WebBrowser htmlDocument, string javascriptToExecute)
        {
            return WebBrowserExtension.Instance.ExecuteJavascript(htmlDocument.Document as HTMLDocument,
                javascriptToExecute);
        }

        public dynamic FindElementByAttributeValue(WebBrowser webBrowser, string tagName, string attribute,
            string value)
        {
            var htmlDocument = webBrowser.Document as HTMLDocument;

            return WebBrowserExtension.Instance.FindElementByAttributeValue(htmlDocument, tagName, attribute, value);
        }

        public List<dynamic> FindElementsByAttributeValue(WebBrowser webBrowser, string tagName,
            string attribute, string value)
        {
            var htmlDocument = webBrowser.Document as HTMLDocument;
            return WebBrowserExtension.Instance.FindElementsByAttributeValue(htmlDocument, tagName, attribute, value);
        }

        public IHTMLElement GetElementById(WebBrowser browser, string controlId)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            return WebBrowserExtension.Instance.GetElementById(htmlDocument, controlId);
        }

        public IEnumerable<IHTMLElement> GetElementsByCssQuery(WebBrowser browser,
            string cssQuery)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            return WebBrowserExtension.Instance.GetElementsByCssQuery(htmlDocument, cssQuery);
        }

        public object GetGlobalVariable(WebBrowser browser, string variable)
        {
            var variablePath = variable.Split('.');
            var i = 0;
            object result = null;
            var variableName = "window";
            while (i < variablePath.Length)
            {
                variableName = variableName + "." + variablePath[i];
                result = InjectAndExecuteJavascript(browser, variableName);
                if (result == null)
                {
                    return null;
                }
                i++;
            }
            return result;
        }

        public dynamic InjectAndExecuteJavascript(WebBrowser browser, string javascriptToExecute)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            return WebBrowserExtension.Instance.InjectAndExecuteJavascript(htmlDocument, javascriptToExecute);
        }

        public void InjectScript(WebBrowser browser, string scriptUrl)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            WebBrowserExtension.Instance.InjectScript(htmlDocument, scriptUrl);
        }

        public string RegisterCsCodeCallableFromJavascript(WebBrowser browser, ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
            {
                comVisibleClass = new ComVisibleClass();
            }
            return RegisterCsCodeCallableFromJavascript(browser, comVisibleClass.CodeToExecute);
        }

        public string RegisterCsCodeCallableFromJavascript(WebBrowser htmlDocument,
            Func<bool> codeToExecute)
        {
            var htmlDocumentObjectForScripting = codeToExecute.Target;
            if (htmlDocumentObjectForScripting.GetType().GetCustomAttribute(typeof(ComVisibleAttribute)) == null)
            {
                throw new NotSupportedException("The class is not COM visible");
            }
            htmlDocument.ObjectForScripting = htmlDocumentObjectForScripting;
            var javascriptToExecute = $"window.external.{codeToExecute.Method.Name}();";
            return javascriptToExecute;
        }

        public void RemoveEventHandlerToControl(WebBrowser browser, string controlId, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            WebBrowserExtension.Instance.RemoveEventHandlerToControl(htmlDocument, controlId, eventName, functionHash,
                getCustomEventHandler);
        }

        public void RemoveEventHandlerToDocument(WebBrowser browser, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            WebBrowserExtension.Instance.RemoveEventHandlerToDocument(htmlDocument, eventName,
                functionHash, getCustomEventHandler);
        }

        public void RemoveHandlersOnNavigating(WebBrowser webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            webBrowser.Navigating += new NavigatingInterceptorWpf(getCustomEventHandler, webBrowser)
                .WebBrowserOnNavigating;
            if (getCustomEventHandler() != null)
            {
                setCustomEventHandler(null);
            }
        }

        public void RunCsFromJavascript(WebBrowser htmlDocument, Func<bool> codeToExecute)
        {
            var javascriptToExecute = RegisterCsCodeCallableFromJavascript(htmlDocument, codeToExecute);
            InjectAndExecuteJavascript(htmlDocument, javascriptToExecute);
        }

        private class NavigatingInterceptorWpf
        {
            private readonly Func<CustomWebBrowserControlEventHandler> _getCustomEventHandler;
            private readonly WebBrowser _webBrowser;

            public NavigatingInterceptorWpf(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                WebBrowser webBrowser)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowser = webBrowser;
            }

            public void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
            {
                var eventHandler = _getCustomEventHandler();
                if (eventHandler == null)
                {
                    return;
                }
                var eventHandlerDelegates = eventHandler.Delegates;
                for (var index = 0; index < eventHandlerDelegates.Count; index++)
                {
                    var eventHandlerDelegate = eventHandlerDelegates[index];
                    var eventName = eventHandlerDelegate.Item1;
                    if (eventName.StartsWith($"{WebBrowserExtension.Instance.DocumentEventPrefix}."))
                    {
                        WebBrowserExtension.Instance.RemoveEventHandlerToDocument(_webBrowser.Document as HTMLDocument,
                            eventName.Split('.')[1], eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        WebBrowserExtension.Instance.RemoveEventHandlerToControl(_webBrowser.Document as HTMLDocument,
                            strings[0], strings[1], eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    index--;
                }
            }
        }
    }
}