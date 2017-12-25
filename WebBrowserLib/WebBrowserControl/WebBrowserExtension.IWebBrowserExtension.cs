using System;
using System.Collections.Generic;
using mshtml;
using WebBrowserLib.WebBrowserControl.Helpers;
using WebBrowserLib.WebBrowserControl.Interfaces;

namespace WebBrowserLib.WebBrowserControl
{
    public partial class WebBrowserExtension
    {
        void IWebBrowserExtension.AttachEventHandlerToControl(object htmlDocument, string controlId, string eventName,
            object firstArgument,
            Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler,
            bool removeHandlers)
        {
            AttachEventHandlerToControl((HTMLDocument) htmlDocument, controlId, eventName, firstArgument,
                customEventDelegate, functionHash, getCustomEventHandler, setCustomEventHandler,
                removeHandlers);
        }

        void IWebBrowserExtension.AttachEventHandlerToDocument(object htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            AttachEventHandlerToDocument((HTMLDocument) htmlDocument, eventName, firstArgument, customEventDelegate,
                functionHash, getCustomEventHandler, setCustomEventHandler);
        }

        void IWebBrowserExtension.DetachEventHandlersFromControl(object htmlDocument, string controlId,
            bool removeHandlers,
            params string[] eventNames)
        {
            DetachEventHandlersFromControl((HTMLDocument) htmlDocument, controlId, removeHandlers,
                eventNames);
        }

        void IWebBrowserExtension.DisableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DisableEventOnControl((HTMLDocument) browser, controlId, eventName, getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.DisableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DisableEventOnDocument((HTMLDocument) browser, eventName, getCustomEventHandler, setCustomEventHandler);
        }

        void IWebBrowserExtension.DisableOnContextMenuOnDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler)
        {
            DisableOnContextMenuOnDocument((HTMLDocument) htmlDocument, getControlEventHandler, setControlEventHandler);
        }

        void IWebBrowserExtension.EnableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            EnableEventOnControl((HTMLDocument) browser, controlId, eventName, getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.EnableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            EnableEventOnDocument((HTMLDocument) browser, eventName, getCustomEventHandler, setCustomEventHandler);
        }

        void IWebBrowserExtension.EnableOnContextMenuToDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            EnableOnContextMenuToDocument((HTMLDocument) htmlDocument, getCustomEventHandler, setCustomEventHandler);
        }

        dynamic IWebBrowserExtension.ExecuteJavascript(object htmlDocument, string javascriptToExecute)
        {
            return ExecuteJavascript((HTMLDocument) htmlDocument, javascriptToExecute);
        }

        dynamic IWebBrowserExtension.FindElementByAttributeValue(object htmlDocument, string tagName, string attribute,
            string value)
        {
            return FindElementByAttributeValue((HTMLDocument) htmlDocument, tagName, attribute, value);
        }

        List<object> IWebBrowserExtension.FindElementsByAttributeValue(object htmlDocument, string tagName,
            string attribute, string value)
        {
            return FindElementsByAttributeValue((HTMLDocument) htmlDocument, tagName, attribute, value);
        }

        IHTMLElement IWebBrowserExtension.GetElementById(object htmlDocument, string controlId)
        {
            return GetElementById((HTMLDocument) htmlDocument, controlId);
        }

        IEnumerable<object> IWebBrowserExtension.GetElementsByCssQuery(object htmlDocument, string cssQuery)
        {
            return GetElementsByCssQuery((HTMLDocument) htmlDocument, cssQuery);
        }

        object IWebBrowserExtension.GetGlobalVariable(object htmlDocument, string variable)
        {
            return GetGlobalVariable((HTMLDocument) htmlDocument, variable);
        }

        dynamic IWebBrowserExtension.InjectAndExecuteJavascript(object htmlDocument, string javascriptToExecute)
        {
            return InjectAndExecuteJavascript((HTMLDocument) htmlDocument, javascriptToExecute);
        }

        void IWebBrowserExtension.InjectScript(object htmlDocument, string scriptUrl)
        {
            InjectScript((HTMLDocument) htmlDocument, scriptUrl);
        }

        void IWebBrowserExtension.RemoveEventHandlerToControl(object htmlDocument, string controlId, string eventName,
            int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            RemoveEventHandlerToControl((HTMLDocument) htmlDocument, controlId, eventName, functionHash,
                getCustomEventHandler);
        }

        void IWebBrowserExtension.RemoveEventHandlerToDocument(object htmlDocument, string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            RemoveEventHandlerToDocument((HTMLDocument) htmlDocument, eventName, functionHash, getCustomEventHandler);
        }

        void IWebBrowserExtension.RemoveHandlersOnNavigating(object webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr)
        {
            RemoveHandlersOnNavigating((HTMLDocument) webBrowser, getCustomEventHandler, setCustomEventHandlerr);
        }

        void IScriptInjector.AddJQueryElement(object head)
        {
            AddJQueryElement(head as HTMLHeadElement);
        }

        void IScriptInjector.AddScriptElement(object head, string scriptBody)
        {
            AddScriptElement(head as HTMLHeadElement, scriptBody);
        }
    }
}