using System;
using System.Collections.Generic;
using mshtml;
using WebBrowserLib.WebBrowserControl;

namespace WebBrowserLib.Interfaces
{
    public interface IWebBrowserExtension<in TControl, THead, out THtmlElementType> : IScriptInjector<THead>
    {
        bool Enabled { get; set; }

        bool JavascriptInjectionEnabled { get; set; }

        void AttachEventHandlerToControl(TControl htmlDocument, string controlId,
            string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false);

        void AttachEventHandlerToDocument(TControl htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DetachEventHandlersFromControl(TControl htmlDocument, string controlId,
            bool removeHandlers = false,
            params string[] eventNames);

        void DisableEventOnControl(TControl htmlDocument, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DisableEventOnDocument(TControl htmlDocument, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DisableOnContextMenuOnDocument(TControl htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler);

        void EnableEventOnControl(TControl htmlDocument, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableEventOnDocument(TControl htmlDocument, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableOnContextMenuToDocument(TControl htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        dynamic ExecuteJavascript(TControl htmlDocument, string javascriptToExecute);

        dynamic FindElementByAttributeValue(TControl htmlDocument, string tagName,
            string attribute, string value);

        List<object> FindElementsByAttributeValue(TControl htmlDocument, string tagName,
            string attribute, string value);

        IHTMLElement GetElementById(TControl htmlDocument, string controlId);

        IEnumerable<THtmlElementType> GetElementsByCssQuery(TControl htmlDocument, string cssQuery);

        object GetGlobalVariable(TControl htmlDocument, string variable);

        dynamic InjectAndExecuteJavascript(TControl htmlDocument, string javascriptToExecute);

        void InjectScript(TControl htmlDocument, string scriptUrl);

        void RemoveEventHandlerToControl(TControl htmlDocument, string controlId,
            string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveEventHandlerToDocument(TControl htmlDocument, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveHandlersOnNavigating(TControl htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandle,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr);
    }
}