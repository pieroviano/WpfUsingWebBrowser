using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WebBrowserLib.MsHtml.Utility.Win32
{
    [ComImport]
    [Guid("00020400-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDispatch
    {
        int GetTypeInfoCount();

        [return: MarshalAs(UnmanagedType.Interface)]
        ITypeInfo GetTypeInfo([In] [MarshalAs(UnmanagedType.U4)] int iTInfo,
            [In] [MarshalAs(UnmanagedType.U4)] int lcid);

        [PreserveSig]
        int GetIDsOfNames([In] ref Guid riid, [In] [MarshalAs(UnmanagedType.LPArray)] string[] rgszNames,
            [In] [MarshalAs(UnmanagedType.U4)] int cNames, [In] [MarshalAs(UnmanagedType.U4)] int lcid,
            [Out] [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);

        [PreserveSig]
        int Invoke(int dispIdMember, [In] ref Guid riid, [In] [MarshalAs(UnmanagedType.U4)] int lcid,
            [In] [MarshalAs(UnmanagedType.U4)] int dwFlags, [In] [Out] DispParams pDispParams,
            [Out] [MarshalAs(UnmanagedType.LPArray)] object[] pVarResult, [In] [Out] ExcepInfo pExcepInfo,
            [Out] [MarshalAs(UnmanagedType.LPArray)] IntPtr[] pArgErr);
    }

}