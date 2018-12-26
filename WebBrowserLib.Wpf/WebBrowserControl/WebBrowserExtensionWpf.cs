using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using mshtml;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;
using WebBrowserLib.MsHtml.WebBrowserControl;
using WebBrowserLib.Wpf.Utility;

namespace WebBrowserLib.Wpf.WebBrowserControl
{
    public partial class WebBrowserExtensionWpf : IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>
    {
        private static readonly Dictionary<WebBrowser, WebBrowserExtensionWpf> WebBrowserExtensionWpfs =
            new Dictionary<WebBrowser, WebBrowserExtensionWpf>();

        private WebBrowserExtensionWpf(WebBrowser webBrowser)
        {
            Browser = webBrowser;
            Browser.Loaded += (sender, args) => { DocumentReady?.Invoke(this, args); };
        }

        public WebBrowser Browser { get; }

        public string DocumentEventPrefix
        {
            get => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.DocumentEventPrefix;
            set => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.DocumentEventPrefix = value;
        }

        IBaseWebBrowserExtension<HTMLDocument, IHTMLElement> IWebBrowserExtensionWithEvent<IHTMLElement,
            HTMLDocument, IHTMLElement>.WebBrowserExtension => WebBrowserExtensionMsHtmlDocument.Instance;


        public bool Enabled
        {
            get => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.Enabled;
            set => ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.Enabled = value;
        }

