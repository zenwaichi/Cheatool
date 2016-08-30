using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Cheatool.Memory.Imports;

namespace Cheatool.Memory
{
    public class BoyerMoore : Reader
    {
        private IntPtr _processHandle;

        public BoyerMoore(IntPtr processHandle)
        {
            _processHandle = processHandle;
        }

        private void MemInfo(bool unwritable)
        {
            IntPtr Addy = new IntPtr();

            while (true)
            {
                MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();

                int MemDump = VirtualQueryEx(_processHandle, Addy, out memInfo, Marshal.SizeOf(memInfo));

                if (MemDump == 0) break;

                if (!unwritable && (memInfo.State & 0x1000) != 0 && (memInfo.Protect & 0xCC) != 0)
                    MemoryRegion.Add(memInfo);
                else if ((memInfo.State & 0x1000) != 0)
                    MemoryRegion.Add(memInfo);

                Addy = new IntPtr(memInfo.BaseAddress.ToInt32() + (int)memInfo.RegionSize);
            }
        }

        private void BoyerAlgo(IntPtr baseAddress, byte[] memoryBrick, byte[] pattern, ref List<IntPtr> addresses)
        {
            int offSet = 0;
            while ((offSet = Array.IndexOf(memoryBrick, pattern[0], offSet)) != -1)
            {
                if (pattern.Length > 1)
                    for (int i = 1; i < pattern.Length; i++)
                    {
                        if (memoryBrick.Length <= offSet + pattern.Length
                            || pattern[i] != memoryBrick[offSet + i]) break;

                        if (i == pattern.Length - 1)
                            addresses.Add(new IntPtr((int)baseAddress + offSet));
                    }
                else addresses.Add(new IntPtr((int)baseAddress + offSet));
                offSet++;
            }
        }

        private void BoyerAlgo(IntPtr baseAddress, byte[] memoryBrick, string pattern, ref List<IntPtr> addresses)
        {
            int offSet = 0;
            string[] aob = pattern.Split(' ');
            List<int> bytesPos = new List<int>();

            for (int i = 0; i < aob.Length; i++)
                if (aob[i] != "??")
                    bytesPos.Add(i);

            if (bytesPos.Count != 0)
                while ((offSet = Array.IndexOf(memoryBrick, (byte)Convert.ToInt32(aob[bytesPos[0]], 16), offSet)) != -1)
                {
                    if (bytesPos.Count > 1)
                        for (int i = 1; i < bytesPos.Count; i++)
                        {
                            if (memoryBrick.Length <= offSet + pattern.Length
                                || (byte)Convert.ToInt32(aob[bytesPos[i]], 16)
                                != memoryBrick[(offSet - bytesPos[0]) + bytesPos[i]]) break;

                            if (i == bytesPos.Count - 1)
                                if (aob[0] == "??")
                                    addresses.Add(new IntPtr((int)baseAddress + (offSet - bytesPos[0])));
                                else addresses.Add(new IntPtr((int)baseAddress + offSet));
                        }
                    else
                        addresses.Add(new IntPtr((int)baseAddress + (offSet - bytesPos[0])));
                    offSet++;
                }
            else
                for (int i = 0; i < memoryBrick.Length; i++)
                    addresses.Add(new IntPtr((int)baseAddress + i));
        }

        public async Task<IntPtr[]> AoByte(string pattern, bool unwritable = false)
        {
            if (!pattern.Contains("?"))
            {
                byte[] buff = pattern.Split(' ').Select(by =>
                (byte)Convert.ToInt32(by, 16)).ToArray();
                return await Task.Run(() => { return GeneralScan(buff, unwritable); });
            }
            else return await Task.Run(() => { return WCScan(pattern, unwritable); });
        }

        public async Task<IntPtr[]> Text(string value, bool unwritable = false)
        {
            byte[] buff = Encoding.UTF8.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> Boolean(bool value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> UShort(ushort value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> Short(short value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> UInteger(uint value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> Integer(int value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> ULong(ulong value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> Long(long value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> Double(double value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> Float(float value, bool unwritable = false)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        public async Task<IntPtr[]> Decimal(decimal value, bool unwritable = false)
        {
            byte[] buff = decimal.GetBits(value).SelectMany(x =>
            BitConverter.GetBytes(x)).ToArray();

            return await Task.Run(() => { return GeneralScan(buff, unwritable); });
        }

        private IntPtr[] GeneralScan(byte[] buff, bool unwritable)
        {
            MemInfo(unwritable);

            List<IntPtr> addresses = new List<IntPtr>();

            for (int i = 0; i < MemoryRegion.Count; i++)
            {
                uint read = 0;
                byte[] wholeMemory = new byte[MemoryRegion[i].RegionSize];

                ReadProcessMemory(_processHandle, MemoryRegion[i].BaseAddress, wholeMemory,
                    MemoryRegion[i].RegionSize, ref read);

                BoyerAlgo(MemoryRegion[i].BaseAddress, wholeMemory, buff, ref addresses);
            }
            return addresses.ToArray();
        }

        private IntPtr[] WCScan(string pattern, bool unwritable)
        {
            MemInfo(unwritable);

            List<IntPtr> addresses = new List<IntPtr>();

            for (int i = 0; i < MemoryRegion.Count; i++)
            {
                uint read = 0;
                byte[] wholeMemory = new byte[MemoryRegion[i].RegionSize];

                ReadProcessMemory(_processHandle, MemoryRegion[i].BaseAddress, wholeMemory,
                    MemoryRegion[i].RegionSize, ref read);

                BoyerAlgo(MemoryRegion[i].BaseAddress, wholeMemory, pattern, ref addresses);
            }
            return addresses.ToArray();
        }
    }
}