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
        public static extern bool VirtualProtectEx(IntPtr process, IntPtr address,
            uint size, uint access, out uint oldProtect);

        [DllImport("KERNEL32.DLL")]
        public static extern int CloseHandle(IntPtr objectHandle);

        [DllImport("KERNEL32.DLL")]
        public static extern IntPtr OpenProcess(uint access, bool inheritHandler, uint processId);

        [Flags]
        public enum Protection
        {
            PEReadWrite = 0x40,
            PReadWrite = 0x04
        }

        [Flags]
        public enum Access
        {
            Synchronize = 0x100000,
            StandardRightsRequired = 0x000F0000,
            AllAccess = StandardRightsRequired | Synchronize | 0xFFFF
        }
    }
}