        public void AddScriptsElements(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            var htmlDocument = Browser.Document as HTMLDocument;

            EnsureScriptIsInCache(scriptUrl);
            var scriptsElements = ((WebBrowserExtensionMsHtmlDocument) (
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension).GetScriptsElements(htmlDocument, scriptUrl);
            var item = htmlDocument?.getElementsByTagName("head").item(null, 0) as HTMLHeadElement;
            foreach (var scriptsElement in scriptsElements)
            {
                item?.appendChild(scriptsElement as IHTMLDOMNode);
            }
        }

        public void AddScriptElement(string scriptBody)
        {
            if (!Enabled)
            {
                return;
            }
            var head = (HTMLHeadElement) ((HTMLDocument) Browser.Document)?.getElementsByTagName("head").item(0);
            var scriptEl = (head?.ownerDocument as HTMLDocument)?.createElement("script") as HTMLScriptElement;
            if (scriptEl != null)
            {
                scriptEl.innerHTML = scriptBody;

                head.appendChild((IHTMLDOMNode) scriptEl);
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
            var htmlDocument = Browser.Document as HTMLDocument;
            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.AttachEventHandlerToControl(htmlDocument, controlId, eventName,
                    firstArgument,
                    customEventDelegate,
                    functionHash, getCustomEventHandler, setCustomEventHandler, removeHandlers);
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
            var htmlDocument = Browser.Document as HTMLDocument;
            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.AttachEventHandlerToDocument(htmlDocument, eventName,
                    firstArgument,
                    customEventDelegate, functionHash,
                    getCustomEventHandler, setCustomEventHandler);
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
            RemoveEventHandlerToDocument(
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public void EnsureScriptIsInCache(string url)
        {
            var webBrowser = new WebBrowser();
            var formattableString = $"<html><head><script type='text/javascript' src='{url}'></script></head></html>";
            var combine = Path.Combine(Environment.GetEnvironmentVariable("TEMP"),Guid.NewGuid()+ ".htm");
            var streamWriter = new StreamWriter(File.OpenWrite(combine));
            streamWriter.Write(formattableString);
            streamWriter.Close();
            webBrowser.Navigate(combine);
            var webBrowserExtensionMsHtmlDocument = ((WebBrowserExtensionMsHtmlDocument) (
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension);
            var expression = webBrowserExtensionMsHtmlDocument.EvaluateExpression(Browser.Document as HTMLDocument, "document.readyState=='complete'");
            while (expression as string!="true")
            {
                expression = webBrowserExtensionMsHtmlDocument.EvaluateExpression(Browser.Document as HTMLDocument, "eval(document.readyState=='complete')");
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
            }
            webBrowser.Dispose();
            File.Delete(combine);
        }

        public dynamic FindElementByAttributeValue(string tagName, string attribute,
            string value)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;

            return ((WebBrowserExtensionMsHtmlDocument) (
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension).FindElementByAttributeValue(htmlDocument, tagName,
                attribute, value);
        }

        public IEnumerable<IHTMLElement> FindElementsByAttributeValue(string tagName,
            string attribute, string value)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            return ((WebBrowserExtensionMsHtmlDocument) (
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension).FindElementsByAttributeValue(htmlDocument, tagName,
                attribute, value);
        }

        public string GetCurrentUrl()
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            return ((WebBrowserExtensionMsHtmlDocument) (
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension).GetCurrentUrl(htmlDocument);
        }

        public IHTMLElement GetElementById(string controlId)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            return ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.GetElementById(htmlDocument, controlId);
        }

        public IEnumerable<IHTMLElement> GetElementsByCssQuery(
            string cssQuery)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            return ((WebBrowserExtensionMsHtmlDocument) (
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension).GetElementsByCssQuery(htmlDocument, cssQuery);
        }

        public dynamic GetGlobalVariable(string variable)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            return ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).GetGlobalVariable(htmlDocument, variable);
        }

        public dynamic EvaluateExpression(string variable)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            return ((WebBrowserExtensionMsHtmlDocument)(
                    (IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)this)
                .WebBrowserExtension).EvaluateExpression(htmlDocument, variable);
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            return ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.ExecuteJavascript(htmlDocument,
                    javascriptToExecute);
        }

        public void RemoveEventHandlerToControl(string controlId, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.RemoveEventHandlerToControl(htmlDocument, controlId, eventName,
                    functionHash,
                    getCustomEventHandler);
        }

        public void RemoveEventHandlerToDocument(string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var htmlDocument = Browser.Document as HTMLDocument;
            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.RemoveEventHandlerToDocument(htmlDocument, eventName,
                    functionHash, getCustomEventHandler);
        }

        public void RemoveHandlersOnNavigating(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Browser.Navigating += new NavigatingInterceptorWpf(getCustomEventHandler, this)
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

            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.AttachEventHandlerToControl(Browser.Document as HTMLDocument,
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

            ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>) this)
                .WebBrowserExtension.AttachEventHandlerToDocument(
                    Browser.Document as HTMLDocument,
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

        public void AddJQueryScript(string url = @"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js")
        {
            if (!Enabled)
            {
                return;
            }
            AddScriptsElements(url);
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

        public void RunCsFromJavascript(Func<bool> codeToExecute)
        {
            if (!Enabled)
            {
                return;
            }
            var javascriptToExecute = RegisterCsCodeCallableFromJavascript(codeToExecute);
            ExecuteJavascript(javascriptToExecute);
        }

        private class NavigatingInterceptorWpf
        {
            private readonly Func<CustomWebBrowserControlEventHandler> _getCustomEventHandler;
            private readonly WebBrowserExtensionWpf _webBrowserExtensionWpf;

            public NavigatingInterceptorWpf(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                WebBrowserExtensionWpf webBrowserExtensionWpf)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowserExtensionWpf = webBrowserExtensionWpf;
            }

            [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
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
                    var documentEventPrefix = _webBrowserExtensionWpf.DocumentEventPrefix;
                    if (eventName.StartsWith(
                        $"{documentEventPrefix}.")
                    )
                    {
                        ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)
                            _webBrowserExtensionWpf
                        ).WebBrowserExtension.RemoveEventHandlerToDocument(
                            _webBrowserExtensionWpf.Browser.Document as HTMLDocument,
                            eventName.Split('.')[1], eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        ((IWebBrowserExtensionWithEvent<IHTMLElement, HTMLDocument, IHTMLElement>)
                            _webBrowserExtensionWpf
                        ).WebBrowserExtension.RemoveEventHandlerToControl(
                            _webBrowserExtensionWpf.Browser.Document as HTMLDocument,
                            strings[0], strings[1], eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    index--;
                }
            }
        }
    }
}