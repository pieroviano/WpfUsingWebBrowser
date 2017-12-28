using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using mshtml;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;
using WebBrowserLib.MsHtml.WebBrowserControl;

namespace WebBrowserLib.WinForms.WebBrowserControl
{
    public class WebBrowserExtensionWinForm : IWebBrowserExtensionWithEvent<IHTMLElement>
    {
        private static readonly Dictionary<WebBrowser, WebBrowserExtensionWinForm> WebBrowserExtensionWinForms =
            new Dictionary<WebBrowser, WebBrowserExtensionWinForm>();

        private readonly WebBrowser _webBrowser;

        private WebBrowserExtensionWinForm(WebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
            _webBrowser.DocumentCompleted += (sender, args) => { DocumentReady?.Invoke(this, args); };
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
            WebBrowserExtensionMsHtmlDocument.Instance.AddJQueryElement(
                _webBrowser.Document?.GetElementsByTagName("head")[0].DomElement as HTMLHeadElement);
        }

        public void AddScriptElement(string scriptBody)
        {
            WebBrowserExtensionMsHtmlDocument.Instance.AddScriptElement(
                _webBrowser.Document?.GetElementsByTagName("head")[0].DomElement as HTMLHeadElement, scriptBody);
        }

        public void Navigate(string targetUrl)
        {
            _webBrowser.Navigate(targetUrl);
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            return WebBrowserExtensionMsHtmlDocument.Instance.ExecuteJavascript(
                _webBrowser.Document?.DomDocument as HTMLDocument, javascriptToExecute);
        }

        public void AttachEventHandlerToControl(string controlId, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            if (_webBrowser.Document != null)
            {
                var htmlDocument = _webBrowser.Document.DomDocument as HTMLDocument;
                WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToControl(htmlDocument, controlId,
                    eventName,
                    firstArgument, customEventDelegate,
                    functionHash, getCustomEventHandler, setCustomEventHandler, removeHandlers);
            }
        }

        public void AttachEventHandlerToDocument(string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (_webBrowser.Document != null)
            {
                var htmlDocument = _webBrowser.Document.DomDocument as HTMLDocument;
                WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToDocument(htmlDocument, eventName,
                    firstArgument,
                    customEventDelegate, functionHash,
                    getCustomEventHandler, setCustomEventHandler);
            }
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
            RemoveEventHandlerToDocument("oncontextmenu", functionHash, getCustomEventHandler);
        }

        public dynamic FindElementByAttributeValue(string tagName, string attribute,
            string value)
        {
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            if (htmlDocument != null)
            {
                return WebBrowserExtensionMsHtmlDocument.Instance.FindElementByAttributeValue(htmlDocument, tagName,
                    attribute,
                    value);
            }
            return null;
        }

        public IEnumerable<IHTMLElement> FindElementsByAttributeValue(string tagName,
            string attribute, string value)
        {
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            if (htmlDocument != null)
            {
                return WebBrowserExtensionMsHtmlDocument.Instance.FindElementsByAttributeValue(htmlDocument, tagName,
                    attribute,
                    value);
            }
            return null;
        }

        public IHTMLElement GetElementById(string controlId)
        {
            if (_webBrowser.Document != null)
            {
                var htmlDocument = _webBrowser.Document.DomDocument as HTMLDocument;
                return WebBrowserExtensionMsHtmlDocument.Instance.GetElementById(htmlDocument, controlId);
            }
            return null;
        }

        public IEnumerable<IHTMLElement> GetElementsByCssQuery(
            string cssQuery)
        {
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            return WebBrowserExtensionMsHtmlDocument.Instance.GetElementsByCssQuery(htmlDocument, cssQuery);
        }

        public object GetGlobalVariable(string variable)
        {
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
            if (_webBrowser.Document != null)
            {
                var htmlDocument = _webBrowser.Document.DomDocument as HTMLDocument;
                return WebBrowserExtensionMsHtmlDocument.Instance.ExecuteJavascript(htmlDocument,
                    javascriptToExecute);
            }
            return null;
        }

