using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using mshtml;
using WebBrowserLib.Extensions;
using WebBrowserLib.WebBrowserControl.Helpers;
using WebBrowserLib.WebBrowserControl.Interfaces;

namespace WebBrowserLib.WebBrowserControl
{
    public partial class WebBrowserExtension : IWebBrowserExtension
    {
        public string
            DocumentEventPrefix = "document";

        private WebBrowserExtension()
        {
        }

        public bool Enabled { get; set; } = true;
        public bool JavascriptInjectionEnabled { get; set; } = true;

        public static WebBrowserExtension Instance { get; } = new WebBrowserExtension();

        public void AddJQueryElement(HTMLHeadElement head)
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

        public void AddScriptElement(HTMLHeadElement head, string scriptBody)
        {
            var scriptEl = (head.ownerDocument as HTMLDocument)?.createElement("script") as HTMLScriptElement;
            if (scriptEl != null)
            {
                scriptEl.innerHTML = scriptBody;

                head.appendChild((IHTMLDOMNode) scriptEl);
            }
        }

        public void AttachEventHandlerToControl(HTMLDocument htmlDocument, string controlId,
            string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            if (!Enabled)
            {
                return;
            }
            DetachEventHandlersFromControl(htmlDocument, controlId, removeHandlers, eventName);

            if (htmlDocument != null)
            {
                var element = GetElementById(htmlDocument, controlId);
                var formattableString = $"{controlId}.{eventName}";
                var customWebBrowserControlEventHandler = getCustomEventHandler();
                if (customWebBrowserControlEventHandler == null)
                {
                    var customEventDelegates = new List<Tuple<string, Delegate, int>>();
                    setCustomEventHandler(new CustomWebBrowserControlEventHandler(customEventDelegates));
                    customWebBrowserControlEventHandler = getCustomEventHandler();
                }
                if (element != null)
                {
                    Delegate @delegate;
                    var eventInfo = EventHandlerAttacher.GetEventInfo(element, eventName);
                    if (eventInfo != null)
                    {
                        var eventInfoEventHandlerType = eventInfo.EventHandlerType;
                        if (!customWebBrowserControlEventHandler.EventHandlerIsAttached(formattableString, functionHash,
                            eventInfoEventHandlerType, firstArgument, customEventDelegate, out @delegate))
                        {
                            eventInfo.AddEventHandler(element, @delegate);
                            customWebBrowserControlEventHandler.TrackHandler(formattableString, @delegate,
                                functionHash);
                        }
                    }
                }
            }
        }

        public void AttachEventHandlerToDocument(HTMLDocument htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var document = (HTMLDocumentEvents_Event) htmlDocument;
            if (document != null)
            {
                var formattableString = $"{DocumentEventPrefix}.{eventName}";
                var customWebBrowserControlEventHandler = getCustomEventHandler();
                if (customWebBrowserControlEventHandler == null)
                {
                    var customEventDelegates = new List<Tuple<string, Delegate, int>>();
                    setCustomEventHandler(new CustomWebBrowserControlEventHandler(customEventDelegates));
                    customWebBrowserControlEventHandler = getCustomEventHandler();
                }
                Delegate @delegate;
                var eventInfo = EventHandlerAttacher.GetEventInfo(document, eventName);
                if (eventInfo != null)
                {
                    var eventInfoEventHandlerType = eventInfo.EventHandlerType;
                    if (!customWebBrowserControlEventHandler.EventHandlerIsAttached(formattableString, functionHash,
                        eventInfoEventHandlerType, firstArgument, customEventDelegate, out @delegate))
                    {
                        eventInfo.AddEventHandler(document, @delegate);
                        customWebBrowserControlEventHandler.TrackHandler(formattableString, @delegate, functionHash);
                    }
                }
            }
        }

