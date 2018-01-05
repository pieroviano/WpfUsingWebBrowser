using System;
using System.Net;
using System.Reflection;

namespace UsingWebBrowserLib.WebServer
{
    public class ResourceResponseSender
    {

        private string _urlPrefix;
        private Type[] _resourceTypes;

        public ResourceResponseSender(string urlPrefix, params Type[] resourceTypes)
        {
            _urlPrefix = urlPrefix;
            _resourceTypes = resourceTypes;
        }

        public string SendResponse(HttpListenerRequest request)
        {
            var substring = request.Url.ToString().Substring(_urlPrefix.Length).ToLower()
                .Replace(".", "_").Replace("-", "_");
            PropertyInfo propertyInfo = null;
            Type type = null;
            for (int i = 0; i < _resourceTypes.Length; i++)
            {
                type = _resourceTypes[i];
                propertyInfo = type.GetProperty(substring);
                if (propertyInfo != null)
                {
                    break;
                }
            }
            var value = (string)propertyInfo?.GetValue(type);
            return value;
        }
    }
}