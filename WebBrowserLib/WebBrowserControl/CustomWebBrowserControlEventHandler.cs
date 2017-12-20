using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using WebBrowserLib.Extensions;

namespace WebBrowserLib.WebBrowserControl
{
    public class CustomWebBrowserControlEventHandler
    {

        public CustomWebBrowserControlEventHandler(List<Tuple<string, Delegate, IntPtr>> customEventDelegate)
        {
            Delegates = customEventDelegate;
        }

        internal List<Tuple<string, Delegate, IntPtr>> Delegates { get; set; }

        public bool EventHandlerIsAttached(string formattableString, IntPtr functionPointer)
        {
            IntPtr functionHash = functionPointer;
            var @delegate = GetDelegate(formattableString, typeof(Func<bool>), functionHash);
            var contains = Delegates.ContainsKey(formattableString, functionPointer) && Delegates.Items(formattableString, @delegate, functionPointer).Any();
            return contains;
        }

        public static IntPtr GetFunctionPointerFromDelegate()
        {
            var customEventDelegate = typeof(Handler).GetMethod("HandleEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            if (customEventDelegate != null)
                return (IntPtr) customEventDelegate.GetHashCode();
            return IntPtr.Zero;
        }

        public Delegate GetDelegate(string formattableString, Type eventHandlerType, IntPtr functionHash)
        {
            var type = typeof(Handler);
            IEnumerable<Tuple<string, Delegate, IntPtr>> returnValues = null;
            if (Delegates.ContainsKey(formattableString, functionHash))
                returnValues = Delegates.Items(formattableString, functionHash);
            var firstArgument = type.GetMethod("HandleEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(firstArgument != null);
            Delegate @delegate = null;
            if (returnValues != null)
                foreach (var returnValue in returnValues)
                {
                    if (functionHash ==
                        returnValue.Item3)
                    {
                        @delegate = returnValue.Item2;
                        break;
                    }
                }
            if (@delegate == null)
            {
                var handler = new Handler(Delegates, formattableString, functionHash);
                @delegate = Delegate.CreateDelegate(eventHandlerType, handler, firstArgument);
                handler.Delegate = @delegate;
            }
            return @delegate;
        }

        public static bool IgnoreEvent()
        {
            return false;
        }

        public void TrackHandler(string formattableString, Delegate @delegate)
        {
            IntPtr functionPointer = (IntPtr)@delegate.Method.GetHashCode();
            Delegates.Add(formattableString, @delegate, functionPointer);
        }

        public void UntrackHandler(string formattableString, Delegate @delegate)
        {
            IntPtr functionPointer = (IntPtr)@delegate.Method.GetHashCode();
            if (Delegates.ContainsKey(formattableString, functionPointer) && Delegates.Items(formattableString, @delegate, functionPointer) != null)
            {
                Delegates.Remove(formattableString, @delegate, functionPointer);
            }
        }

        public class Handler
        {
            private readonly List<Tuple<string, Delegate, IntPtr>> _eventDelegate;
            private readonly string _formattableString;
            private readonly IntPtr _functionPointer;
            public Delegate Delegate { get; set; }

            public Handler(List<Tuple<string, Delegate, IntPtr>> eventDelegate, string formattableString, IntPtr functionPointer)
            {
                _eventDelegate = eventDelegate;
                _formattableString = formattableString;
                _functionPointer = functionPointer;
            }

            internal bool HandleEvent()
            {
                var enumerable = _eventDelegate.Items(_formattableString, Delegate, _functionPointer);

                var handleEvent = !_eventDelegate.ContainsKey(_formattableString, _functionPointer);
                if (handleEvent)
                    return true;
                handleEvent = true;
                foreach (var tuple in enumerable)
                {
                    var tupleItem2 = tuple.Item2;
                    handleEvent &= (bool)(tupleItem2?.DynamicInvoke() ?? true);
                }
                return handleEvent;
            }
        }
    }
}