        public void AddJavascriptByUrl(string scriptUrl)
        {
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            var htmlHeadElement = htmlDocument.getElementsByName("head").item(0) as HTMLHeadElement;
            WebBrowserExtensionMsHtmlDocument.Instance.AddScriptByUrl(htmlHeadElement, scriptUrl);
        }

        public void RemoveEventHandlerToControl(string controlId, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (_webBrowser.Document != null)
            {
                var htmlDocument = _webBrowser.Document.DomDocument as HTMLDocument;
                WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToControl(htmlDocument, controlId,
                    eventName,
                    functionHash,
                    getCustomEventHandler);
            }
        }

        public void RemoveEventHandlerToDocument(string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (_webBrowser.Document != null)
            {
                var htmlDocument = _webBrowser.Document.DomDocument as HTMLDocument;
                WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToDocument(htmlDocument, eventName,
                    functionHash, getCustomEventHandler);
            }
        }

        public void RemoveHandlersOnNavigating(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            _webBrowser.Navigating += new NavigatingInterceptorWinForm(getCustomEventHandler, _webBrowser)
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
            var codeToExecuteClass = new Utility.CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();

            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToControl(
                _webBrowser.Document?.DomDocument as HTMLDocument,
                controlId,
                eventName, codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void AttachCustomFunctionOnDocument(string eventName, Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var codeToExecuteClass = new Utility.CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();

            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToDocument(
                _webBrowser.Document?.DomDocument as HTMLDocument,
                eventName,
                codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void AddJQueryElement(HtmlElement head)
        {
            WebBrowserExtensionMsHtmlDocument.Instance.AddJQueryElement(head.DomElement as HTMLHeadElement);
        }

        public void AddScriptElement(HtmlElement head, string scriptBody)
        {
            WebBrowserExtensionMsHtmlDocument.Instance.AddScriptElement(head.DomElement as HTMLHeadElement, scriptBody);
        }

        public void AttachCustomFunctionOnDocument(WebBrowser webBrowser, string eventName, Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            var codeToExecuteClass = new Utility.CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();
            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToDocument(
                (HTMLDocument) webBrowser.Document?.DomDocument,
                eventName, codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void CauseCsBreakpoint(ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
            {
                comVisibleClass = new ComVisibleClass();
            }
            RunCsFromJavascript(comVisibleClass.CodeToExecute);
        }

        public static WebBrowserExtensionWinForm GetInstance(WebBrowser webBrowser)
        {
            if (!WebBrowserExtensionWinForms.ContainsKey(webBrowser))
            {
                WebBrowserExtensionWinForms.Add(webBrowser, new WebBrowserExtensionWinForm(webBrowser));
            }
            return WebBrowserExtensionWinForms[webBrowser];
        }

        public string RegisterCsCodeCallableFromJavascript(ref ComVisibleClass comVisibleClass)
        {
            if (comVisibleClass == null)
            {
                comVisibleClass = new ComVisibleClass();
            }
            return RegisterCsCodeCallableFromJavascript(comVisibleClass.CodeToExecute);
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

        public bool RunCsFromJavascript(Func<bool> codeToExecute)
        {
            var javascriptToExecute = RegisterCsCodeCallableFromJavascript(codeToExecute);
            return Convert.ToBoolean(InjectAndExecuteJavascript(javascriptToExecute));
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
                    if (eventName.StartsWith($"{WebBrowserExtensionMsHtmlDocument.Instance.DocumentEventPrefix}."))
                    {
                        WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToDocument(
                            _webBrowser.Document?.DomDocument as HTMLDocument, eventName.Split('.')[1],
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        WebBrowserExtensionMsHtmlDocument.Instance.RemoveEventHandlerToControl(
                            _webBrowser.Document?.DomDocument as HTMLDocument, strings[0], strings[1],
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                }
            }
        }
    }
}