using System.Runtime.InteropServices;

namespace WebBrowserLib.MsHtml.Utility
{
    public static class Win32Interop
    {
        [DllImport("kernel32.dll", EntryPoint = "GetThreadLocale", CharSet = CharSet.Auto)]
        public static extern int GetThreadLCID();

        [DllImport("oleaut32.dll", PreserveSig = false)]
        public static extern void VariantClear(HandleRef pObject);
    }
}