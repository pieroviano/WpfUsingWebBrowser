using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
    public class WebBrowserExtensionWinForm : IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument,
        IHTMLElement>
    {
        private static readonly Dictionary<WebBrowser, WebBrowserExtensionWinForm> WebBrowserExtensionWinForms =
            new Dictionary<WebBrowser, WebBrowserExtensionWinForm>();

        private WebBrowserExtensionWinForm(WebBrowser webBrowser)
        {
            Browser = webBrowser;
            Browser.DocumentCompleted += (sender, args) => { DocumentReady?.Invoke(this, args); };
        }

        public WebBrowser Browser { get; }

        IBaseWebBrowserExtension<HTMLDocument, IHTMLElement>
            IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>.WebBrowserExtension =>
            WebBrowserExtensionMsHtmlDocument.Instance;

        public string DocumentEventPrefix
        {
            get => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension.DocumentEventPrefix;
            set => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension.DocumentEventPrefix = value;
        }

        public bool Enabled
        {
            get => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension.Enabled;
            set => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension.Enabled = value;
        }

        public void AddJQueryScript(string url=@"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js")
        {
            if (!Enabled)
            {
                return;
            }
            AddScriptsElements(url);
        }

        public void EnsureScriptIsInCache(string url)
        {
            var webBrowser = new WebBrowser();
            webBrowser.DocumentText = $"<html><head><script type='text/javascript' src='{url}'></script></head></html>";
            while (webBrowser.ReadyState != WebBrowserReadyState.Complete)
            {
                Application.DoEvents();
            }
            webBrowser.Dispose();
        }

        public void AddScriptsElements(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            EnsureScriptIsInCache(scriptUrl);
            var scriptsByUrl = ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).GetScriptsElements(htmlDocument, scriptUrl);
            var htmlHeadElement = (Browser.Document?.GetElementsByTagName("head")[0].DomElement as HTMLHeadElement);
            foreach (var htmlElement in scriptsByUrl)
            {
                htmlHeadElement?
                    .appendChild(htmlElement as IHTMLDOMNode);
            }
        }

        public void AddScriptElement(string scriptBody)
        {
            if (!Enabled)
            {
                return;
            }
            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            var htmlHeadElement = Browser.Document?.GetElementsByTagName("head")[0].DomElement as HTMLHeadElement;
            var scriptsElements = ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).CreateScriptElement(
                htmlDocument, scriptBody);
            foreach (var htmlElement in scriptsElements)
            {
                var scriptElement = (IHTMLDOMNode)htmlElement;
                htmlHeadElement?.appendChild(scriptElement);
            }
        }

        public void Navigate(string targetUrl)
        {
            if (!Enabled)
            {
                return;
            }
            Browser.Navigate(targetUrl);
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
            if (Browser.Document != null)
            {
                var htmlDocument = Browser.Document.DomDocument as HTMLDocument;
                ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension.AttachEventHandlerToControl(htmlDocument, controlId,
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
            if (Browser.Document != null)
            {
                var htmlDocument = Browser.Document.DomDocument as HTMLDocument;
                ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension.AttachEventHandlerToDocument(htmlDocument, eventName,
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
            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            if (htmlDocument != null)
            {
                return ((WebBrowserExtensionMsHtmlDocument)(
                        (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension).FindElementByAttributeValue(htmlDocument, tagName,
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
            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            if (htmlDocument != null)
            {
                return ((WebBrowserExtensionMsHtmlDocument)(
                        (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension).FindElementsByAttributeValue(htmlDocument, tagName,
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
            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            return ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).GetCurrentUrl(htmlDocument);
        }

        public IHTMLElement GetElementById(string controlId)
        {
            if (!Enabled)
            {
                return null;
            }
            if (Browser.Document != null)
            {
                var htmlDocument = Browser.Document.DomDocument as HTMLDocument;
                return ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension.GetElementById(htmlDocument, controlId);
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
            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            return ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).GetElementsByCssQuery(htmlDocument, cssQuery);
        }

        public dynamic GetGlobalVariable(string variable)
        {
            if (!Enabled)
            {
                return null;
            }

            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            var globalVariable = ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).GetGlobalVariable(htmlDocument, variable);
            return globalVariable;
        }

        public dynamic EvaluateExpression(string variable)
        {
            if (!Enabled)
            {
                return null;
            }

            var htmlDocument = Browser.Document?.DomDocument as HTMLDocument;
            var globalVariable = ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).EvaluateExpression(htmlDocument, variable);
            return globalVariable;
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            if (Browser.Document != null)
            {
                var htmlDocument = Browser.Document.DomDocument as HTMLDocument;
                return ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension.ExecuteJavascript(htmlDocument,
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
            if (Browser.Document != null)
            {
                var htmlDocument = Browser.Document.DomDocument as HTMLDocument;
                ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension.RemoveEventHandlerToControl(htmlDocument, controlId,
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
            if (Browser.Document != null)
            {
                var htmlDocument = Browser.Document.DomDocument as HTMLDocument;
                ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension.RemoveEventHandlerToDocument(htmlDocument, eventName,
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
            Browser.Navigating += new NavigatingInterceptorWinForm(getCustomEventHandler, this)
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

            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension.AttachEventHandlerToControl(
                    Browser.Document?.DomDocument as HTMLDocument,
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

            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension.AttachEventHandlerToDocument(
                    Browser.Document?.DomDocument as HTMLDocument,
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
            var htmlHeadElement = Browser.Document?.GetElementsByTagName("head")[0].DomElement as HTMLHeadElement;
            if (htmlHeadElement != null)
            {
                var scriptsElements = ((WebBrowserExtensionMsHtmlDocument)(
                        (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                    .WebBrowserExtension).GetScriptsElements(Browser.Document.DomDocument as HTMLDocument, scriptUrl);
                foreach (var htmlElement in scriptsElements)
                {
                    var scriptElement = (IHTMLDOMNode)htmlElement;
                    htmlHeadElement.appendChild(scriptElement);
                }
            }
        }

        public void CreateScriptElement(HTMLDocument document, string scriptBody)
        {
            if (!Enabled)
            {
                return;
            }
            ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).CreateScriptElement(document, scriptBody);
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
            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension.AttachEventHandlerToDocument(
                    (HTMLDocument)webBrowser.Document?.DomDocument,
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
            Browser.ObjectForScripting = htmlDocumentObjectForScripting;
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
            private readonly WebBrowserExtensionWinForm _webBrowserExtensionWinForm;

            public NavigatingInterceptorWinForm(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                WebBrowserExtensionWinForm webBrowserExtensionWinForm)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowserExtensionWinForm = webBrowserExtensionWinForm;
            }


            [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
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
                    var documentEventPrefix =
                        (_webBrowserExtensionWinForm as IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument,
                            IHTMLElement>)?.WebBrowserExtension.DocumentEventPrefix;
                    if (eventName.StartsWith(
                        $"{documentEventPrefix}."))
                    {
                        (_webBrowserExtensionWinForm as IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument,
                                IHTMLElement>)?.WebBrowserExtension.RemoveEventHandlerToDocument(
                            _webBrowserExtensionWinForm.Browser.Document?.DomDocument as HTMLDocument,
                            eventName.Split('.')[1],
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        (_webBrowserExtensionWinForm as IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument,
                                IHTMLElement
                            >
                        )?.WebBrowserExtension.RemoveEventHandlerToControl(
                            _webBrowserExtensionWinForm.Browser.Document?.DomDocument as HTMLDocument, strings[0],
                            strings[1],
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                }
            }
        }
    }
}