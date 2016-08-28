using System;
using System.Text;

using Cheatool.Memory.Imports;

namespace Cheatool.Memory
{
    public class Inspector : Reader
    {
        IntPtr _processHandle;

        public Inspector(IntPtr processHadle)
        {
            _processHandle = processHadle;
        }

        public byte[] AoByte(IntPtr address, byte[] buff)
        {
            uint read = 0;
            if(!ReadProcessMemory(_processHandle, address, buff, (uint)buff.Length, ref read) ||
                read != buff.Length)
                throw new AccessViolationException("We couldn't read in this memory address");

            return buff;
        }

        public string Text(IntPtr address, int size = 16)
        {
            byte[] buff = new byte[size];
            AoByte(address, buff);            
            return Encoding.UTF8.GetString(buff);
        }

        public bool Bool(IntPtr address)
        {
            byte[] buff = new byte[sizeof(bool)];
            AoByte(address, buff);
            return BitConverter.ToBoolean(buff, 0);
        }

        public ushort UShort(IntPtr address)
        {
            byte[] buff = new byte[sizeof(ushort)];
            AoByte(address, buff);
            return BitConverter.ToUInt16(buff, 0);
        }

        public short Short(IntPtr address)
        {
            byte[] buff = new byte[sizeof(short)];
            AoByte(address, buff);
            return BitConverter.ToInt16(buff, 0);
        }

        public uint UInt(IntPtr address)
        {
            byte[] buff = new byte[sizeof(uint)];
            AoByte(address, buff);
            return BitConverter.ToUInt32(buff, 0);
        }

        public int Int(IntPtr address)
        {
            byte[] buff = new byte[sizeof(int)];
            AoByte(address, buff);
            return BitConverter.ToInt32(buff, 0);
        }

        public ulong ULong(IntPtr address)
        {
            byte[] buff = new byte[sizeof(ulong)];
            AoByte(address, buff);
            return BitConverter.ToUInt64(buff, 0);
        }

        public long Long(IntPtr address)
        {
            byte[] buff = new byte[sizeof(long)];
            AoByte(address, buff);
            return BitConverter.ToInt64(buff, 0);
        }

        public double Double(IntPtr address)
        {
            byte[] buff = new byte[sizeof(double)];
            AoByte(address, buff);
            return BitConverter.ToDouble(buff, 0);
        }

        public float Float(IntPtr address)
        {
            byte[] buff = new byte[sizeof(float)];
            AoByte(address, buff);
            return BitConverter.ToSingle(buff, 0);
        }

        public decimal Decimal(IntPtr address)
        {
            byte[] buff = new byte[sizeof(decimal)];
            AoByte(address, buff);
            return BitConverter.ToInt32(buff, 0);
        }
    }
}
