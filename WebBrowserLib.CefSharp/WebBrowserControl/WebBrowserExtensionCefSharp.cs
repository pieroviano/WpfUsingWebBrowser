using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using WebBrowserLib.CefSharp.Utility;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;
using WebBrowserLib.WebBrowserControl;

namespace WebBrowserLib.CefSharp.WebBrowserControl
{
    public class WebBrowserExtensionCefSharp : BaseWebBrowserExtension<object, object>,
        IWebBrowserExtensionWithEvent<object, object, object>
    {
        private static readonly Dictionary<ChromiumWebBrowser, WebBrowserExtensionCefSharp> WebBrowserExtensionsCefSharp
            = new Dictionary<ChromiumWebBrowser, WebBrowserExtensionCefSharp>();

        private readonly ChromiumWebBrowser _webBrowser;
        private NavigatingInterceptorWpf _navigatingInterceptorWpf;


        private WebBrowserExtensionCefSharp(ChromiumWebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
            _webBrowser.LoadingStateChanged += WebBrowserLoadingStateChanged;
        }

        IBaseWebBrowserExtension<object, object> IWebBrowserExtensionWithEvent<object,
            object, object>.WebBrowserExtension =>
            this;

        public bool Enabled { get; set; }


        public void Navigate(string targetUrl)
        {
            if (!Enabled)
            {
                return;
            }
            _webBrowser.Address = targetUrl;
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
            var htmlDocument = _webBrowser;
            ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                .AttachEventHandlerToControl(htmlDocument, controlId, eventName,
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
            var htmlDocument = _webBrowser;
            ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                .AttachEventHandlerToDocument(htmlDocument, eventName,
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
            string scriptId = ExecuteJavascriptInCefSharp("var scripts=document.getElementsByTagName('script');" +
                                                          $"for(var i=0;i<scripts.length;i++)if (scripts[i].src=='{url}') return scripts[i].id;");
            while (GetGlobalVariable(
                       $"eval(document.all['{scriptId}'].readyState == 'loaded' || document.all['{scriptId}'].readyState == 'complete')") !=
                   true)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
            }
        }

        public dynamic FindElementByAttributeValue(string tagName, string attribute,
            string value)
        {
            if (!Enabled)
            {
                return null;
            }
            return FindElementsByAttributeValue(tagName,
                attribute, value)?.FirstOrDefault();
        }

        public IEnumerable<dynamic> FindElementsByAttributeValue(string tagName,
            string attribute, string value)
        {
            if (!Enabled)
            {
                return null;
            }
            var script = @"(function(tagName,attribute, value){
                var ret= new Array();
                var all = document.getElementsByTagName(tagName);
                for (var i = 0; i < all.length; i++)
                {
                    var str = all[i].getAttribute(attribute);
                    if (str != '' && str!=null)
                    {
                        if (str == value)
                        {
                            ret.push(all.item(i));
                        }
                    }
                }
                return ret;
            })" + $"({tagName},{attribute},{value});";
            return (IEnumerable<dynamic>) ExecuteJavascriptInCefSharp(script);
        }

        public string GetCurrentUrl()
        {
            if (!Enabled)
            {
                return null;
            }
            var url = ExecuteJavascriptInCefSharp("return (document.location.href);");
            return url;
        }

        public dynamic GetElementById(string controlId)
        {
            if (!Enabled)
            {
                return null;
            }
            return ExecuteJavascriptInCefSharp($"return (document.getElementById({controlId});");
        }

        public IEnumerable<dynamic> GetElementsByCssQuery(
            string cssQuery)
        {
            if (!Enabled)
            {
                return null;
            }
            return
                ExecuteJavascriptInCefSharp(
                    $"Array.prototype.slice.call(document.querySelectorAll('{cssQuery}'));");
        }

        public dynamic GetGlobalVariable(string variable)
        {
            if (!Enabled)
            {
                return null;
            }
            return ScriptHelper.GetGlobalVariable(variable, ExecuteJavascript);
        }

        public dynamic EvaluateExpression(string expression)
        {
            if (!Enabled)
            {
                return null;
            }
            return ScriptHelper.EvaluateExpression(expression, ExecuteJavascript);
        }

        public dynamic ExecuteJavascript(string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            return ExecuteJavascriptInCefSharp(
                javascriptToExecute);
        }

