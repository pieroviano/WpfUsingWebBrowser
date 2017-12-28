using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using mshtml;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;
using WebBrowserLib.MsHtml.WebBrowserControl;
using WebBrowserLib.Wpf.Utility;

namespace WebBrowserLib.Wpf.WebBrowserControl
{
    public class WebBrowserExtensionWpf : IWebBrowserExtensionWithEvent<IHTMLElement>
    {
        private static readonly Dictionary<WebBrowser, WebBrowserExtensionWpf> WebBrowserExtensionWpfs =
            new Dictionary<WebBrowser, WebBrowserExtensionWpf>();

        private readonly WebBrowser _webBrowser;

        private WebBrowserExtensionWpf(WebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
            _webBrowser.Loaded += (sender, args) => { DocumentReady?.Invoke(this, args); };
        }


        public bool Enabled
        {
            get => WebBrowserExtensionMsHtmlDocument.Instance.Enabled;
            set => WebBrowserExtensionMsHtmlDocument.Instance.Enabled = value;
        }

        public bool JavascriptInjectionEnabled
        {
            get => WebBrowserExtensionMsHtmlDocument.Instance.JavascriptInjectionEnabled;
            set => WebBrowserExtensionMsHtmlDocument.Instance.JavascriptInjectionEnabled = value;
        }

        public void AddJQueryElement()
        {
            var head = (HTMLHeadElement) ((HTMLDocument) _webBrowser.Document)?.getElementsByTagName("head").item(0);

            if (head != null)
            {
                var htmlDocument = head.ownerDocument as HTMLDocument;
                var scriptEl = htmlDocument?.createElement("script") as HTMLScriptElement;
                var jQueryElement = (IHTMLScriptElement) scriptEl;
                if (jQueryElement != null)
                {
                    jQueryElement.src = @"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";
                }

                head.appendChild((IHTMLDOMNode) scriptEl);
            }
        }

        public void AddScriptElement(string scriptBody)
        {
            var head = (HTMLHeadElement) ((HTMLDocument) _webBrowser.Document)?.getElementsByTagName("head").item(0);
            var scriptEl = (head?.ownerDocument as HTMLDocument)?.createElement("script") as HTMLScriptElement;
            if (scriptEl != null)
            {
                scriptEl.innerHTML = scriptBody;

                head.appendChild((IHTMLDOMNode) scriptEl);
            }
        }

        public void Navigate(string targetUrl)
        {
            _webBrowser.Navigate(targetUrl);
        }

        public void AttachEventHandlerToControl(string controlId, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToControl(htmlDocument, controlId, eventName,
                firstArgument,
                customEventDelegate,
                functionHash, getCustomEventHandler, setCustomEventHandler, removeHandlers);
        }

