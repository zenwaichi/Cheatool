using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cheatool.Memory.Imports
{
    public abstract class Reader
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("KERNEL32.DLL")]
        public static extern int VirtualQueryEx(IntPtr hProcess,
            IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        [DllImport("KERNEL32.DLL", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr process, IntPtr address, byte[] buffer, uint size, ref uint read);

        public List<MEMORY_BASIC_INFORMATION> MemoryRegion = new List<MEMORY_BASIC_INFORMATION>();
    }
}
