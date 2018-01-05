using System;
using System.Collections.Generic;
using WebBrowserLib.EventHandling;

namespace WebBrowserLib.Interfaces
{
    public interface IWebBrowserExtension<in TDocument, THtmlElementType> : IScriptInjector<TDocument, THtmlElementType>
    {
        bool Enabled { get; set; }

        string DocumentEventPrefix { get; set; }

        void AttachEventHandlerToControl(TDocument htmlDocument, string controlId,
            string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false);

        void AttachEventHandlerToDocument(TDocument htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DetachEventHandlersFromControl(TDocument htmlDocument, string controlId,
            bool removeHandlers = false,
            params string[] eventNames);

        void DisableEventOnControl(TDocument htmlDocument, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DisableEventOnDocument(TDocument htmlDocument, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        object InvokeScript(TDocument htmlDocument, string scriptName, params object[] args);

        void DisableOnContextMenuOnDocument(TDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler);

        void EnableEventOnControl(TDocument htmlDocument, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableEventOnDocument(TDocument htmlDocument, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void EnableOnContextMenuToDocument(TDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        object EvaluateExpression(TDocument htmlDocument, string variableName);

        dynamic ExecuteJavascript(TDocument htmlDocument, string javascriptToExecute);

        dynamic InvokeJavascript(TDocument htmlDocument, string javascriptToExecute);

        dynamic FindElementByAttributeValue(TDocument htmlDocument, string tagName,
            string attribute, string value);

        IEnumerable<THtmlElementType> FindElementsByAttributeValue(TDocument htmlDocument, string tagName,
            string attribute, string value);

        string GetCurrentUrl(TDocument htmlDocument);

        THtmlElementType GetElementById(TDocument htmlDocument, string controlId);

        IEnumerable<THtmlElementType> GetElementsByCssQuery(TDocument htmlDocument, string cssQuery);

        object GetGlobalVariable(TDocument htmlDocument, string variable);

        void RemoveEventHandlerToControl(TDocument htmlDocument, string controlId,
            string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveEventHandlerToDocument(TDocument htmlDocument, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveHandlersOnNavigating(TDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandle,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr);
    }
}