        public void RemoveEventHandlerToControl(string controlId, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var htmlDocument = _webBrowser;
            ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                .RemoveEventHandlerToControl(htmlDocument, controlId, eventName,
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
            var htmlDocument = _webBrowser;
            ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                .RemoveEventHandlerToDocument(htmlDocument, eventName,
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
            _navigatingInterceptorWpf =
                new NavigatingInterceptorWpf(getCustomEventHandler, _webBrowser, DocumentEventPrefix);
            if (getCustomEventHandler() != null)
            {
                setCustomEventHandler(null);
            }
        }

        public void AddScriptsElements(string scriptUrl)
        {
            GetScriptsElementsInCefSharp(scriptUrl);
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

            ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                .AttachEventHandlerToControl(_webBrowser,
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

            ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                .AttachEventHandlerToDocument(
                    _webBrowser,
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

        public void AddJQueryScript(string url)
        {
            throw new NotImplementedException();
        }

        public void AddScriptElement(string scriptBody)
        {
            throw new NotImplementedException();
        }

        private void WebBrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                DocumentReady?.Invoke(this, e);
            }
            else
            {
                _navigatingInterceptorWpf?
                    .WebBrowserOnNavigating(sender, e);
            }
        }

        public void AddJavascriptByUrl(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            GetScriptsElementsInCefSharp(scriptUrl);
        }

        private void GetScriptsElementsInCefSharp(string scriptUrl)
        {
            if (!Enabled)
            {
                return;
            }
            ExecuteJavascriptInCefSharp("var script=document.createElement('script');" +
                                        "script.setAttribute('type','text/javascript');" +
                                        $"script.setAttribute('src','{scriptUrl}');" +
                                        "document.getElementsByTagName('head')[0].appendChild(script);");
            while ((string)GetGlobalVariable("document.readyState") != "complete")
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
            }
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

        public override dynamic ExecuteJavascript(object htmlDocument, string javascriptToExecute)
        {
            return ExecuteJavascriptInCefSharp(javascriptToExecute);
        }

        public override dynamic InvokeJavascript(object htmlDocument, string javascriptToExecute)
        {
            return ExecuteJavascriptInCefSharp(javascriptToExecute);
        }

        private dynamic ExecuteJavascriptInCefSharp(string script)
        {
            if (!Enabled)
            {
                return null;
            }
            var task = _webBrowser.EvaluateScriptAsync(
                script);
            task.Wait();
            return task.Result;
        }

        public override object GetElementById(object htmlDocument, string controlId)
        {
            return GetElementById(controlId);
        }

        public static WebBrowserExtensionCefSharp GetInstance(ChromiumWebBrowser webBrowser)
        {
            if (!WebBrowserExtensionsCefSharp.ContainsKey(webBrowser))
            {
                WebBrowserExtensionsCefSharp.Add(webBrowser, new WebBrowserExtensionCefSharp(webBrowser));
            }
            return WebBrowserExtensionsCefSharp[webBrowser];
        }

        public string RegisterCsCodeCallableFromJavascript(
            Func<bool> codeToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlDocumentObjectForScripting = codeToExecute.Target;
            _webBrowser.RegisterAsyncJsObject("htmlDocumentObjectForScripting",
                htmlDocumentObjectForScripting); //Standard object rego
            var methodName = codeToExecute.Method.Name;
            methodName = methodName.Substring(0, 1).ToLower() + methodName.Substring(1);
            var javascriptToExecute = $"htmlDocumentObjectForScripting.{methodName}();";
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
            private readonly string _documentEventPrefix;
            private readonly Func<CustomWebBrowserControlEventHandler> _getCustomEventHandler;
            private readonly ChromiumWebBrowser _webBrowser;

            public NavigatingInterceptorWpf(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                ChromiumWebBrowser webBrowser, string documentEventPrefix)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowser = webBrowser;
                _documentEventPrefix = documentEventPrefix;
            }

            public void WebBrowserOnNavigating(object sender, LoadingStateChangedEventArgs e)
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
                    if (eventName.StartsWith($"{_documentEventPrefix}."))
                    {
                        ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                            .RemoveEventHandlerToDocument(
                                _webBrowser,
                                eventName.Split('.')[1], eventHandlerDelegate.Item3,
                                _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        ((IWebBrowserExtensionWithEvent<object, object, object>) this).WebBrowserExtension
                            .RemoveEventHandlerToControl(
                                _webBrowser,
                                strings[0], strings[1], eventHandlerDelegate.Item3,
                                _getCustomEventHandler);
                    }
                    index--;
                }
            }
        }
    }
}