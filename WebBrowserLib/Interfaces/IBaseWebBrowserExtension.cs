using System;
using WebBrowserLib.EventHandling;

namespace WebBrowserLib.Interfaces
{
    public interface IBaseWebBrowserExtension<in THtmlDocument, out THtmlElement>
    {
        string DocumentEventPrefix { get; set; }
        bool Enabled { get; set; }

        void AttachEventHandlerToControl(THtmlDocument htmlDocument, string controlId,
            string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler, bool removeHandlers = false);

        void AttachEventHandlerToDocument(THtmlDocument htmlDocument, string eventName,
            object firstArgument, Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void DetachEventHandlersFromControl(THtmlDocument htmlDocument, string controlId,
            bool removeHandlers = false,
            params string[] eventNames);

        dynamic ExecuteJavascript(THtmlDocument htmlDocument, string javascriptToExecute);

        dynamic InvokeJavascript(THtmlDocument htmlDocument, string javascriptToExecute);

        THtmlElement GetElementById(THtmlDocument htmlDocument, string controlId);

        void RemoveEventHandlerToControl(THtmlDocument htmlDocument, string controlId,
            string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void RemoveEventHandlerToDocument(THtmlDocument htmlDocument, string eventName,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);
    }
}