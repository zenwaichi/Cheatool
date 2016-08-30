using System;
using System.Linq;
using System.Text;

using Cheatool.Memory.Imports;

namespace Cheatool.Memory
{
    public class Editor : Manager
    {
        IntPtr _processHandle;

        public Editor(IntPtr processHandle)
        {
            _processHandle = processHandle;
        }

        bool CanWrite(IntPtr address, byte[] buff)
        {
            uint old, write = 0;

            return (!VirtualProtectEx(_processHandle, address, (uint)buff.Length, (uint)Protection.PEReadWrite, 
                out old) || !WriteProcessMemory(_processHandle, address, buff, (uint)buff.Length, ref write) ||
                write != buff.Length);
        }

        public void AoByte(IntPtr address, byte[] buff)
        {
            if (CanWrite(address, buff))
                throw new AccessViolationException("We couldn't write in this memory address");
        }

        public void Text(IntPtr address, string replacement)
        {
            byte[] buff = Encoding.UTF8.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void Boolean(IntPtr address, bool replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void UShort(IntPtr address, ushort replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void Short(IntPtr address, short replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void UInt(IntPtr address, uint replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void Int(IntPtr address, int replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void ULong(IntPtr address, ulong replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void Long(IntPtr address, long replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void Double(IntPtr address, double replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void Float(IntPtr address, float replacement)
        {
            byte[] buff = BitConverter.GetBytes(replacement);
            AoByte(address, buff);
        }

        public void Decimal(IntPtr address, decimal replacement)
        {
            byte[] buff = decimal.GetBits(replacement).SelectMany(x =>
            BitConverter.GetBytes(x)).ToArray();
            AoByte(address, buff);
        }        
    }
}
