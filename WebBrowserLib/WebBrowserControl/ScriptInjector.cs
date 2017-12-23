using System.Windows.Forms;
using mshtml;

namespace WebBrowserLib.WebBrowserControl
{
    namespace StarbExplorer.Utility
    {
        public class ScriptInjector
        {
            public static void AddJQueryElement(HtmlElement head)
            {
                var scriptEl = head.Document?.CreateElement("script");
                var jQueryElement = (IHTMLScriptElement) scriptEl.DomElement;
                jQueryElement.src = @"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";

                head.AppendChild(scriptEl);
            }

            public static void AddJQueryElement(HTMLHeadElement head)
            {
                var scriptEl = (head.ownerDocument as HTMLDocument)?.createElement("script") as HTMLScriptElement;
                var jQueryElement = (IHTMLScriptElement) scriptEl;
                if (jQueryElement != null)
                {
                    jQueryElement.src = @"http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js";
                }

                head.appendChild((IHTMLDOMNode) scriptEl);
            }

            public static void AddScriptElement(HtmlElement head, string scriptBody)
            {
                var scriptEl = head.Document?.CreateElement("script");
                if (scriptEl != null)
                {
                    scriptEl.InnerHtml = scriptBody;

                    head.AppendChild(scriptEl);
                }
            }

            public static void AddScriptElement(HTMLHeadElement head, string scriptBody)
            {
                var scriptEl = (head.ownerDocument as HTMLDocument)?.createElement("script") as HTMLScriptElement;
                if (scriptEl != null)
                {
                    scriptEl.innerHTML = scriptBody;

                    head.appendChild((IHTMLDOMNode) scriptEl);
                }
            }
        }
    }
}