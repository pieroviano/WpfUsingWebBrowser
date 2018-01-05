using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.Expando;
using mshtml;
using WebBrowserLib.EventHandling;
using WebBrowserLib.Extensions;
using WebBrowserLib.Interfaces;
using WebBrowserLib.MsHtml.Utility;
using WebBrowserLib.WebBrowserControl;

namespace WebBrowserLib.MsHtml.WebBrowserControl
{
    public class WebBrowserExtensionMsHtmlDocument : BaseWebBrowserExtension<HTMLDocument, IHTMLElement>,
        IWebBrowserExtension<HTMLDocument, IHTMLElement>
    {
        private WebBrowserExtensionMsHtmlDocument()
        {
        }

        public static WebBrowserExtensionMsHtmlDocument Instance { get; } = new WebBrowserExtensionMsHtmlDocument();

        public List<IHTMLElement> GetJQueryScriptsElements(HTMLDocument document)
        {
            if (!Enabled)
            {
                return null;
            }
            var returnValue = new List<IHTMLElement>();
            var htmlDocument = document;
            var scriptEl = htmlDocument?.createElement("script") as HTMLScriptElement;
            var jQueryElement = (IHTMLScriptElement)scriptEl;
            if (jQueryElement != null)
            {
                jQueryElement.src = @"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";
            }
            returnValue.Add((IHTMLElement)scriptEl);
            return returnValue;
        }

        public List<IHTMLElement> GetScriptsElements(HTMLDocument htmlDocument, string scriptUrl)
        {
            if (!Enabled)
            {
                return null;
            }
            var returnValue = new List<IHTMLElement>();
            var htmlElement = htmlDocument?.createElement("meta");
            if (htmlElement != null)
            {
                htmlElement.setAttribute("http-equiv", "x-ua-compatible");
                htmlElement.setAttribute("content", "IE=9");
                returnValue.Add(htmlElement);
            }
            htmlElement = htmlDocument?.createElement("script");
            if (htmlElement != null)
            {
                htmlElement.setAttribute("type", "text/javascript");
                htmlElement.setAttribute("src", scriptUrl);
                returnValue.Add(htmlElement);
            }
            return returnValue;
        }

        public List<IHTMLElement> CreateScriptElement(HTMLDocument document, string scriptBody)
        {
            if (!Enabled)
            {
                return null;
            }
            var returnValue = new List<IHTMLElement>();
            var scriptEl = document?.createElement("script") as HTMLScriptElement;
            if (scriptEl != null)
            {
                scriptEl.innerHTML = scriptBody;

                returnValue.Add((IHTMLElement)scriptEl);
            }
            return returnValue;
        }

        public void DisableEventOnControl(HTMLDocument browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToControl(browser, controlId, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableEventOnDocument(HTMLDocument browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(browser, eventName,
                null, customEventDelegate, functionHash, getCustomEventHandler,
                setCustomEventHandler);
        }

        public void DisableOnContextMenuOnDocument(HTMLDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getControlEventHandler,
            Action<CustomWebBrowserControlEventHandler> setControlEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            AttachEventHandlerToDocument(htmlDocument, "oncontextmenu",
                null, customEventDelegate, functionHash, getControlEventHandler,
                setControlEventHandler);
        }

        public void EnableEventOnControl(HTMLDocument browser, string controlId, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToControl(browser, controlId, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableEventOnDocument(HTMLDocument browser, string eventName,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(browser, eventName,
                functionHash, getCustomEventHandler);
        }

        public void EnableOnContextMenuToDocument(HTMLDocument htmlDocument,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandler)
        {
            if (!Enabled)
            {
                return;
            }
            Func<bool> customEventDelegate = CustomWebBrowserControlEventHandler.IgnoreEvent;
            var functionHash = customEventDelegate.GetFullNameHashCode();
            RemoveEventHandlerToDocument(htmlDocument,
                "oncontextmenu", functionHash, getCustomEventHandler);
        }

        public override dynamic ExecuteJavascript(HTMLDocument htmlDocument, string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            var htmlWindow2 = (htmlDocument.Script as HTMLWindow2);
            return htmlWindow2.execScript(javascriptToExecute, "javascript");
        }

        public override dynamic InvokeJavascript(HTMLDocument htmlDocument, string javascriptToExecute)
        {
            if (!Enabled)
            {
                return null;
            }
            return htmlDocument.parentWindow.execScript(javascriptToExecute);
        }

        public dynamic FindElementByAttributeValue(HTMLDocument htmlDocument, string tagName,
            string attribute, string value)
        {
            if (!Enabled)
            {
                return null;
            }
            var all = htmlDocument.getElementsByTagName(tagName);
            for (var i = 0; i < all.length; i++)
            {
                if ((string)((IHTMLElement)all.item(i)).getAttribute(attribute) == value)
                {
                    return all.item(i);
                }
            }
            return null;
        }

        public IEnumerable<IHTMLElement> FindElementsByAttributeValue(HTMLDocument htmlDocument, string tagName,
            string attribute, string value)
        {
            var ret = new List<IHTMLElement>();
            if (!Enabled)
            {
                return ret;
            }
            var all = htmlDocument.getElementsByTagName(tagName);
            for (var i = 0; i < all.length; i++)
            {
                var str = (string)((IHTMLElement)all.item(i)).getAttribute(attribute);
                if (str != "")
                {
                    if (str == value)
                    {
                        ret.Add((IHTMLElement)all.item(i));
                    }
                }
            }
            return ret;
        }

        public string GetCurrentUrl(HTMLDocument htmlDocument)
        {
            return htmlDocument.location.href;
        }

        public override IHTMLElement GetElementById(HTMLDocument htmlDocument, string controlId)
        {
            return htmlDocument?.getElementById(controlId);
        }

        public IEnumerable<IHTMLElement> GetElementsByCssQuery(HTMLDocument htmlDocument, string cssQuery)
        {
            var comObject =
                ExecuteJavascript(htmlDocument,
                    $"Array.prototype.slice.call(document.querySelectorAll('{cssQuery}'));");
            Type type = comObject.GetType();
            var length = (int)type.InvokeMember("length", BindingFlags.GetProperty, null, comObject, null);

            for (var i = 1; i <= length; i++)
            {
                yield return type.InvokeMember(i.ToString(), BindingFlags.GetProperty, null, comObject,
                    null) as IHTMLElement;
            }
        }

        public object GetGlobalVariable(HTMLDocument htmlDocument, string variable)
        {
            if (!Enabled)
            {
                return null;
            }
            var variablePath = variable.Split('.');
            var i = 0;
            string result = null;
            var variableName = "window";
            while (i < variablePath.Length)
            {
                variableName = variableName + "." + variablePath[i];
                if ((result = (string)EvaluateExpression(htmlDocument, variableName)) == null) return null;
                i++;
            }
            return result;
        }

        public object EvaluateExpression(HTMLDocument htmlDocument, string variableName)
        {
            var script =
                "if(document.all['_GlobalVariable_']==null){var input = document.createElement('input'); input.setAttribute('type', 'hidden'); " +
                "input.setAttribute('name', '_GlobalVariable_');input.setAttribute('id', '_GlobalVariable_'); " +
                "document.body.appendChild(input); } document.all['_GlobalVariable_'].value=''+" + $"eval({variableName});";
            ExecuteJavascript(htmlDocument, script);
            var htmlInputHiddenElement = htmlDocument.getElementById("_GlobalVariable_") as HTMLInputElement;
            var result = htmlInputHiddenElement?.value;
            if (string.IsNullOrEmpty(result) || result == "undefined")
            {
                return null;
            }
            return result;
        }

        public void RemoveHandlersOnNavigating(HTMLDocument webBrowser,
            Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
            Action<CustomWebBrowserControlEventHandler> setCustomEventHandlerr)
        {
            var htmlWindowEvents2 = webBrowser.parentWindow as HTMLWindowEvents_Event;
            if (htmlWindowEvents2 != null)
            {
                htmlWindowEvents2.onbeforeunload +=
                    new NavigatingInterceptor(this, getCustomEventHandler, webBrowser)
                        .WebBrowserOnNavigating;
            }
        }

        public object InvokeScript(HTMLDocument htmlDocument_, string scriptName, params object[] args)
        {
            var htmlDocument = htmlDocument_ as IHTMLDocument2;
            object invokeScript = null;
            var pDispParams = new DispParams
            {
                rgvarg = IntPtr.Zero
            };
            try
            {
                var script = htmlDocument.Script as IDispatch;
                if (script == null)
                {
                    return null;
                }
                var empty = Guid.Empty;
                string[] rgszNames = { scriptName };
                int[] rgDispId = { -1 };
                var iDsOfNames = script.GetIDsOfNames(ref empty, rgszNames, 1,
                    Win32Interop.GetThreadLCID(), rgDispId);
                if (!MsHtmlNativeUtility.Succeeded(iDsOfNames) || rgDispId[0] == -1)
                {
                    return null;
                }
                if (args != null)
                {
                    Array.Reverse(args);
                }
                pDispParams.rgvarg = args == null ? IntPtr.Zero : MsHtmlNativeUtility.ArrayToVariantVector(args);
                pDispParams.cArgs = args?.Length ?? 0;
                pDispParams.rgdispidNamedArgs = IntPtr.Zero;
                pDispParams.cNamedArgs = 0;
                var pVarResult = new object[1];
                var pExcepInfo = new ExcepInfo();
                if (script.Invoke(rgDispId[0], ref empty, Win32Interop.GetThreadLCID(), 1, pDispParams, pVarResult,
                        pExcepInfo, null) == 0)
                {
                    invokeScript = pVarResult[0];
                }
            }
            catch (Exception exception)
            {
                if (MsHtmlNativeUtility.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (pDispParams.rgvarg != IntPtr.Zero)
                {
                    if (args != null)
                    {
                        MsHtmlNativeUtility.FreeVariantVector(pDispParams.rgvarg, args.Length);
                    }
                }
            }
            return invokeScript;
        }

        private class NavigatingInterceptor
        {
            private readonly Func<CustomWebBrowserControlEventHandler> _getCustomEventHandler;
            private readonly HTMLDocument _webBrowser;
            private readonly WebBrowserExtensionMsHtmlDocument _webBrowserExtensionMsHtmlDocument;

            public NavigatingInterceptor(WebBrowserExtensionMsHtmlDocument webBrowserExtensionMsHtmlDocument,
                Func<CustomWebBrowserControlEventHandler> getCustomEventHandler,
                HTMLDocument webBrowser)
            {
                _getCustomEventHandler = getCustomEventHandler;
                _webBrowser = webBrowser;
                _webBrowserExtensionMsHtmlDocument = webBrowserExtensionMsHtmlDocument;
            }

            public void WebBrowserOnNavigating()
            {
                var eventHandler = _getCustomEventHandler();
                if (eventHandler == null)
                {
                    return;
                }
                var eventHandlerDelegates = eventHandler.Delegates;
                foreach (var eventHandlerDelegate in eventHandlerDelegates)
                {
                    var eventName = eventHandlerDelegate.Item1;
                    if (eventName.StartsWith($"{_webBrowserExtensionMsHtmlDocument.DocumentEventPrefix}."))
                    {
                        _webBrowserExtensionMsHtmlDocument.RemoveEventHandlerToDocument(_webBrowser, eventName,
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                    else
                    {
                        var strings = eventName.Split('.');
                        _webBrowserExtensionMsHtmlDocument.RemoveEventHandlerToControl(_webBrowser, strings[0],
                            strings[1],
                            eventHandlerDelegate.Item3,
                            _getCustomEventHandler);
                    }
                }
            }
        }
    }
}