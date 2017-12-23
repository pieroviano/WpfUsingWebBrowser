using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mshtml;
using WebBrowserLib.Extensions;
using WebBrowserLib.WebBrowserControl.Helpers;

namespace WebBrowserLib.WebBrowserControl
{
    public static partial class WebBrowserExtension
    {
        public static void AttachEventHandlerToControl(this WebBrowser browser, string controlId, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            if (browser.Document != null)
            {
                var htmlDocument = browser.Document.DomDocument as HTMLDocument;
                AttachEventHandlerToControl(htmlDocument, controlId, eventName, firstArgument, customEventDelegate,
                    functionHash, getCustomEventHandler, setCustomEventHandler, removeHandlers);
            }
        }

        public static void AttachEventHandlerToDocument(this WebBrowser browser, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (browser.Document != null)
            {
                var htmlDocument = browser.Document.DomDocument as HTMLDocument;
                AttachEventHandlerToDocument(htmlDocument, eventName, firstArgument, customEventDelegate, functionHash,
                    getCustomEventHandler, setCustomEventHandler);
            }
        }

        public static void AttachCustomFunctionOnDocument(this WebBrowser webBrowser, Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();
            webBrowser.AttachEventHandlerToDocument("onclick", codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public static void CauseCsBreakpoint(this WebBrowser browser, ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
                comVisibleClass = new ComVisibleClass();
            RunCsFromJavascript(browser, comVisibleClass.CodeToExecute);
        }

        public static void DetachEventHandlersFromControl(this WebBrowser browser, string controlId,
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

        public static void DisableEventOnControl(this WebBrowser browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToControl(browser, controlId, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public static void DisableEventOnDocument(this WebBrowser browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(browser, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public static void DisableOnContextMenuOnDocument(this WebBrowser browser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(browser, "oncontextmenu",
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public static void EnableEventOnControl(this WebBrowser browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToControl(browser, controlId, eventName,
                functionHash, getCustomEventHandler);
        }

        public static void EnableEventOnDocument(this WebBrowser browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(browser, eventName,
                functionHash, getCustomEventHandler);
        }

        public static void EnableOnContextMenuToDocument(this WebBrowser browser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(browser,
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public static dynamic FindElementByAttributeValue(this WebBrowser webBrowser, string tagName, string attribute,
            string value)
        {
            var htmlDocument = webBrowser.Document?.DomDocument as HTMLDocument;
            if (htmlDocument != null)
            {
                return FindElementByAttributeValue(htmlDocument, tagName, attribute, value);
            }
            return null;
        }

        public static List<dynamic> FindElementsByAttributeValue(this WebBrowser webBrowser, string tagName,
            string attribute, string value)
        {
            var htmlDocument = webBrowser.Document?.DomDocument as HTMLDocument;
            if (htmlDocument != null)
            {
                return FindElementsByAttributeValue(htmlDocument, tagName, attribute, value);
            }
            return null;
        }

        public static IHTMLElement GetElementById(this WebBrowser browser, string controlId)
        {
            if (browser.Document != null)
            {
                var htmlDocument = browser.Document.DomDocument as HTMLDocument;
                return GetElementById(htmlDocument, controlId);
            }
            return null;
        }

        public static IEnumerable<IHTMLElement> GetElementsByCssQuery(this WebBrowser browser,
            string cssQuery)
        {
            var htmlDocument = browser.Document?.DomDocument as HTMLDocument;
            return GetElementsByCssQuery(htmlDocument, cssQuery);
        }

        public static object GetGlobalVariable(this WebBrowser browser, string variable)
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

        public static dynamic InjectAndExecuteJavascript(this WebBrowser browser, string javascriptToExecute)
        {
            if (browser.Document != null)
            {
                var htmlDocument = browser.Document.DomDocument as HTMLDocument;
                return InjectAndExecuteJavascript(htmlDocument, javascriptToExecute);
            }
            return null;
        }

        public static void InjectScript(this WebBrowser browser, string scriptUrl)
        {
            var htmlDocument = browser.Document?.DomDocument as HTMLDocument;
            InjectScript(htmlDocument, scriptUrl);
        }

        public static string RegisterCsCodeCallableFromJavascript(this WebBrowser browser, ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
                comVisibleClass = new ComVisibleClass();
            return RegisterCsCodeCallableFromJavascript(browser, comVisibleClass.CodeToExecute);
        }

        public static string RegisterCsCodeCallableFromJavascript(this WebBrowser htmlDocument,
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

        public static void RemoveEventHandlerToControl(this WebBrowser browser, string controlId, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (browser.Document != null)
            {
                var htmlDocument = browser.Document.DomDocument as HTMLDocument;
                RemoveEventHandlerToControl(htmlDocument, controlId, eventName, functionHash,
                    getCustomEventHandler);
            }
        }

        public static void RemoveEventHandlerToDocument(this WebBrowser browser, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (browser.Document != null)
            {
                var htmlDocument = browser.Document.DomDocument as HTMLDocument;
                RemoveEventHandlerToDocument(htmlDocument, eventName,
                    functionHash, getCustomEventHandler);
            }
        }

        public static void RemoveHandlersOnNavigating(this WebBrowser webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            webBrowser.Navigating += new NavigatingInterceptorWinForm(getCustomEventHandler, webBrowser)
                .WebBrowserOnNavigating;
            if (getCustomEventHandler() != null)
            {
                setCustomEventHandler(null);
            }
        }

        public static bool RunCsFromJavascript(this WebBrowser htmlDocument, Func<bool> codeToExecute)
        {
            var javascriptToExecute = RegisterCsCodeCallableFromJavascript(htmlDocument, codeToExecute);
            return Convert.ToBoolean(InjectAndExecuteJavascript(htmlDocument, javascriptToExecute));
        }

        private class NavigatingInterceptorWinForm
        {
            private readonly Func<CustomWebBrowserControlEventHandler> _getCustomEventHandler;
            private readonly WebBrowser _webBrowser;

            public NavigatingInterceptorWinForm(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                WebBrowser webBrowser)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowser = webBrowser;
            }

            public void WebBrowserOnNavigating(object sender, WebBrowserNavigatingEventArgs e)
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
                    if (eventName.StartsWith($"{DocumentEventPrefix}."))
                    {
                        RemoveEventHandlerToDocument(_webBrowser, eventName, eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        RemoveEventHandlerToControl(_webBrowser, strings[0], strings[1], eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                }
            }
        }
    }
}