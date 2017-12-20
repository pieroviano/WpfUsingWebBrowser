using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using mshtml;
using WebBrowserLib.WebBrowserControl.Helpers;

namespace WebBrowserLib.WebBrowserControl
{
    public static partial class WebBrowserExtension
    {
        public static void AttachEventHandlerToControl(this WebBrowser browser, string controlId, string eventName,
            Func<bool> customEventDelegate, IntPtr functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            AttachEventHandlerToControl(htmlDocument, controlId, eventName, customEventDelegate,
                functionHash, getCustomEventHandler, setCustomEventHandler, removeHandlers);
        }

        public static void AttachEventHandlerToDocument(this WebBrowser browser, string eventName,
            Func<bool> customEventDelegate, IntPtr functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            AttachEventHandlerToDocument(htmlDocument, eventName, customEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public static void AttachFunctionOnClickPlusShift(this WebBrowser webBrowser, Func<bool> codeToExecute,
            IntPtr functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = () =>
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    codeToExecute();
                }
                return true;
            };
            webBrowser.AttachEventHandlerToDocument("onclick", customEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public static void CauseCsBreakpoint(this WebBrowser browser)
        {
            RunCsFromJavascript(browser, new ComVisibleClass().CodeToExecute);
        }

        public static void DetachEventHandlersFromControl(this WebBrowser browser, string controlId,
            bool removeHandlers = false,
            params string[] eventNames)
        {
            var cleanHandlers = ScriptHelper.PrepareCleanHandlers(eventNames);
            if (!removeHandlers)
            {
                if (!string.IsNullOrEmpty(cleanHandlers))
                    InjectAndExecuteJavascript(browser, cleanHandlers);
                return;
            }
            InjectAndExecuteJavascript(browser,
                ScriptHelper.GetJavascriptToExecuteToRemoveHandlers(controlId, cleanHandlers));
        }

        public static void DisableOnContextMenuToDocument(this WebBrowser browser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = (IntPtr)customEventDelegate.GetHashCode();
            AttachEventHandlerToDocument(browser, "oncontextmenu",
                customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public static void EnableOnContextMenuToDocument(this WebBrowser browser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = (IntPtr)customEventDelegate.GetHashCode();
            RemoveEventHandlerToDocument(browser,
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public static dynamic FindElementByAttributeValue(this WebBrowser webBrowser, string tagName, string attribute,
            string value)
        {
            var htmlDocument = webBrowser.Document as HTMLDocument;

            return FindElementByAttributeValue(htmlDocument, tagName, attribute, value);
        }

        public static List<dynamic> FindElementsByAttributeValue(this WebBrowser webBrowser, string tagName,
            string attribute, string value)
        {
            var htmlDocument = webBrowser.Document as HTMLDocument;
            return FindElementsByAttributeValue(htmlDocument, tagName, attribute, value);
        }

        public static IHTMLElement GetElementById(this WebBrowser browser, string controlId)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            return GetElementById(htmlDocument, controlId);
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
                    return null;
                i++;
            }
            return result;
        }

        public static dynamic InjectAndExecuteJavascript(this WebBrowser browser, string javascriptToExecute)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            return InjectAndExecuteJavascript(htmlDocument, javascriptToExecute);
        }

        public static string RegisterCsBreakpoint(this WebBrowser browser)
        {
            return RegisterCsCodeCallableFromJavascript(browser, new ComVisibleClass().CodeToExecute);
        }

        public static string RegisterCsCodeCallableFromJavascript(this WebBrowser htmlDocument,
            Func<bool> codeToExecute)
        {
            var htmlDocumentObjectForScripting = codeToExecute.Target;
            if (htmlDocumentObjectForScripting.GetType().GetCustomAttribute(typeof(ComVisibleAttribute)) == null)
                throw new NotSupportedException("The class is not COM visible");
            htmlDocument.ObjectForScripting = htmlDocumentObjectForScripting;
            var javascriptToExecute = $"window.external.{codeToExecute.Method.Name}();";
            return javascriptToExecute;
        }

        public static void RemoveEventHandlerToControl(this WebBrowser browser, string controlId, string eventName,
            IntPtr functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            RemoveEventHandlerToControl(htmlDocument, controlId, eventName, functionHash,
                getCustomEventHandler);
        }

        public static void RemoveEventHandlerToDocument(this WebBrowser browser, string eventName,
            IntPtr functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            var htmlDocument = browser.Document as HTMLDocument;
            RemoveEventHandlerToDocument(htmlDocument, eventName,
                functionHash, getCustomEventHandler);
        }

        public static void RemoveHandlersOnNavigating(this WebBrowser webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            webBrowser.Navigating += new NavigatingInterceptorWpf(getCustomEventHandler, webBrowser)
                .WebBrowserOnNavigating;
        }

        public static void RunCsFromJavascript(this WebBrowser htmlDocument, Func<bool> codeToExecute)
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
                    return;
                var eventHandlerDelegates = eventHandler.Delegates;
                foreach (var eventHandlerDelegate in eventHandlerDelegates)
                {
                    var eventName = eventHandlerDelegate.Item1;
                    if (eventName.StartsWith($"{DocumentEventPrefix}."))
                    {
                        RemoveEventHandlerToDocument(_webBrowser, eventName.Split('.')[1], eventHandlerDelegate.Item3,
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