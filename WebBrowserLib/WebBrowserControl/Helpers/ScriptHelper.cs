using System.Diagnostics;

namespace WebBrowserLib.WebBrowserControl.Helpers
{
    public class ScriptHelper
    {
        public static string GetJavascriptToExecuteToRemoveHandlers(string controlId, string cleanHandlers)
        {
            return
                $"var old_element = document.getElementById('{controlId}');{cleanHandlers}var new_element = old_element.cloneNode(true);old_element.parentNode.replaceChild(new_element, old_element); " +
                "}";
        }

        public static string JavascriptBreakIfDebuggerIsAttached()
        {
            if (Debugger.IsAttached)
            {
                return "{debugger;}";
            }
            return "";
        }

        public static string PrepareCleanHandlers(string[] eventNames)
        {
            var str = "if(old_element!=null){";
            foreach (var eventName in eventNames)
            {
                str += $"old_element.{eventName}=null;";
            }
            return str;
        }
    }
}