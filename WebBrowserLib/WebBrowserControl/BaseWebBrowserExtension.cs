using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Extensions;
using WebBrowserLib.Helpers;
using WebBrowserLib.Interfaces;

namespace WebBrowserLib.WebBrowserControl
{
    public abstract class BaseWebBrowserExtension<THtmlDocument, THtmlElement> : IBaseWebBrowserExtension<THtmlDocument, THtmlElement>
    {
        public string DocumentEventPrefix { get; set; } = "document";

        public bool Enabled { get; set; } = true;

        public virtual void AttachEventHandlerToControl(THtmlDocument htmlDocument, string controlId,
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

        public virtual void AttachEventHandlerToDocument(THtmlDocument htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            var document = htmlDocument;
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

        public virtual void DetachEventHandlersFromControl(THtmlDocument htmlDocument, string controlId,
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
                    ExecuteJavascript(htmlDocument, cleanHandlers);
                }
                return;
            }
            var javascriptToExecuteToRemoveHandlers =
                ScriptHelper.GetJavascriptToExecuteToRemoveHandlers(controlId, cleanHandlers);
            if (!string.IsNullOrEmpty(javascriptToExecuteToRemoveHandlers))
            {
                ExecuteJavascript(htmlDocument, javascriptToExecuteToRemoveHandlers);
            }
        }

        public abstract dynamic ExecuteJavascript(THtmlDocument htmlDocument, string javascriptToExecute);
        public abstract dynamic InvokeJavascript(THtmlDocument htmlDocument, string javascriptToExecute);

        public abstract THtmlElement GetElementById(THtmlDocument htmlDocument, string controlId);

        public virtual void RemoveEventHandlerToControl(THtmlDocument htmlDocument, string controlId,
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
            var element = GetElementById(htmlDocument, controlId);
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

        public virtual void RemoveEventHandlerToDocument(THtmlDocument htmlDocument, string eventName,
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

            var evn = htmlDocument;
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
    }
}