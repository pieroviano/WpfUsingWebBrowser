using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace WebBrowserLib.Extensions
{
    public static class DelegateExtender
    {
        public static int GetCalculatedHashValue(this string inputString)
        {
            var sb = 0;
            var bytes = GetHash(inputString);
            for (var index = 0; index < bytes.Length; index++)
            {
                var b = bytes[index];
                unchecked
                {
                    var i = (index % 4)<<3;
                    sb += b << i;
                }
            }

            return sb;
        }

        public static int GetFullNameHashCode(this Delegate @delegate)
        {
            return GetFullNameHashCode(@delegate.Method);
        }

        public static int GetFullNameHashCode(this MethodInfo delegateMethod)
        {
            var formattableString = $"{delegateMethod.DeclaringType?.FullName}.{delegateMethod.Name}";
            var fullNameHashCode = GetCalculatedHashValue(formattableString);
            return fullNameHashCode;
        }

        public static byte[] GetHash(this string inputString)
        {
            HashAlgorithm algorithm = MD5.Create(); //or use SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
    }
}