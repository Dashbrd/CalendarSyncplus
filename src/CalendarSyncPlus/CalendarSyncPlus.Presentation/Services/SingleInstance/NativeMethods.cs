#region Imports

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

#endregion

namespace CalendarSyncPlus.Presentation.Services.SingleInstance
{
    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        #region Delegates

        /// <summary>
        ///     Delegate declaration that matches WndProc signatures.
        /// </summary>
        public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);

        #endregion

        [DllImport("shell32.dll", EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode)]
        private static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine,
            out int numArgs);

        [DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
        private static extern IntPtr _LocalFree(IntPtr hMem);

        public static string[] CommandLineToArgvW(string cmdLine)
        {
            var argv = IntPtr.Zero;
            try
            {
                var numArgs = 0;

                argv = _CommandLineToArgvW(cmdLine, out numArgs);
                if (argv == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                var result = new string[numArgs];

                for (var i = 0; i < numArgs; i++)
                {
                    var currArg = Marshal.ReadIntPtr(argv, i*Marshal.SizeOf(typeof (IntPtr)));
                    result[i] = Marshal.PtrToStringUni(currArg);
                }

                return result;
            }
            finally
            {
                var p = _LocalFree(argv);
                // Otherwise LocalFree failed.
                // Assert.AreEqual(IntPtr.Zero, p);
            }
        }
    }
}