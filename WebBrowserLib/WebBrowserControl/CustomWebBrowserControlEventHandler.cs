using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using WebBrowserLib.Extensions;

namespace WebBrowserLib.WebBrowserControl
{
    public class CustomWebBrowserControlEventHandler
    {
        public CustomWebBrowserControlEventHandler(List<Tuple<string, Delegate, int>> customEventDelegate)
        {
            Delegates = customEventDelegate;
        }

        public List<Tuple<string, Delegate, int>> Delegates { get; set; }

        public bool EventHandlerIsAttached(string formattableString, int functionHash, Type eventInfoEventHandlerType,
            object firstArgument, Delegate customEventDelegate, out Delegate @delegate)
        {
            @delegate = GetDelegate(formattableString, eventInfoEventHandlerType, firstArgument, customEventDelegate,
                functionHash);
            var contains = Delegates.ContainsKey(formattableString, functionHash) &&
                           Delegates.Items(formattableString, @delegate, functionHash).Any();
            return contains;
        }

        private Delegate GetCurrentDelegateForFullEventName(string fullEventName, int functionHash)
        {
            IEnumerable<Tuple<string, Delegate, int>> returnValues = null;
            if (Delegates.ContainsKey(fullEventName, functionHash))
            {
                returnValues = Delegates.Items(fullEventName, functionHash);
            }
            Delegate @delegate = null;
            if (returnValues != null)
            {
                foreach (var returnValue in returnValues)
                {
                    if (functionHash ==
                        returnValue.Item3)
                    {
                        @delegate = returnValue.Item2;
                        break;
                    }
                }
            }
            return @delegate;
        }

        public Delegate GetDelegate(string fullEventName, Type eventHandlerType, object objectHavingHandler,
            Delegate customEventDelegate, int functionHash)
        {
            var type = typeof(Handler);
            var @delegate = GetCurrentDelegateForFullEventName(fullEventName, functionHash);
            if (@delegate == null)
            {
                if (customEventDelegate == null)
                {
                    var handler = new Handler(Delegates, fullEventName, functionHash);
                    var methodInfo = type.GetMethod("HandleEvent", BindingFlags.Instance | BindingFlags.NonPublic);
                    Debug.Assert(methodInfo != null);
                    @delegate = Delegate.CreateDelegate(eventHandlerType, handler, methodInfo);
                    handler.Delegate = @delegate;
                }
                else if (objectHavingHandler != null)
                {
                    var methodInfo = customEventDelegate.Method;
                    @delegate = Delegate.CreateDelegate(eventHandlerType, objectHavingHandler, methodInfo);
                }
                else
                {
                    var methodInfo = customEventDelegate.Method;
                    @delegate = Delegate.CreateDelegate(eventHandlerType, methodInfo);
                }
            }
            return @delegate;
        }

        public static int GetFunctionPointerFromDelegate()
        {
            var customEventDelegate =
                typeof(Handler).GetMethod("HandleEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            if (customEventDelegate != null)
            {
                return customEventDelegate.GetFullNameHashCode();
            }
            return 0;
        }

        public static bool IgnoreEvent()
        {
            return false;
        }

        public void TrackHandler(string formattableString, Delegate @delegate, int functionHash)
        {
            Delegates.Add(formattableString, @delegate, functionHash);
        }

        public void UntrackHandler(string formattableString, Delegate @delegate, int functionHash)
        {
            if (Delegates.ContainsKey(formattableString, functionHash) &&
                Delegates.Items(formattableString, @delegate, functionHash) != null)
            {
                Delegates.Remove(formattableString, @delegate, functionHash);
            }
        }

        public class Handler
        {
            private readonly List<Tuple<string, Delegate, int>> _eventDelegate;
            private readonly string _formattableString;
            private readonly int _functionPointer;

            public Handler(List<Tuple<string, Delegate, int>> eventDelegate, string formattableString, int functionHash)
            {
                _eventDelegate = eventDelegate;
                _formattableString = formattableString;
                _functionPointer = functionHash;
            }

            public Delegate Delegate { get; set; }

            internal bool HandleEvent()
            {
                var enumerable = _eventDelegate.Items(_formattableString, Delegate, _functionPointer);

                var handleEvent = !_eventDelegate.ContainsKey(_formattableString, _functionPointer);
                if (handleEvent)
                {
                    return true;
                }
                handleEvent = true;
                foreach (var tuple in enumerable)
                {
                    var tupleItem2 = tuple.Item2;
                    handleEvent &= (bool) (tupleItem2?.DynamicInvoke() ?? true);
                }
                return handleEvent;
            }
        }
    }
}