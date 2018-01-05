using System.Runtime.InteropServices;

namespace WebBrowserLib.MsHtml.Utility
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FindSizeOfVariant
    {
        [MarshalAs(UnmanagedType.Struct)] public readonly object var;
        public readonly byte b;
    }
}