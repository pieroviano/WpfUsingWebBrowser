using System;
using System.Collections.Generic;
using WebBrowserLib.EventHandling;

namespace WebBrowserLib.Interfaces
{
    public interface IWebBrowserExtensionWithEventBase<out THtmlElementType>
    {
        bool Enabled { get; set; }

        void AddJQueryElement();
        void AddScriptElement(string scriptBody);

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

        IEnumerable<THtmlElementType> FindElementsByAttributeValue(string tagName,
            string attribute, string value);

        THtmlElementType GetElementById(string controlId);

        IEnumerable<THtmlElementType> GetElementsByCssQuery(string cssQuery);

        dynamic GetGlobalVariable(string variable);

        dynamic InjectAndExecuteJavascript(string javascriptToExecute);

        void Navigate(string targetUrl);
    }
}