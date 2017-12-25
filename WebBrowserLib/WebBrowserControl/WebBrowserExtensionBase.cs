using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using mshtml;
using WebBrowserLib.WebBrowserControl.Interfaces;

namespace WebBrowserLib.WebBrowserControl
{
    public abstract class WebBrowserExtensionBase<TControl, THead> : IWebBrowserExtension<TControl, THead>
    {
        public abstract bool Enabled { get; set; }

        public abstract bool JavascriptInjectionEnabled { get; set; }

        object IWebBrowserExtension.Instance { get { return Instance; } }

        void IScriptInjector.AddJQueryElement(object head) { AddJQueryElement((THead)head);}

        void IScriptInjector.AddScriptElement(object head, string scriptBody)
        {
            AddScriptElement((THead)head, scriptBody);
        }

        void IWebBrowserExtension.AttachEventHandlerToControl(object htmlDocument, string controlId, string eventName,
            object firstArgument,
            Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler,
            bool removeHandlers)
        {
            DoAttachEventHandlerToControl(htmlDocument, controlId, eventName,
                firstArgument,
                customEventDelegate, functionHash,
                getCustomEventHandler,
                setCustomEventHandler,
                removeHandlers);
        }

        void IWebBrowserExtension.AttachEventHandlerToDocument(object htmlDocument, string eventName, object firstArgument,
            Func<bool> customEventDelegate,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DoAttachEventHandlerToDocument(htmlDocument, eventName, firstArgument,
                customEventDelegate,
                functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.DetachEventHandlersFromControl(object htmlDocument, string controlId,
            bool removeHandlers,
            params string[] eventNames)
        {
            DoDetachEventHandlersFromControl(htmlDocument, controlId,
                removeHandlers,
                eventNames);
        }

        void IWebBrowserExtension.DisableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DoDisableEventOnControl(browser, controlId, eventName,
                getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.DisableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DoDisableEventOnDocument(browser, eventName,
                getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.DisableOnContextMenuOnDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler)
        {
            DoDisableOnContextMenuOnDocument(htmlDocument,
                getControlEventHandler,
                setControlEventHandler);
        }

        void IWebBrowserExtension.EnableEventOnControl(object browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            DoEnableEventOnControl(browser, controlId, eventName,
                getCustomEventHandler,
                setCustomEventHandler);
        }

        void IWebBrowserExtension.EnableEventOnDocument(object browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        void IWebBrowserExtension.EnableOnContextMenuToDocument(object htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        dynamic IWebBrowserExtension.ExecuteJavascript(object htmlDocument, string javascriptToExecute);

        dynamic IWebBrowserExtension.FindElementByAttributeValue(object htmlDocument, string tagName, string attribute,
            string value);

        List<object> IWebBrowserExtension.FindElementsByAttributeValue(object htmlDocument, string tagName, string attribute,
            string value);

        IHTMLElement IWebBrowserExtension.GetElementById(object htmlDocument, string controlId);
        IEnumerable<object> IWebBrowserExtension.GetElementsByCssQuery(object htmlDocument, string cssQuery);
        object IWebBrowserExtension.GetGlobalVariable(object htmlDocument, string variable);
        dynamic IWebBrowserExtension.InjectAndExecuteJavascript(object htmlDocument, string javascriptToExecute);
        void IWebBrowserExtension.InjectScript(object htmlDocument, string scriptUrl);

        void IWebBrowserExtension.RemoveEventHandlerToControl(object htmlDocument, string controlId, string eventName,
            int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void IWebBrowserExtension.RemoveEventHandlerToDocument(object htmlDocument, string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        void IWebBrowserExtension.RemoveHandlersOnNavigating(object webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandle,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr);

        //

        public abstract TControl Instance { get; }

        public abstract void AddJQueryElement(THead head);
        public abstract void AddScriptElement(THead head, string scriptBody);

        public abstract void AttachEventHandlerToControl(TControl htmlDocument, string controlId, string eventName,
            object firstArgument,
            Func<bool> customEventDelegate, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler,
            bool removeHandlers = false);

        public abstract void AttachEventHandlerToDocument(TControl htmlDocument, string eventName, object firstArgument,
            Func<bool> customEventDelegate,
            int functionHash, Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        public abstract void DetachEventHandlersFromControl(TControl htmlDocument, string controlId,
            bool removeHandlers = false,
            params string[] eventNames);

        public abstract void DisableEventOnControl(TControl browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        public abstract void DisableEventOnDocument(TControl browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        public abstract void DisableOnContextMenuOnDocument(TControl htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler);

        public abstract void EnableEventOnControl(TControl browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        public abstract void EnableEventOnDocument(TControl browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        public abstract void EnableOnContextMenuToDocument(TControl htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler);

        public abstract dynamic ExecuteJavascript(TControl htmlDocument, string javascriptToExecute);

        public abstract dynamic FindElementByAttributeValue(TControl htmlDocument, string tagName, string attribute,
            string value);

        public abstract List<object> FindElementsByAttributeValue(TControl htmlDocument, string tagName, string attribute,
            string value);

        public abstract IHTMLElement GetElementById(TControl htmlDocument, string controlId);
        public abstract IEnumerable<object> GetElementsByCssQuery(TControl htmlDocument, string cssQuery);
        public abstract object GetGlobalVariable(TControl htmlDocument, string variable);
        public abstract dynamic InjectAndExecuteJavascript(TControl htmlDocument, string javascriptToExecute);
        public abstract void InjectScript(TControl htmlDocument, string scriptUrl);

        public abstract void RemoveEventHandlerToControl(TControl htmlDocument, string controlId, string eventName,
            int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        public abstract void RemoveEventHandlerToDocument(TControl htmlDocument, string eventName, int functionHash,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler);

        public abstract void RemoveHandlersOnNavigating(TControl webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandle,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr);

    }
}