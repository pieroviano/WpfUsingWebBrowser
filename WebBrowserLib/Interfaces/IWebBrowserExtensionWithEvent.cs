using System;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Helpers;

namespace WebBrowserLib.Interfaces
{
    public interface
        IWebBrowserExtensionWithEvent<out THtmlElementType> : IWebBrowserExtensionWithEventBase<THtmlElementType>
    {
        bool JavascriptInjectionEnabled { get; set; }

        void AttachCustomFunctionOnControl(string controlId, string eventName,
            Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void AttachCustomFunctionOnDocument(string eventName, Func<bool> codeToExecute,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

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

        string RegisterCsCodeCallableFromJavascript(ref ComVisibleClass comVisibleClass);

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