        public void AttachEventHandlerToDocument(string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToDocument(htmlDocument, eventName,
                firstArgument,
                customEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void DetachEventHandlersFromControl(string controlId,
            bool removeHandlers = false,
            params string[] eventNames)
        {
            var cleanHandlers = ScriptHelper.PrepareCleanHandlers(eventNames);
            if (!removeHandlers)
            {
                if (!string.IsNullOrEmpty(cleanHandlers))
                {
                    InjectAndExecuteJavascript(cleanHandlers);
                }
                return;
            }
            InjectAndExecuteJavascript(ScriptHelper.GetJavascriptToExecuteToRemoveHandlers(controlId, cleanHandlers));
        }

        public void DisableEventOnControl(string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToControl(controlId, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableEventOnDocument(string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableOnContextMenuOnDocument(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument("oncontextmenu",
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void EnableEventOnControl(string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToControl(controlId, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableEventOnDocument(string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableOnContextMenuToDocument(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            return WebBrowserExtensionMsHtmlDocument.Instance.ExecuteJavascript(_webBrowser.Document as HTMLDocument,
                javascriptToExecute);
        }

        public dynamic FindElementByAttributeValue(string tagName, string attribute,
            string value)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;

            return WebBrowserExtensionMsHtmlDocument.Instance.FindElementByAttributeValue(htmlDocument, tagName,
                attribute, value);
        }

        public IEnumerable<IHTMLElement> FindElementsByAttributeValue(string tagName,
            string attribute, string value)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            return WebBrowserExtensionMsHtmlDocument.Instance.FindElementsByAttributeValue(htmlDocument, tagName,
                attribute, value);
        }

        public IHTMLElement GetElementById(string controlId)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            return WebBrowserExtensionMsHtmlDocument.Instance.GetElementById(htmlDocument, controlId);
        }

        public IEnumerable<IHTMLElement> GetElementsByCssQuery(
            string cssQuery)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            return WebBrowserExtensionMsHtmlDocument.Instance.GetElementsByCssQuery(htmlDocument, cssQuery);
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
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            return WebBrowserExtensionMsHtmlDocument.Instance.ExecuteJavascript(htmlDocument,
                javascriptToExecute);
        }

        public void RemoveEventHandlerToControl(string controlId, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToControl(htmlDocument, controlId, eventName,
                functionHash,
                getCustomEventHandler);
        }

        public void RemoveEventHandlerToDocument(string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToDocument(htmlDocument, eventName,
                functionHash, getCustomEventHandler);
        }

        public void RemoveHandlersOnNavigating(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            _webBrowser.Navigating += new NavigatingInterceptorWpf(getCustomEventHandler, _webBrowser)
                .WebBrowserOnNavigating;
            if (getCustomEventHandler() != null)
            {
                setCustomEventHandler(null);
            }
        }

        public event EventHandler DocumentReady;

        public void AttachCustomFunctionOnControl(string controlId, string eventName,
            Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();

            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToControl(_webBrowser.Document as HTMLDocument,
                controlId,
                eventName, codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void AttachCustomFunctionOnDocument(string eventName, Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();

            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToDocument(
                _webBrowser.Document as HTMLDocument,
                eventName,
                codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public string RegisterCsCodeCallableFromJavascript(ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
            {
                comVisibleClass = new ComVisibleClass();
            }
            return RegisterCsCodeCallableFromJavascript(comVisibleClass.CodeToExecute);
        }

        public void AddJavascriptByUrl(string scriptUrl)
        {
            var htmlDocument = _webBrowser.Document as HTMLDocument;
            var item = htmlDocument?.getElementsByName("head").item(0);
            WebBrowserExtensionMsHtmlDocument.Instance.AddScriptByUrl(item, scriptUrl);
        }

        public void CauseCsBreakpoint(ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
            {
                comVisibleClass = new ComVisibleClass();
            }
            RunCsFromJavascript(comVisibleClass.CodeToExecute);
        }

        public static WebBrowserExtensionWpf GetInstance(WebBrowser webBrowser)
        {
            if (!WebBrowserExtensionWpfs.ContainsKey(webBrowser))
            {
                WebBrowserExtensionWpfs.Add(webBrowser, new WebBrowserExtensionWpf(webBrowser));
            }
            return WebBrowserExtensionWpfs[webBrowser];
        }

        public string RegisterCsCodeCallableFromJavascript(
            Func<bool> codeToExecute)
        {
            var htmlDocumentObjectForScripting = codeToExecute.Target;
            if (htmlDocumentObjectForScripting.GetType().GetCustomAttribute(typeof(ComVisibleAttribute)) == null)
            {
                throw new NotSupportedException("The class is not COM visible");
            }
            _webBrowser.ObjectForScripting = htmlDocumentObjectForScripting;
            var javascriptToExecute = $"window.external.{codeToExecute.Method.Name}();";
            return javascriptToExecute;
        }

        public void RunCsFromJavascript(Func<bool> codeToExecute)
        {
            var javascriptToExecute = RegisterCsCodeCallableFromJavascript(codeToExecute);
            InjectAndExecuteJavascript(javascriptToExecute);
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
                    if (eventName.StartsWith($"{WebBrowserExtensionMsHtmlDocument.Instance.DocumentEventPrefix}."))
                    {
                        WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToDocument(
                            _webBrowser.Document as HTMLDocument,
                            eventName.Split('.')[1], eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToControl(
                            _webBrowser.Document as HTMLDocument,
                            strings[0], strings[1], eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    index--;
                }
            }
        }
    }
}