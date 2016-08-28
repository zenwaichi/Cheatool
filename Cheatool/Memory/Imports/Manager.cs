using System;
using System.Runtime.InteropServices;

namespace Cheatool.Memory.Imports
{
    public abstract class Manager
    {
        [DllImport("KERNEL32.DLL", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr process, IntPtr address, byte[] buffer, uint size, ref uint written);

        [DllImport("KERNEL32.DLL")]
        public static extern int CloseHandle(IntPtr objectHandle);

        [DllImport("KERNEL32.DLL")]
        public static extern IntPtr OpenProcess(
           [MarshalAs(UnmanagedType.U4)]ProcessAccess access,
           [MarshalAs(UnmanagedType.Bool)]bool inheritHandler, uint processId);

        [Flags]
        public enum ProcessAccess
        {
            Synchronize = 0x100000,
            StandardRightsRequired = 0x000F0000,
            AllAccess = StandardRightsRequired | Synchronize | 0xFFFF
        }

    }
}
