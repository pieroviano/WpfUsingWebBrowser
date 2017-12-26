using System;
using System.Collections.Generic;
using mshtml;
using WebBrowserLib.WebBrowserControl;

namespace WebBrowserLib.Interfaces
{
    public interface IWebBrowserExtensionWithEvent<THtmlElementType>
    {
        bool Enabled { get; set; }

        bool JavascriptInjectionEnabled { get; set; }

        void AddJQueryElement();
        void AddScriptElement(string scriptBody);

        void AttachEventHandlerToControl(string controlId,
            string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false);

        void AttachEventHandlerToDocument(string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DetachEventHandlersFromControl(string controlId,
            bool removeHandlers = false,
            params string[] eventNames);

        void DisableEventOnControl(string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DisableEventOnDocument(string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DisableOnContextMenuOnDocument(
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler);

        event EventHandler DocumentReady;

        void EnableEventOnControl(string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableEventOnDocument(string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableOnContextMenuToDocument(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        dynamic ExecuteJavascript(string javascriptToExecute);

        dynamic FindElementByAttributeValue(string tagName,
            string attribute, string value);

        List<object> FindElementsByAttributeValue(string tagName,
            string attribute, string value);

        IHTMLElement GetElementById(string controlId);

        IEnumerable<THtmlElementType> GetElementsByCssQuery(string cssQuery);

        object GetGlobalVariable(string variable);

        dynamic InjectAndExecuteJavascript(string javascriptToExecute);

        void InjectScript(string scriptUrl);

        void RemoveEventHandlerToControl(string controlId,
            string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveEventHandlerToDocument(string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveHandlersOnNavigating(
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandle,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr);
    }
}