using System;
using System.Linq;
using System.Reflection;

namespace WebBrowserLib.EventHandling
{
    public static class EventHandlerAttacher
    {
        public static EventInfo GetEventInfo(object objectToAttachHandler, string eventName)
        {
            var eventInfos = objectToAttachHandler?.GetType().GetEvents();
            var eventInfo = eventInfos
                ?.FirstOrDefault(e =>
                {
                    var lower = e.Name.ToLower();
                    var s = eventName.ToLower();
                    var endsWith = lower.EndsWith(s);
                    return endsWith;
                });
            return eventInfo;
        }

        public static MethodInfo SearchMethodInfoHandlingEvent(object objectToAttachHandler, string eventName,
            object objectHavingHandler, string methodNameToAttach, out EventInfo eventInfo, out Type type)
        {
            // Find the handler method
            var method = objectHavingHandler.GetType().GetMethod
            (methodNameToAttach,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            // Subscribe to the event
            eventInfo = GetEventInfo(objectToAttachHandler, eventName);
            type = eventInfo?.EventHandlerType;
            return method;
        }
    }
}