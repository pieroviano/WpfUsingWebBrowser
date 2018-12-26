using System;
using System.Runtime.InteropServices;

namespace WebBrowserLib.MsHtml.Utility.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public class ExcepInfo
    {
        [MarshalAs(UnmanagedType.BStr)] public string bstrDescription;

        [MarshalAs(UnmanagedType.BStr)] public string bstrHelpFile;

        [MarshalAs(UnmanagedType.BStr)] public string bstrSource;

        [MarshalAs(UnmanagedType.U4)] public int dwHelpContext;

        public IntPtr pfnDeferredFillIn = IntPtr.Zero;
        public IntPtr pvReserved = IntPtr.Zero;

        [MarshalAs(UnmanagedType.U4)] public int scode;

        [MarshalAs(UnmanagedType.U2)] public short wCode;

        [MarshalAs(UnmanagedType.U2)] public short wReserved;
    }
}