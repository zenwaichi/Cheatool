//Some methods based on NNT (Never Nop Tech) Code

using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Cheatool.Helpers
{
    /// <summary>
    /// Represents an access to a remote process memory
    /// </summary>
    sealed class Memory : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr process, IntPtr address, byte[] buffer, uint size, ref uint written);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr process, IntPtr address, byte[] buffer, uint size, ref uint read);

        [Flags]
        public enum ProcessAccessType
        {
            PROCESS_TERMINATE = (0x0001),
            PROCESS_CREATE_THREAD = (0x0002),
            PROCESS_SET_SESSIONID = (0x0004),
            PROCESS_VM_OPERATION = (0x0008),
            PROCESS_VM_READ = (0x0010),
            PROCESS_VM_WRITE = (0x0020),
            PROCESS_DUP_HANDLE = (0x0040),
            PROCESS_CREATE_PROCESS = (0x0080),
            PROCESS_SET_QUOTA = (0x0100),
            PROCESS_SET_INFORMATION = (0x0200),
            PROCESS_QUERY_INFORMATION = (0x0400)
        }

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr objectHandle);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
           [MarshalAs(UnmanagedType.U4)]ProcessAccessType access,
           [MarshalAs(UnmanagedType.Bool)]bool inheritHandler, uint processId);

        private Process process;
        private IntPtr processHandle;
        private bool isDisposed;

        public const string OffsetPattern = "(\\+|\\-){0,1}(0x){0,1}[a-fA-F0-9]{1,}";

        /// <summary>
        /// Initializes a new instance of the Memory
        /// </summary>
        /// <param name="process">Remote process</param>
        public Memory(Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            this.process = process;
            processHandle = OpenProcess(
                ProcessAccessType.PROCESS_VM_READ | ProcessAccessType.PROCESS_VM_WRITE |
                ProcessAccessType.PROCESS_VM_OPERATION, true, (uint)process.Id);
            if (processHandle == IntPtr.Zero)
                throw new InvalidOperationException("Could not open the process");
        }

        #region IDisposable

        ~Memory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;
            CloseHandle(processHandle);
            process = null;
            processHandle = IntPtr.Zero;
            isDisposed = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the process to which this memory is attached to
        /// </summary>
        public Process Process
        {
            get
            {
                return process;
            }
        }

        #endregion

        /// <summary>
        /// Finds module with the given name
        /// </summary>
        /// <param name="name">Module name</param>
        /// <returns></returns>
        private ProcessModule FindModule(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.ToLower() == name.ToLower())
                    return module;
            }
            return null;
        }

        /// <summary>
        /// Gets module based address
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="baseAddress">Base address</param>
        /// <param name="offsets">Collection of offsets</param>
        /// <returns></returns>
        public IntPtr GetAddress(string moduleName, IntPtr baseAddress, int[] offsets)
        {
            if (string.IsNullOrEmpty(moduleName))
                throw new ArgumentNullException("moduleName");

            ProcessModule module = FindModule(moduleName);
            if (module == null)
                return IntPtr.Zero;
            else
            {
                int address = module.BaseAddress.ToInt32() + baseAddress.ToInt32();
                return GetAddress((IntPtr)address, offsets);
            }
        }

        /// <summary>
        /// Gets address
        /// </summary>
        /// <param name="baseAddress">Base address</param>
        /// <param name="offsets">Collection of offsets</param>
        /// <returns></returns>
        public IntPtr GetAddress(IntPtr baseAddress, int[] offsets)
        {
            if (baseAddress == IntPtr.Zero)
                throw new ArgumentException("Invalid base address");

            int address = baseAddress.ToInt32();

            if (offsets != null && offsets.Length > 0)
            {
                byte[] buffer = new byte[4];
                foreach (int offset in offsets)
                    address = ReadValue<int>((IntPtr)address) + offset;
            }

            return (IntPtr)address;
        }

        /// <summary>
        /// Gets address pointer
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns></returns>
        public IntPtr GetAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");

            string moduleName = null;
            int index = address.IndexOf('"');

            if (index != -1)
            {
                int endIndex = address.IndexOf('"', index + 1);
                if (endIndex == -1)
                    throw new ArgumentException("Invalid module name. Could not find matching \"");
                moduleName = address.Substring(index + 1, endIndex - 1);
                address = address.Substring(endIndex + 1);
            }

            int[] offsets = GetAddressOffsets(address);
            int[] _offsets = null;

            IntPtr baseAddress = offsets != null && offsets.Length > 0 ?
                (IntPtr)offsets[0] : IntPtr.Zero;

            if (offsets != null && offsets.Length > 1)
            {
                _offsets = new int[offsets.Length - 1];
                for (int i = 0; i < offsets.Length - 1; i++)
                    _offsets[i] = offsets[i + 1];
            }

            if (moduleName != null)
                return GetAddress(moduleName, baseAddress, _offsets);
            else
                return GetAddress(baseAddress, _offsets);
        }

        /// <summary>
        /// Gets address offsets
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns></returns>
        private static int[] GetAddressOffsets(string address)
        {
            if (string.IsNullOrEmpty(address))
                return new int[0];
            else
            {
                MatchCollection matches = Regex.Matches(address, OffsetPattern);
                int[] offsets = new int[matches.Count];
                string value;
                char ch;
                for (int i = 0; i < matches.Count; i++)
                {
                    ch = matches[i].Value[0];
                    if (ch == '+' || ch == '-')
                        value = matches[i].Value.Substring(1);
                    else
                        value = matches[i].Value;
                    offsets[i] = Convert.ToInt32(value, 16);
                    if (ch == '-')
                        offsets[i] = -offsets[i];
                }
                return offsets;
            }
        }

        #region ReadValues

        /// <summary>
        /// Reads memory at the address
        /// </summary>
        /// <param name="address">Memory address</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size in bytes</param>
        public void ReadMemory(IntPtr address, byte[] buffer, int size)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Memory");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (size <= 0)
                throw new ArgumentException("Size must be greater than zero");
            if (address == IntPtr.Zero)
                throw new ArgumentException("Invalid address");

            uint read = 0;
            if (!ReadProcessMemory(processHandle, address, buffer, (uint)size, ref read) ||
                read != size)
                throw new AccessViolationException();
        }

        /// <summary>
        /// Read any type corresponding to the address
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="process"></param>
        /// <param name="address"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public T ReadValue<T>(IntPtr address)
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        byte[] booleanBuffer = new byte[sizeof(bool)];
                        ReadMemory(address, booleanBuffer, sizeof(bool));
                        object booleanValue = BitConverter.ToBoolean(booleanBuffer, 0);
                        return (T)booleanValue;

                    case TypeCode.UInt16:
                        byte[] ushortBuffer = new byte[sizeof(ushort)];
                        ReadMemory(address, ushortBuffer, sizeof(ushort));
                        object ushortValue = BitConverter.ToUInt16(ushortBuffer, 0);
                        return (T)ushortValue;

                    case TypeCode.Int16:
                        byte[] shortBuffer = new byte[sizeof(short)];
                        ReadMemory(address, shortBuffer, sizeof(short));
                        object shortValue = BitConverter.ToInt16(shortBuffer, 0);
                        return (T)shortValue;

                    case TypeCode.UInt32:
                        byte[] uintBuffer = new byte[sizeof(uint)];
                        ReadMemory(address, uintBuffer, sizeof(uint));
                        object uintValue = BitConverter.ToUInt32(uintBuffer, 0);
                        return (T)uintValue;

                    case TypeCode.Int32:
                        byte[] intBuffer = new byte[sizeof(int)];
                        ReadMemory(address, intBuffer, sizeof(int));
                        object intValue = BitConverter.ToInt32(intBuffer, 0);
                        return (T)intValue;

                    case TypeCode.UInt64:
                        byte[] ulongBuffer = new byte[sizeof(ulong)];
                        ReadMemory(address, ulongBuffer, sizeof(ulong));
                        object ulongValue = BitConverter.ToUInt64(ulongBuffer, 0);
                        return (T)ulongValue;

                    case TypeCode.Int64:
                        byte[] longBuffer = new byte[sizeof(long)];
                        ReadMemory(address, longBuffer, sizeof(long));
                        object longValue = BitConverter.ToInt64(longBuffer, 0);
                        return (T)longValue;

                    case TypeCode.Double:
                        byte[] doubleBuffer = new byte[8];
                        ReadMemory(address, doubleBuffer, 8);
                        object doubleValue = BitConverter.ToDouble(doubleBuffer, 0);
                        return (T)doubleValue;

                    case TypeCode.Decimal:
                        byte[] decimalBuffer = new byte[sizeof(decimal)];
                        ReadMemory(address, decimalBuffer, sizeof(decimal));
                        object decimalValue = BitConverter.ToInt32(decimalBuffer, 0);
                        return (T)decimalValue;

                    default:
                        byte[] floatBuffer = new byte[4];
                        ReadMemory(address, floatBuffer, 4);
                        object floatValue = BitConverter.ToSingle(floatBuffer, 0);
                        return (T)floatValue;
                }
            }
            catch (Exception e)
            {
                throw new Exception("You're trying to access to a private/damaged memory", e);
            }
        }

        #endregion

        #region WriteValues

        /// <summary>
        /// Writes memory at the address
        /// </summary>
        /// <param name="address">Memory address</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="size">Size in bytes</param>
        public void ReplaceMemory(IntPtr address, byte[] buffer, int size)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Memory");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (size <= 0)
                throw new ArgumentException("Size must be greater than zero");
            if (address == IntPtr.Zero)
                throw new ArgumentException("Invalid address");

            uint write = 0;
            if (!WriteProcessMemory(processHandle, address, buffer, (uint)size, ref write) ||
                write != size)
                throw new AccessViolationException();
        }

        /// <summary>
        /// Replaces a value corresponding to the address
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="replace"></param>
        public void ReplaceValue<T>(IntPtr address, T replacement)
        {
            object replacementValue = replacement;

            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        bool booleanValue = (bool)replacementValue;
                        byte[] booleanBuffer = BitConverter.GetBytes(booleanValue);
                        ReplaceMemory(address, booleanBuffer, sizeof(bool));
                        break;

                    case TypeCode.UInt16:
                        ushort ushortValue = (ushort)replacementValue;
                        byte[] ushortBuffer = BitConverter.GetBytes(ushortValue);
                        ReplaceMemory(address, ushortBuffer, sizeof(ushort));
                        break;

                    case TypeCode.Int16:
                        short shortValue = (short)replacementValue;
                        byte[] shortBuffer = BitConverter.GetBytes(shortValue);
                        ReplaceMemory(address, shortBuffer, sizeof(short));
                        break;

                    case TypeCode.UInt32:
                        uint uintValue = uint.Parse(replacement.ToString());
                        byte[] uintBuffer = BitConverter.GetBytes(uintValue);
                        ReplaceMemory(address, uintBuffer, sizeof(uint));
                        break;

                    case TypeCode.Int32:
                        int intValue = int.Parse(replacement.ToString());
                        byte[] intBuffer = BitConverter.GetBytes(intValue);
                        ReplaceMemory(address, intBuffer, sizeof(int));
                        break;

                    case TypeCode.UInt64:
                        ulong ulongValue = ulong.Parse(replacement.ToString());
                        byte[] ulongBuffer = BitConverter.GetBytes(ulongValue);
                        ReplaceMemory(address, ulongBuffer, sizeof(ulong));
                        break;

                    case TypeCode.Int64:
                        long longValue = long.Parse(replacement.ToString());
                        byte[] longBuffer = BitConverter.GetBytes(longValue);
                        ReplaceMemory(address, longBuffer, sizeof(long));
                        break;

                    case TypeCode.Double:
                        double doubleValue = double.Parse(replacement.ToString());
                        byte[] doubleBuffer = BitConverter.GetBytes(doubleValue);
                        ReplaceMemory(address, doubleBuffer, sizeof(double));
                        break;

                    case TypeCode.Decimal:
                        decimal decimalValue = decimal.Parse(replacement.ToString());
                        byte[] decimalBuffer = decimal.GetBits(decimalValue).SelectMany(x
                        => BitConverter.GetBytes(x)).ToArray();
                        ReplaceMemory(address, decimalBuffer, sizeof(decimal));
                        break;

                    default:
                        float floatValue = float.Parse(replacement.ToString());
                        byte[] floatBuffer = BitConverter.GetBytes(floatValue);
                        ReplaceMemory(address, floatBuffer, sizeof(float));
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception("You're trying to access to a private/damaged memory", e);
            }
        }

        #endregion

        [DllImport("kernel32.dll")]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private List<MEMORY_BASIC_INFORMATION> MemoryRegion { get; set; }

        private void MemInfo(IntPtr pHandle)
        {
            IntPtr Addy = new IntPtr();
            while (true)
            {
                MEMORY_BASIC_INFORMATION MemInfo = new MEMORY_BASIC_INFORMATION();
                int MemDump = VirtualQueryEx(pHandle, Addy, out MemInfo, Marshal.SizeOf(MemInfo));
                if (MemDump == 0) break;
                if ((MemInfo.State & 0x1000) != 0 && (MemInfo.Protect & 0x100) == 0)
                    MemoryRegion.Add(MemInfo);
                Addy = new IntPtr(MemInfo.BaseAddress.ToInt32() + (int)MemInfo.RegionSize);
            }
        }
        private IntPtr Scan(byte[] sIn, byte[] sFor)
        {
            int[] sBytes = new int[256]; int Pool = 0;
            int End = sFor.Length - 1;

            for (int i = 0; i < 256; i++)
                sBytes[i] = sFor.Length;

            for (int i = 0; i < End; i++)
                sBytes[sFor[i]] = End - i;

            while (Pool <= sIn.Length - sFor.Length)
            {
                for (int i = End; sIn[Pool + i] == sFor[i]; i--)
                    if (i == 0)
                        return new IntPtr(Pool);

                Pool += sBytes[sIn[Pool + End]];
            }

            return IntPtr.Zero;
        }

        public async Task<IntPtr[]> AoBScan(string pattern, uint baseAddress, int lenght)
        {
            MemoryRegion = new List<MEMORY_BASIC_INFORMATION>();
            List<IntPtr> address = new List<IntPtr>();
            MemInfo(process.Handle);

            try
            {
                return await Task.Run(async () =>
                {
                    IntPtr resultSig = IntPtr.Zero;

                    for (int i = 0; i < MemoryRegion.Count; i++)
                    {
                        resultSig = await SigScan.AoBScan(process, baseAddress++,
                            lenght, pattern);

                        if (resultSig != IntPtr.Zero && !address.Contains(resultSig))
                            address.Add(resultSig);
                    }
                    return address.ToArray();
                });
            }
            catch (OperationCanceledException e)
            {
                throw new Exception(e.ToString());
            }
        }

        public async Task<IntPtr[]> AoBScan(string pattern)
        {

            MemoryRegion = new List<MEMORY_BASIC_INFORMATION>();
            List<IntPtr> address = new List<IntPtr>();
            MemInfo(Process.Handle);

            string[] strPattern = pattern.Split(' ');
            byte[] bPattern = new byte[strPattern.Length];
            uint read = 0, readp = 0;

            try
            {
                return await Task.Run(() =>
                {
                    foreach (var by in strPattern)
                        bPattern[(int)readp++] = byte.Parse(by, System.Globalization.NumberStyles.HexNumber);

                    for (int i = 0; i < MemoryRegion.Count; i++)
                    {
                        byte[] buff = new byte[MemoryRegion[i].RegionSize];
                        ReadProcessMemory(Process.Handle, MemoryRegion[i].BaseAddress, buff, MemoryRegion[i].RegionSize,
                            ref read);

                        IntPtr Result = Scan(buff, bPattern);

                        if (Result != IntPtr.Zero && !address.Contains(Result))
                            address.Add(new IntPtr(MemoryRegion[i].BaseAddress.ToInt32() + Result.ToInt32()));
                    }

                    return address.ToArray();
                });
            }
            catch (OperationCanceledException e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}