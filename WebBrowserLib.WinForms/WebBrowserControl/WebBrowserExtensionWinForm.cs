using System;
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
using WebBrowserLib.WinForms.Utility;

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

        public void AddJQueryElement()
        {
            if (!Enabled)
            {
                return;
            }
            WebBrowserExtensionMsHtmlDocument.Instance.AddJQueryElement(
                _webBrowser.Document?.GetElementsByTagName("head")[0].DomElement as HTMLHeadElement);
        }

        public void AddScriptElement(string scriptBody)
        {
            if (!Enabled)
            {
                return;
            }
            WebBrowserExtensionMsHtmlDocument.Instance.AddScriptElement(
                _webBrowser.Document?.GetElementsByTagName("head")[0].DomElement as HTMLHeadElement, scriptBody);
        }

        public void Navigate(string targetUrl)
        {
            if (!Enabled)
            {
                return;
            }
            _webBrowser.Navigate(targetUrl);
        }

        public void AttachEventHandlerToControl(string controlId, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
            var cleanHandlers = ScriptHelper.PrepareCleanHandlers(eventNames);
            if (!removeHandlers)
            {
                if (!string.IsNullOrEmpty(cleanHandlers))
                {
                    ExecuteJavascript(cleanHandlers);
                }
                return;
            }
            ExecuteJavascript(ScriptHelper.GetJavascriptToExecuteToRemoveHandlers(controlId, cleanHandlers));
        }

        public void DisableEventOnControl(string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToControl(controlId, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableEventOnDocument(string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableOnContextMenuToDocument(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument("oncontextmenu", functionHash, getCustomEventHandler);
        }

        public dynamic FindElementByAttributeValue(string tagName, string attribute,
            string value)
        {
            if (!Enabled)
            {
                return null;
            }
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
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            if (htmlDocument != null)
            {
                return WebBrowserExtensionMsHtmlDocument.Instance.FindElementsByAttributeValue(htmlDocument, tagName,
                    attribute,
                    value);
            }
            return null;
        }

        public string GetCurrentUrl()
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            return WebBrowserExtensionMsHtmlDocument.Instance.GetCurrentUrl(htmlDocument);
        }

        public IHTMLElement GetElementById(string controlId)
        {
            if (!Enabled)
            {
                return null;
            }
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
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            return WebBrowserExtensionMsHtmlDocument.Instance.GetElementsByCssQuery(htmlDocument, cssQuery);
        }

        public dynamic GetGlobalVariable(string variable)
        {
            if (!Enabled)
            {
                return null;
            }
            return ScriptHelper.GetGlobalVariable(variable, ExecuteJavascript);
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            if (_webBrowser.Document != null)
            {
                var htmlDocument = _webBrowser.Document.DomDocument as HTMLDocument;
                return WebBrowserExtensionMsHtmlDocument.Instance.ExecuteJavascript(htmlDocument,
                    javascriptToExecute);
            }
            return null;
        }

        public void RemoveEventHandlerToControl(string controlId, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
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
            if (!Enabled)
            {
                return;
            }
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
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
            if (!Enabled)
            {
                return;
            }
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();

            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToDocument(
                _webBrowser.Document?.DomDocument as HTMLDocument,
                eventName,
                codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public string RegisterCsCodeCallableFromJavascript(ref ComVisibleClass comVisibleClass)
        {
            if (!Enabled)
            {
                return null;
            }
            if (comVisibleClass == null)
            {
                comVisibleClass = new ComVisibleClass();
            }
            return RegisterCsCodeCallableFromJavascript(comVisibleClass.CodeToExecute);
        }

        public void AddJavascriptByUrl(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            var htmlDocument = _webBrowser.Document?.DomDocument as HTMLDocument;
            var htmlHeadElement = htmlDocument?.getElementsByName("head").item(0) as HTMLHeadElement;
            if (htmlHeadElement != null)
            {
                WebBrowserExtensionMsHtmlDocument.Instance.AddScriptByUrl(htmlHeadElement, scriptUrl);
            }
        }

        public void AddJQueryElement(HtmlElement head)
        {
            if (!Enabled)
            {
                return;
            }
            WebBrowserExtensionMsHtmlDocument.Instance.AddJQueryElement(head.DomElement as HTMLHeadElement);
        }

        public void AddScriptElement(HtmlElement head, string scriptBody)
        {
            if (!Enabled)
            {
                return;
            }
            WebBrowserExtensionMsHtmlDocument.Instance.AddScriptElement(head.DomElement as HTMLHeadElement, scriptBody);
        }

        public void AttachCustomFunctionOnDocument(WebBrowser webBrowser, string eventName, Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var codeToExecuteClass = new CodeToExecuteClass(codeToExecute);
            functionHash += new Func<bool>(codeToExecuteClass.CustomEventDelegate).GetFullNameHashCode();
            WebBrowserExtensionMsHtmlDocument.Instance.AttachEventHandlerToDocument(
                (HTMLDocument) webBrowser.Document?.DomDocument,
                eventName, codeToExecuteClass,
                codeToExecuteClass.CustomEventDelegate, functionHash,
                getCustomEventHandler, setCustomEventHandler);
        }

        public void CauseCsBreakpoint(ref ComVisibleClass comVisibleClass)
        {
            if (!Enabled)
            {
                return;
            }
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

        public string RegisterCsCodeCallableFromJavascript(
            Func<bool> codeToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
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
            if (!Enabled)
            {
                return false;
            }
            var javascriptToExecute = RegisterCsCodeCallableFromJavascript(codeToExecute);
            return Convert.ToBoolean(ExecuteJavascript(javascriptToExecute));
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