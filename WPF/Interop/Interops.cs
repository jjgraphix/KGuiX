using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace KGuiX.Interop
{
    internal static class Interops
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryStatusEx
        {
            public uint  Length;
            public uint  MemoryLoad;
            public ulong TotalPhys;
            public ulong AvailPhys;
            public ulong TotalPageFile;
            public ulong AvailPageFile;
            public ulong TotalVirtual;
            public ulong AvailVirtual;
            public ulong AvailExtendedVirtual;

            public MemoryStatusEx()
            {
                Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
                MemoryLoad = 0;
                TotalPhys = 0;
                AvailPhys = 0;
                TotalPageFile = 0;
                AvailPageFile = 0;
                TotalVirtual = 0;
                AvailVirtual = 0;
                AvailExtendedVirtual = 0;
            }
        }

        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        public const int HWND_BROADCAST = 0xffff;

        /// <summary>
        /// Retrieves information about the system's current usage of both physical and virtual memory.
        /// </summary>
        /// <param name="lpBuffer">The <see cref="MemoryStatusEx"/> structure that receives information about current memory availability.</param>
        /// <returns>
        /// <br>If the function succeeds, the return value is true.</br>
        /// <br>If the function fails, the return value is false.</br>
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx([In, Out] ref MemoryStatusEx lpBuffer);

        /// <summary>
        /// Posts a message in the message queue associated with the thread that created the specified window
        /// and returns without waiting for the thread to process the message.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure is to receive the message.</param>
        /// <param name="Msg">The message to be posted.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>
        /// <br>If the function succeeds, the return value is nonzero.</br>
        /// <br>If the function fails, the return value is zero.</br>
        /// </returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Defines a new window message that is guaranteed to be unique throughout the system.
        /// </summary>
        /// <param name="lpString">The message to be registered.</param>
        /// <returns>
        /// <br>If the message is successfully registered, the return value is a message identifier in the range 0xC000 through 0xFFFF.</br>
        /// <br>If the function fails, the return value is zero.</br>
        /// </returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegisterWindowMessage(string lpString);

        /// <summary>
        /// Sets the minimum and maximum working set sizes for the specified process.
        /// </summary>
        /// <param name="hProcess">A handle to the process.</param>
        /// <param name="dwMinimumWorkingSetSize">The minimum working set size for the process, in bytes.</param>
        /// <param name="dwMaximumWorkingSetSize">The maximum working set size for the process, in bytes.</param>
        /// <returns>
        /// <br>If the function succeeds, the return value is nonzero.</br>
        /// <br>If the function fails, the return value is zero.</br>
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);

        public static bool SetWorkingSetSize(Process process, long minWorkingSetSize, long maxWorkingSetSize)
        {
            // Can empty working set in most win versions with min and max values of -1
            IntPtr dwMinimumWorkingSetSize = new IntPtr(minWorkingSetSize);     // Min working set in bytes
            IntPtr dwMaximumWorkingSetSize = new IntPtr(maxWorkingSetSize);     // Max working set in bytes

            bool wsIsSet = SetProcessWorkingSetSize(process.Handle, dwMinimumWorkingSetSize, dwMaximumWorkingSetSize);

            return wsIsSet;

            // if (!wsIsSet)    // DEBUG
            // {
                // int error = Marshal.GetLastWin32Error();
                // throw new Exception($"Failed to set working set size: {error}");
            // }
        }
    }
}