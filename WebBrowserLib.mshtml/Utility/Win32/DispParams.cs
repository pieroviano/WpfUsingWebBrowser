using System;
using System.Runtime.InteropServices;

namespace WebBrowserLib.MsHtml.Utility
{
    [StructLayout(LayoutKind.Sequential)]
    public sealed class DispParams
    {
        [MarshalAs(UnmanagedType.U4)] public int cArgs;

        [MarshalAs(UnmanagedType.U4)] public int cNamedArgs;

        public IntPtr rgdispidNamedArgs;
        public IntPtr rgvarg;
    }
}