using System;
using System.Diagnostics;

namespace WebBrowserLib.Helpers
{
    public static class ScriptHelper
    {
        public static dynamic GetGlobalVariable(string variable, Func<string, object> injectAndExecuteJavascript)
        {
            var variablePath = variable.Split('.');
            var i = 0;
            object result = null;
            var variableName = "window";
            while (i < variablePath.Length)
            {
                variableName = variableName + "." + variablePath[i];
                result = EvaluateExpression(variableName, injectAndExecuteJavascript);
                if(result==null)
                {
                    return null;
                }
                i++;
            }
            return result;
        }

        public static object EvaluateExpression(string expression, Func<string, object> injectAndExecuteJavascript)
        {
            object result = null;
            try
            {
                result = injectAndExecuteJavascript(expression);
            }
            catch (Exception)
            {
                return result;
            }
            return result;
        }

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