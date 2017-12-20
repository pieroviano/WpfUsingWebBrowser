using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
            Func<bool> customEventDelegate, IntPtr functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false)
        {
            if (!Enabled)
                return;
            DetachEventHandlersFromControl(htmlDocument, controlId, removeHandlers, eventName);

            if (htmlDocument != null)
            {
                var element = GetElementById(htmlDocument, controlId);
                var formattableString = $"{controlId}.{eventName}";
                if (getCustomEventHandler() == null)
                {
                    var customEventDelegates = new List<Tuple<string, Delegate, IntPtr>>();
                    setCustomEventHandler(new CustomWebBrowserControlEventHandler(customEventDelegates));
                }
                if (element != null)
                {
                    if (!getCustomEventHandler().EventHandlerIsAttached(formattableString, functionHash))
                    {
                        var eventInfo = EventHandlerAttacher.GetEventInfo(element, eventName);
                        var @delegate = getCustomEventHandler()
                            .GetDelegate(formattableString, eventInfo.EventHandlerType, functionHash);
                        eventInfo.AddEventHandler(element, @delegate);
                        getCustomEventHandler().TrackHandler(formattableString, @delegate);
                    }
                }
            }
        }

        public static void AttachEventHandlerToDocument(this HTMLDocument htmlDocument, string eventName,
            Func<bool> customEventDelegate, IntPtr functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
                return;
            var evn = (HTMLDocumentEvents_Event) htmlDocument;
            var formattableString = $"{DocumentEventPrefix}.{eventName}";
            if (getCustomEventHandler() == null)
            {
                var customEventDelegates = new List<Tuple<string, Delegate, IntPtr>>();
                setCustomEventHandler(new CustomWebBrowserControlEventHandler(customEventDelegates));
            }
            if (evn != null)
            {
                if (!getCustomEventHandler().EventHandlerIsAttached(formattableString, functionHash))
                {
                    var eventInfo = EventHandlerAttacher.GetEventInfo(evn, eventName);
                    var @delegate = getCustomEventHandler()
                        .GetDelegate(formattableString, eventInfo.EventHandlerType, functionHash);
                    eventInfo.AddEventHandler(evn, @delegate);
                    getCustomEventHandler().TrackHandler(formattableString, @delegate);
                }
            }
        }

        public static void DetachEventHandlersFromControl(this HTMLDocument htmlDocument, string controlId,
            bool removeHandlers = false,
            params string[] eventNames)
        {
            if (!Enabled)
                return;
            var cleanHandlers = ScriptHelper.PrepareCleanHandlers(eventNames);
            if (!removeHandlers)
            {
                if (!string.IsNullOrEmpty(cleanHandlers))
                    InjectAndExecuteJavascript(htmlDocument, cleanHandlers);
                return;
            }
            var javascriptToExecuteToRemoveHandlers =
                ScriptHelper.GetJavascriptToExecuteToRemoveHandlers(controlId, cleanHandlers);
            if (!string.IsNullOrEmpty(javascriptToExecuteToRemoveHandlers))
                InjectAndExecuteJavascript(htmlDocument, javascriptToExecuteToRemoveHandlers);
        }

        public static void DisableOnContextMenuToDocument(this HTMLDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler)
        {
            if (!Enabled)
                return;
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = (IntPtr)customEventDelegate.GetHashCode();
            AttachEventHandlerToDocument(htmlDocument, "oncontextmenu",
                customEventDelegate, functionHash, getControlEventHandler,
                setControlEventHandler);
        }

        public static void EnableOnContextMenuToDocument(this HTMLDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
                return;
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = (IntPtr)customEventDelegate.GetHashCode();
            RemoveEventHandlerToDocument(htmlDocument,
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public static void RemoveHandlersOnNavigating(this HTMLDocument webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            webBrowser.parentWindow.onbeforeunload += new Func<bool>(new NavigatingInterceptor(getCustomEventHandler, webBrowser)
                .WebBrowserOnNavigating);
        }

        public static dynamic FindElementByAttributeValue(this HTMLDocument htmlDocument, string tagName,
            string attribute, string value)
        {
            if (!Enabled)
                return null;
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
                return ret;
            var all = htmlDocument.getElementsByTagName(tagName);
            for (var i = 0; i < all.length; i++)
            {
                var str = all.item(i).GetAttribute(attribute);
                if (str != "")
                    if (str == value)
                    {
                        ret.Add(all.item(i));
                    }
            }
            return ret;
        }

        public static IHTMLElement GetElementById(this HTMLDocument htmlDocument, string controlId)
        {
            return htmlDocument?.getElementById(controlId);
        }

        public static object GetGlobalVariable(this HTMLDocument htmlDocument, string variable)
        {
            if (!Enabled)
                return null;
            var variablePath = variable.Split('.');
            var i = 0;
            object result = null;
            var variableName = "window";
            while (i < variablePath.Length)
            {
                variableName = variableName + "." + variablePath[i];
                result = InjectAndExecuteJavascript(htmlDocument, variableName);
                if (result == null)
                    return null;
                i++;
            }
            return result;
        }

        public static dynamic InjectAndExecuteJavascript(this HTMLDocument htmlDocument, string javascriptToExecute)
        {
            if (!Enabled || !JavascriptInjectionEnabled)
                return null;
            return htmlDocument.parentWindow.execScript(javascriptToExecute);
        }

        public static void RemoveEventHandlerToControl(this HTMLDocument htmlDocument, string controlId,
            string eventName, IntPtr functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
                return;
            if (getCustomEventHandler() == null)
                return;
            var element = htmlDocument?.getElementById(controlId);
            if (element != null)
            {
                var eventInfo = EventHandlerAttacher.GetEventInfo(element, eventName);
                var @delegate = getCustomEventHandler().GetDelegate($"{controlId}.{eventName}",
                    eventInfo.EventHandlerType, functionHash);
                eventInfo.RemoveEventHandler(element, @delegate);
                getCustomEventHandler().UntrackHandler($"{controlId}.{eventName}", @delegate);
            }
        }

        public static void RemoveEventHandlerToDocument(this HTMLDocument htmlDocument, string eventName,
            IntPtr functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            if (!Enabled)
                return;
            if (getCustomEventHandler() == null)
                return;

            var evn = htmlDocument as HTMLDocumentEvents_Event;
            if (evn != null)
            {
                var eventInfo = EventHandlerAttacher.GetEventInfo(evn, eventName);
                if (eventInfo != null)
                {
                    var formattableString = $"{DocumentEventPrefix}.{eventName}";
                    var @delegate = getCustomEventHandler()
                        .GetDelegate(formattableString, eventInfo.EventHandlerType, functionHash);
                    var dictionary = getCustomEventHandler().Delegates;
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
                    getCustomEventHandler().UntrackHandler(formattableString, @delegate);
                }
            }
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
                    return true;
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