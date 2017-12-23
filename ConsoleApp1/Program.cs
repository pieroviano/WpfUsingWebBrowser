using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ConsoleApp1
{
    namespace Demo
    {
        internal class WindowsByClassFinder
        {
            public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

            private readonly StringBuilder _apiResult = new StringBuilder(1024);

            private readonly string _className;
            private readonly List<IntPtr> _result = new List<IntPtr>();

            private WindowsByClassFinder(string className)
            {
                _className = className;
                EnumWindows(callback, IntPtr.Zero);
            }

            private bool callback(IntPtr hWnd, IntPtr lparam)
            {
                if (GetClassName(hWnd, _apiResult, _apiResult.Capacity) != 0)
                {
                    if (string.CompareOrdinal(_apiResult.ToString(), _className) == 0)
                    {
                        _result.Add(hWnd);
                    }
                }

                return true; // Keep enumerating.
            }

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            [SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            [SuppressUnmanagedCodeSecurity]
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

            [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            [SuppressUnmanagedCodeSecurity]
            [DllImport("User32", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern int GetWindowText(IntPtr windowHandle, StringBuilder stringBuilder, int nMaxCount);

            [DllImport("user32.dll", EntryPoint = "GetWindowTextLength", SetLastError = true)]
            internal static extern int GetWindowTextLength(IntPtr hwnd);


            /// <summary>Find the windows matching the specified class name.</summary>
            public static IEnumerable<IntPtr> WindowsMatching(string className)
            {
                return new WindowsByClassFinder(className)._result;
            }

            public static IEnumerable<IntPtr> WindowsMatchingClassName(string className)
            {
                if (string.IsNullOrWhiteSpace(className))
                {
                    throw new ArgumentOutOfRangeException("className", className, "className can't be null or blank.");
                }

                return WindowsMatching(className);
            }

            public static IEnumerable<string> WindowTitlesForClass(string className)
            {
                foreach (var windowHandle in WindowsMatchingClassName(className))
                {
                    var length = GetWindowTextLength(windowHandle);
                    var sb = new StringBuilder(length + 1);
                    GetWindowText(windowHandle, sb, sb.Capacity);
                    yield return sb.ToString();
                }
            }
        }

        internal class Program
        {
            public IEnumerable<string> ChromeWindowTitles()
            {
                foreach (var title in WindowsByClassFinder.WindowTitlesForClass("Chrome_WidgetWin_0"))
                {
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        yield return title;
                    }
                }

                foreach (var title in WindowsByClassFinder.WindowTitlesForClass("Chrome_WidgetWin_1"))
                {
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        yield return title;
                    }
                }
            }

            private static void Main()
            {
                new Program().run();
            }

            private void run()
            {
                ChromeWindowTitles().Print();
            }
        }

        internal static class DemoUtil
        {
            public static void Print(this object self)
            {
                Console.WriteLine(self);
            }

            public static void Print(this string self)
            {
                Console.WriteLine(self);
            }

            public static void Print<T>(this IEnumerable<T> self)
            {
                foreach (var item in self)
                {
                    Console.WriteLine(item);
                }
            }
        }
    }
}