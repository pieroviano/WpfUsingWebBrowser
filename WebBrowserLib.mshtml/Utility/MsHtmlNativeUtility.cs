using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace WebBrowserLib.MsHtml.Utility
{
    public class MsHtmlNativeUtility
    {
        private static readonly int VariantSize = (int) Marshal.OffsetOf(typeof(FindSizeOfVariant), "b");

        public static bool Succeeded(int hr)
        {
            return hr >= 0;
        }

        internal static unsafe IntPtr ArrayToVariantVector(object[] args)
        {
            var length = args.Length;
            var ptr = Marshal.AllocCoTaskMem(length * VariantSize);
            var numPtr = (byte*) ptr;
            for (var i = 0; i < length; i++)
            {
                Marshal.GetNativeVariantForObject(args[i], (IntPtr) (numPtr + VariantSize * i));
            }
            return ptr;
        }

        internal static unsafe void FreeVariantVector(IntPtr mem, int len)
        {
            var numPtr = (byte*) mem;
            for (var i = 0; i < len; i++)
            {
                Win32Interop.VariantClear(new HandleRef(null, (IntPtr) (numPtr + VariantSize * i)));
            }
            Marshal.FreeCoTaskMem(mem);
        }

        public static bool IsCriticalException(Exception ex)
        {
            return ex is NullReferenceException || ex is StackOverflowException || ex is OutOfMemoryException ||
#pragma warning disable 618
                   ex is ThreadAbortException || ex is ExecutionEngineException || ex is IndexOutOfRangeException ||
#pragma warning restore 618
                   ex is AccessViolationException;
        }

        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            return ex is SecurityException || IsCriticalException(ex);
        }
    }
}