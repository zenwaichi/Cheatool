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

        private List<MEMORY_BASIC_INFORMATION> _memoryRegion = new List<MEMORY_BASIC_INFORMATION>();

        private void MemInfo(bool writable)
        {
            IntPtr Addy = new IntPtr();

            while (true)
            {
                MEMORY_BASIC_INFORMATION memInfo = new MEMORY_BASIC_INFORMATION();

                int MemDump = VirtualQueryEx(_processHandle, Addy, out memInfo, Marshal.SizeOf(memInfo));

                if (MemDump == 0) break;

                if (writable)
                {
                    if ((memInfo.State & 0x1000) != 0 && (memInfo.Protect & 0xCC) != 0)
                        _memoryRegion.Add(memInfo);
                }
                else
                {
                    if ((memInfo.State & 0x1000) != 0)
                        _memoryRegion.Add(memInfo);
                }

                Addy = new IntPtr(memInfo.BaseAddress.ToInt32() + (int)memInfo.RegionSize);
            }
        }

        private void Scan(IntPtr baseAddress, byte[] memoryBrick, byte[] pattern, ref List<IntPtr> addresses)
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
                        {
                            addresses.Add(new IntPtr((int)baseAddress + offSet));
                            break;
                        }
                    }
                else addresses.Add(new IntPtr((int)baseAddress + offSet));
                offSet++;
            }
        }

        private void Scan(IntPtr baseAddress, byte[] memoryBrick, string pattern, ref List<IntPtr> addresses)
        {
            //Here's the WildCard scan, currently i've something in hands but it breaks when the WildCards
            //start in the beginning of the pattern. Dont worry, i'm working hard to fix this ;)
        }

        public async Task<IntPtr[]> AoByte(string pattern, bool writable = true)
        {
            byte[] buff = pattern.Split(' ').Select(by =>
            (byte)Convert.ToInt32(by.ToUpper(), 16)).ToArray();

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Text(string value, bool writable = true)
        {
            byte[] buff = Encoding.UTF8.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Boolean(bool value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> UShort(ushort value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Short(short value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> UInteger(uint value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Integer(int value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> ULong(ulong value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Long(long value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Double(double value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Float(float value, bool writable = true)
        {
            byte[] buff = BitConverter.GetBytes(value);

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        public async Task<IntPtr[]> Decimal(decimal value, bool writable = true)
        {
            byte[] buff = decimal.GetBits(value).SelectMany(x =>
            BitConverter.GetBytes(x)).ToArray();

            return await Task.Run(() => { return GeneralScan(buff, writable); });
        }

        private IntPtr[] GeneralScan(byte[] buff, bool writable)
        {
            MemInfo(writable);

            List<IntPtr> addresses = new List<IntPtr>();

            for (int i = 0; i < _memoryRegion.Count; i++)
            {
                uint read = 0;
                byte[] wholeMemory = new byte[_memoryRegion[i].RegionSize];

                ReadProcessMemory(_processHandle, _memoryRegion[i].BaseAddress, wholeMemory,
                    _memoryRegion[i].RegionSize, ref read);

                Scan(_memoryRegion[i].BaseAddress, wholeMemory, buff, ref addresses);
            }
            return addresses.ToArray();
        }
    }
}

