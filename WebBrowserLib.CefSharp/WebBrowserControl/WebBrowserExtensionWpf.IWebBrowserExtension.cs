using System;
using System.Collections.Generic;
using System.Windows.Controls;
using mshtml;
using WebBrowserLib.mshtml.WebBrowserControl;
using WebBrowserLib.WebBrowserControl;
using WebBrowserLib.WebBrowserControl.Interfaces;

namespace WebBrowserLib.Wpf.WebBrowserControl
{
    public partial class WebBrowserExtensionWpf { 
        object IWebBrowserExtension.Instance => Instance;

        void IWebBrowserExtension.AttachEventHandlerToControl(object htmlDocument, string controlId, string eventName,
            object firstArgument,
            Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler,
            bool removeHandlers)
        {
            AttachEventHandlerToControl((WebBrowser) htmlDocument, controlId, eventName, firstArgument,
                customEventDelegate, functionHash, getCustomEventHandler, setCustomEventHandler,
                removeHandlers);
        }

        void IWebBrowserExtension.AttachEventHandlerToDocument(object htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            AttachEventHandlerToDocument((WebBrowser) htmlDocument, eventName, firstArgument, customEventDelegate,
                functionHash, getCustomEventHandler, setCustomEventHandler);
        }

        void IWebBrowserExtension.DetachEventHandlersFromControl(object htmlDocument, string controlId,
            bool removeHandlers,
            params string[] eventNames)
        {
            DetachEventHandlersFromControl((WebBrowser) htmlDocument, controlId, removeHandlers,
                eventNames);
        }

        void IWebBrowserExtension.DisableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DisableEventOnControl((WebBrowser) browser, controlId, eventName, getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.DisableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DisableEventOnDocument((WebBrowser) browser, eventName, getCustomEventHandler, setCustomEventHandler);
        }

        void IWebBrowserExtension.DisableOnContextMenuOnDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler)
        {
            DisableOnContextMenuOnDocument((WebBrowser) htmlDocument, getControlEventHandler, setControlEventHandler);
        }

        void IWebBrowserExtension.EnableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            EnableEventOnControl((WebBrowser) browser, controlId, eventName, getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.EnableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            EnableEventOnDocument((WebBrowser) browser, eventName, getCustomEventHandler, setCustomEventHandler);
        }

        void IWebBrowserExtension.EnableOnContextMenuToDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            EnableOnContextMenuToDocument((WebBrowser) htmlDocument, getCustomEventHandler, setCustomEventHandler);
        }

        dynamic IWebBrowserExtension.ExecuteJavascript(object htmlDocument, string javascriptToExecute)
        {
            return ExecuteJavascript((WebBrowser) htmlDocument, javascriptToExecute);
        }

        dynamic IWebBrowserExtension.FindElementByAttributeValue(object htmlDocument, string tagName, string attribute,
            string value)
        {
            return FindElementByAttributeValue((WebBrowser) htmlDocument, tagName, attribute, value);
        }

        List<object> IWebBrowserExtension.FindElementsByAttributeValue(object htmlDocument, string tagName,
            string attribute, string value)
        {
            return FindElementsByAttributeValue((WebBrowser) htmlDocument, tagName, attribute, value);
        }

        IHTMLElement IWebBrowserExtension.GetElementById(object htmlDocument, string controlId)
        {
            return GetElementById((WebBrowser) htmlDocument, controlId);
        }

        IEnumerable<object> IWebBrowserExtension.GetElementsByCssQuery(object htmlDocument, string cssQuery)
        {
            return GetElementsByCssQuery((WebBrowser) htmlDocument, cssQuery);
        }

        object IWebBrowserExtension.GetGlobalVariable(object htmlDocument, string variable)
        {
            return GetGlobalVariable((WebBrowser) htmlDocument, variable);
        }

        dynamic IWebBrowserExtension.InjectAndExecuteJavascript(object htmlDocument, string javascriptToExecute)
        {
            return InjectAndExecuteJavascript((WebBrowser) htmlDocument, javascriptToExecute);
        }

        void IWebBrowserExtension.InjectScript(object htmlDocument, string scriptUrl)
        {
            InjectScript((WebBrowser) htmlDocument, scriptUrl);
        }

        void IWebBrowserExtension.RemoveEventHandlerToControl(object htmlDocument, string controlId, string eventName,
            int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            RemoveEventHandlerToControl((WebBrowser) htmlDocument, controlId, eventName, functionHash,
                getCustomEventHandler);
        }

        void IWebBrowserExtension.RemoveEventHandlerToDocument(object htmlDocument, string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler)
        {
            RemoveEventHandlerToDocument((WebBrowser) htmlDocument, eventName, functionHash, getCustomEventHandler);
        }

        void IWebBrowserExtension.RemoveHandlersOnNavigating(object webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandle,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr)
        {
            RemoveHandlersOnNavigating((WebBrowser) webBrowser, getCustomEventHandle, setCustomEventHandlerr);
        }
    }
}