        public void DetachEventHandlersFromControl(HTMLDocument htmlDocument, string controlId,
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
                    InjectAndExecuteJavascript(htmlDocument, cleanHandlers);
                }
                return;
            }
            var javascriptToExecuteToRemoveHandlers =
                ScriptHelper.GetJavascriptToExecuteToRemoveHandlers(controlId, cleanHandlers);
            if (!string.IsNullOrEmpty(javascriptToExecuteToRemoveHandlers))
            {
                InjectAndExecuteJavascript(htmlDocument, javascriptToExecuteToRemoveHandlers);
            }
        }

        public void DisableEventOnControl(HTMLDocument browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToControl(browser, controlId, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableEventOnDocument(HTMLDocument browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(browser, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableOnContextMenuOnDocument(HTMLDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(htmlDocument, "oncontextmenu",
                null, customEventDelegate, functionHash, getControlEventHandler,
                setControlEventHandler);
        }

        public void EnableEventOnControl(HTMLDocument browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToControl(browser, controlId, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableEventOnDocument(HTMLDocument browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(browser, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableOnContextMenuToDocument(HTMLDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(htmlDocument,
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public dynamic ExecuteJavascript(HTMLDocument htmlDocument, string javascriptToExecute)
        {
            return htmlDocument.parentWindow.execScript(javascriptToExecute);
        }

        public dynamic FindElementByAttributeValue(HTMLDocument htmlDocument, string tagName,
            string attribute, string value)
        {
            if (!Enabled)
            {
                return null;
            }
            var all = htmlDocument.getElementsByTagName(tagName);
            for (var i = 0; i < all.length; i++)
            {
                if (all.item(i).GetAttribute(attribute) == value)
                {
                    return all.item(i);
                }
            }
            return null;
        }

        public List<dynamic> FindElementsByAttributeValue(HTMLDocument htmlDocument, string tagName,
            string attribute, string value)
        {
            var ret = new List<dynamic>();
            if (!Enabled)
            {
                return ret;
            }
            var all = htmlDocument.getElementsByTagName(tagName);
            for (var i = 0; i < all.length; i++)
            {
                var str = all.item(i).GetAttribute(attribute);
                if (str != "")
                {
                    if (str == value)
                    {
                        ret.Add(all.item(i));
                    }
                }
            }
            return ret;
        }

        public IHTMLElement GetElementById(HTMLDocument htmlDocument, string controlId)
        {
            return htmlDocument?.getElementById(controlId);
        }

        public IEnumerable<IHTMLElement> GetElementsByCssQuery(HTMLDocument htmlDocument, string cssQuery)
        {
            var comObject =
                ExecuteJavascript(htmlDocument,
                    $"Array.prototype.slice.call(document.querySelectorAll('{cssQuery}'));");
            Type type = comObject.GetType();
            var length = (int) type.InvokeMember("length", BindingFlags.GetProperty, null, comObject, null);

            for (var i = 1; i <= length; i++)
            {
                yield return type.InvokeMember(i.ToString(), BindingFlags.GetProperty, null, comObject,
                    null) as IHTMLElement;
            }
        }

        public object GetGlobalVariable(HTMLDocument htmlDocument, string variable)
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
                result = InjectAndExecuteJavascript(htmlDocument, variableName);
                if (result == null)
                {
                    return null;
                }
                i++;
            }
            return result;
        }

        public dynamic InjectAndExecuteJavascript(HTMLDocument htmlDocument, string javascriptToExecute)
        {
            if (!Enabled || !JavascriptInjectionEnabled)
            {
                return null;
            }
            return ExecuteJavascript(htmlDocument, javascriptToExecute);
        }

        public void InjectScript(HTMLDocument htmlDocument, string scriptUrl)
        {
            var htmlDocument2 = htmlDocument as IHTMLDocument2;
            var htmlHeadElement = (HTMLHeadElement) htmlDocument.getElementsByTagName("head").item(0);
            var htmlElement = htmlDocument2?.createElement("meta");
            if (htmlElement != null)
            {
                htmlElement.setAttribute("http-equiv", "x-ua-compatible");
                htmlElement.setAttribute("content", "IE=9");
                htmlHeadElement.appendChild((IHTMLDOMNode) htmlElement);
            }
            htmlElement = htmlDocument2?.createElement("script");
            if (htmlElement != null)
            {
                htmlElement.setAttribute("type", "text/javascript");
                htmlElement.setAttribute("src", scriptUrl);
                htmlHeadElement.appendChild((IHTMLDOMNode) htmlElement);
            }

            InjectAndExecuteJavascript(htmlDocument, @"var markup = document.documentElement.innerHTML;alert(markup);");
        }


        public void RemoveEventHandlerToControl(HTMLDocument htmlDocument, string controlId,
            string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var customWebBrowserControlEventHandler = getCustomEventHandler();
            if (customWebBrowserControlEventHandler == null)
            {
                return;
            }
            var element = htmlDocument?.getElementById(controlId);
            if (element != null)
            {
                var eventInfo = EventHandlerAttacher.GetEventInfo(element, eventName);
                if (eventInfo != null)
                {
                    var formattableString = $"{controlId}.{eventName}";
                    var @delegate = customWebBrowserControlEventHandler
                        .GetDelegate(formattableString, eventInfo.EventHandlerType, null, null, functionHash);
                    eventInfo.RemoveEventHandler(element, @delegate);
                    customWebBrowserControlEventHandler.UntrackHandler(formattableString, @delegate, functionHash);
                }
            }
        }

        public void RemoveEventHandlerToDocument(HTMLDocument htmlDocument, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var customWebBrowserControlEventHandler = getCustomEventHandler();
            if (customWebBrowserControlEventHandler == null)
            {
                return;
            }

            var evn = htmlDocument as HTMLDocumentEvents_Event;
            if (evn != null)
            {
                var eventInfo = EventHandlerAttacher.GetEventInfo(evn, eventName);
                if (eventInfo != null)
                {
                    var formattableString = $"{DocumentEventPrefix}.{eventName}";
                    var @delegate = customWebBrowserControlEventHandler
                        .GetDelegate(formattableString, eventInfo.EventHandlerType, null, null, functionHash);
                    var dictionary = customWebBrowserControlEventHandler.Delegates;
                    if (@delegate != null && dictionary.ContainsKey(formattableString, functionHash) &&
                        dictionary.Items(formattableString, @delegate, functionHash).Any())
                    {
                        try
                        {
                            eventInfo.RemoveEventHandler(evn, @delegate);
                        }
                        catch (Exception e)
                        {
                            Debug.Write(e);
                        }
                    }
                    customWebBrowserControlEventHandler.UntrackHandler(formattableString, @delegate, functionHash);
                }
            }
        }

        public void RemoveHandlersOnNavigating(HTMLDocument webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr)
        {
            webBrowser.parentWindow.onbeforeunload += new Func<bool>(
                new NavigatingInterceptor(this, getCustomEventHandler, webBrowser)
                    .WebBrowserOnNavigating);
        }

        private class NavigatingInterceptor
        {
            private readonly Func<CustomWebBrowserControlEventHandler> _getCustomEventHandler;
            private readonly HTMLDocument _webBrowser;
            private readonly WebBrowserExtension _webBrowserExtension;

            public NavigatingInterceptor(WebBrowserExtension webBrowserExtension,
                Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                HTMLDocument webBrowser)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowser = webBrowser;
                _webBrowserExtension = webBrowserExtension;
            }

            public bool WebBrowserOnNavigating()
            {
                var eventHandler = _getCustomEventHandler();
                if (eventHandler == null)
                {
                    return true;
                }
                var eventHandlerDelegates = eventHandler.Delegates;
                foreach (var eventHandlerDelegate in eventHandlerDelegates)
                {
                    var eventName = eventHandlerDelegate.Item1;
                    if (eventName.StartsWith($"{_webBrowserExtension.DocumentEventPrefix}."))
                    {
                        _webBrowserExtension.RemoveEventHandlerToDocument(_webBrowser, eventName,
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        _webBrowserExtension.RemoveEventHandlerToControl(_webBrowser, strings[0], strings[1],
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                }
                return true;
            }
        }
    }
}