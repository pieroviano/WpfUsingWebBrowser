using System;
using System.Collections.Generic;

namespace WebBrowserLib.Interfaces
{
    public interface IWebBrowserExtensionWithEventBase<out THtmlElementType>: IJavascriptExecutor
    {
        bool Enabled { get; set; }

        void AddJQueryElement();
        void AddScriptElement(string scriptBody);

        event EventHandler DocumentReady;

        dynamic FindElementByAttributeValue(string tagName,
            string attribute, string value);

        IEnumerable<THtmlElementType> FindElementsByAttributeValue(string tagName,
            string attribute, string value);

        string GetCurrentUrl();

        THtmlElementType GetElementById(string controlId);

        IEnumerable<THtmlElementType> GetElementsByCssQuery(string cssQuery);

        dynamic GetGlobalVariable(string variable);

        void Navigate(string targetUrl);
    }
}