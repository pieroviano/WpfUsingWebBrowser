using System;
using System.Collections.Generic;

namespace WebBrowserLib.Interfaces
{
    public interface IWebBrowserExtensionWithEventBase<out THtmlElementType> : IJavascriptExecutor
    {
        bool Enabled { get; set; }

        void AddJQueryScript(string url);

        void AddScriptElement(string scriptBody);

        void AddScriptsElements(string scriptUrl);

        event EventHandler DocumentReady;

        void EnsureScriptIsInCache(string url);

        dynamic FindElementByAttributeValue(string tagName,
            string attribute, string value);

        IEnumerable<THtmlElementType> FindElementsByAttributeValue(string tagName,
            string attribute, string value);

        string GetCurrentUrl();

        THtmlElementType GetElementById(string controlId);

        IEnumerable<THtmlElementType> GetElementsByCssQuery(string cssQuery);

        dynamic GetGlobalVariable(string variable);

        dynamic EvaluateExpression(string expression);

        void Navigate(string targetUrl);
    }
}