using System;
using System.Collections.Generic;
using mshtml;
using WebBrowserLib.WebBrowserControl.Helpers;

namespace WebBrowserLib.WebBrowserControl.Interfaces
{
    public interface IWebBrowserExtension: IScriptInjector
    {
        void AttachEventHandlerToControl(object htmlDocument, string controlId,
            string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false);

        void AttachEventHandlerToDocument(object htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DetachEventHandlersFromControl(object htmlDocument, string controlId,
            bool removeHandlers = false,
            params string[] eventNames);

        void DisableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DisableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DisableOnContextMenuOnDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler);

        void EnableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableOnContextMenuToDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        dynamic ExecuteJavascript(object htmlDocument, string javascriptToExecute);

        dynamic FindElementByAttributeValue(object htmlDocument, string tagName,
            string attribute, string value);

        List<object> FindElementsByAttributeValue(object htmlDocument, string tagName,
            string attribute, string value);

        IHTMLElement GetElementById(object htmlDocument, string controlId);
        IEnumerable<object> GetElementsByCssQuery(object htmlDocument, string cssQuery);
        object GetGlobalVariable(object htmlDocument, string variable);
        dynamic InjectAndExecuteJavascript(object htmlDocument, string javascriptToExecute);
        void InjectScript(object htmlDocument, string scriptUrl);

        void RemoveEventHandlerToControl(object htmlDocument, string controlId,
            string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveEventHandlerToDocument(object htmlDocument, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveHandlersOnNavigating(object webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandle,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr);
    }
}