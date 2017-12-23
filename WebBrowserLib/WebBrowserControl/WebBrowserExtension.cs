using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using mshtml;
using WebBrowserLib.Extensions;
using WebBrowserLib.WebBrowserControl.Helpers;

namespace WebBrowserLib.WebBrowserControl
{
    public static partial class WebBrowserExtension
    {
        internal static string
            DocumentEventPrefix = "document";

        public static bool Enabled { get; set; } = true;
        public static bool JavascriptInjectionEnabled { get; set; } = true;

        public static void AttachEventHandlerToControl(this HTMLDocument htmlDocument, string controlId,
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
                            customWebBrowserControlEventHandler.TrackHandler(formattableString, @delegate, functionHash);
                        }
                    }
                }
            }
        }

        public static void AttachEventHandlerToDocument(this HTMLDocument htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var document = (HTMLDocumentEvents_Event)htmlDocument;
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

        public static void DetachEventHandlersFromControl(this HTMLDocument htmlDocument, string controlId,
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

        public static void DisableEventOnControl(this HTMLDocument browser, string controlId, string eventName,
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

        public static void DisableEventOnDocument(this HTMLDocument browser, string eventName,
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

        public static void DisableOnContextMenuOnDocument(this HTMLDocument htmlDocument,
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

        public static void EnableEventOnControl(this HTMLDocument browser, string controlId, string eventName,
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

        public static void EnableEventOnDocument(this HTMLDocument browser, string eventName,
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

        public static void EnableOnContextMenuToDocument(this HTMLDocument htmlDocument,
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

        public static dynamic ExecuteJavascript(this HTMLDocument htmlDocument, string javascriptToExecute)
        {
            return htmlDocument.parentWindow.execScript(javascriptToExecute);
        }

        public static dynamic FindElementByAttributeValue(this HTMLDocument htmlDocument, string tagName,
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

        public static List<dynamic> FindElementsByAttributeValue(this HTMLDocument htmlDocument, string tagName,
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

        public static IHTMLElement GetElementById(this HTMLDocument htmlDocument, string controlId)
        {
            return htmlDocument?.getElementById(controlId);
        }

        public static IEnumerable<IHTMLElement> GetElementsByCssQuery(this HTMLDocument htmlDocument, string cssQuery)
        {
            var comObject =
                htmlDocument.ExecuteJavascript($"Array.prototype.slice.call(document.querySelectorAll('{cssQuery}'));");
            Type type = comObject.GetType();
            var length = (int)type.InvokeMember("length", BindingFlags.GetProperty, null, comObject, null);

            for (var i = 1; i <= length; i++)
            {
                yield return type.InvokeMember(i.ToString(), BindingFlags.GetProperty, null, comObject,
                    null) as IHTMLElement;
            }
        }

        public static object GetGlobalVariable(this HTMLDocument htmlDocument, string variable)
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

        public static dynamic InjectAndExecuteJavascript(this HTMLDocument htmlDocument, string javascriptToExecute)
        {
            if (!Enabled || !JavascriptInjectionEnabled)
            {
                return null;
            }
            return ExecuteJavascript(htmlDocument, javascriptToExecute);
        }

        public static void InjectScript(this HTMLDocument htmlDocument, string scriptUrl)
        {
            var htmlDocument2 = htmlDocument as IHTMLDocument2;
            var htmlHeadElement = (HTMLHeadElement)htmlDocument.getElementsByTagName("head").item(0);
            var htmlElement = htmlDocument2?.createElement("meta");
            if (htmlElement != null)
            {
                htmlElement.setAttribute("http-equiv", "x-ua-compatible");
                htmlElement.setAttribute("content", "IE=9");
                htmlHeadElement.appendChild((IHTMLDOMNode)htmlElement);
            }
            htmlElement = htmlDocument2?.createElement("script");
            if (htmlElement != null)
            {
                htmlElement.setAttribute("type", "text/javascript");
                htmlElement.setAttribute("src", scriptUrl);
                htmlHeadElement.appendChild((IHTMLDOMNode)htmlElement);
            }

            InjectAndExecuteJavascript(htmlDocument, @"var markup = document.documentElement.innerHTML;alert(markup);");
        }


        public static void RemoveEventHandlerToControl(this HTMLDocument htmlDocument, string controlId,
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

        public static void RemoveEventHandlerToDocument(this HTMLDocument htmlDocument, string eventName,
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

        public static void RemoveHandlersOnNavigating(this HTMLDocument webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            webBrowser.parentWindow.onbeforeunload += new Func<bool>(
                new NavigatingInterceptor(getCustomEventHandler, webBrowser)
                    .WebBrowserOnNavigating);
        }

        private class NavigatingInterceptor
        {
            private readonly Func<CustomWebBrowserControlEventHandler> _getCustomEventHandler;
            private readonly HTMLDocument _webBrowser;

            public NavigatingInterceptor(Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                HTMLDocument webBrowser)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowser = webBrowser;
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
                return true;
            }
        }
    }
}