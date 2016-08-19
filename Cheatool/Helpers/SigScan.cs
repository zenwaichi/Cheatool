//SigScan classe wrote by at0mos (I guess)

using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cheatool.Helpers
{
    sealed class SigScan
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            UIntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
            );

        private byte[] m_vDumpedRegion;
        private Process m_vProcess;
        private UIntPtr m_vAddress;
        private int m_vSize;

        public int Size
        {
            get { return m_vSize; }
            set { m_vSize = value; }
        }

        public Process Process
        {
            get { return m_vProcess; }
            set { m_vProcess = value; }
        }

        public UIntPtr Address
        {
            get { return m_vAddress; }
            set { m_vAddress = value; }
        }

        public void ResetRegion()
        {
            m_vDumpedRegion = null;
        }

        public SigScan()
        {
            m_vProcess = null;
            m_vAddress = UIntPtr.Zero;
            m_vSize = 0;
            m_vDumpedRegion = null;
        }

        public SigScan(Process proc, UIntPtr addr, int size)
        {
            m_vProcess = proc;
            m_vAddress = addr;
            m_vSize = size;
        }

        private bool DumpMemory()
        {
            try
            {
                if (m_vProcess == null || m_vProcess.HasExited || m_vAddress == UIntPtr.Zero || m_vSize == 0)
                    return false;

                m_vDumpedRegion = new byte[m_vSize];

                int nBytesRead; 
                var ret = ReadProcessMemory(m_vProcess.Handle, m_vAddress,
                    m_vDumpedRegion, m_vSize, out nBytesRead);

                return ret && nBytesRead == this.m_vSize;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool MaskCheck(int nOffset, IEnumerable<byte> btPattern, string strMask)
        => !btPattern.Where((t, x) => strMask[x] != '?' && ((strMask[x] == 'x')
        && (t != m_vDumpedRegion[nOffset + x]))).Any();

        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
        {
            try
            {
                if (m_vDumpedRegion == null || m_vDumpedRegion.Length == 0)
                    if (!DumpMemory())
                        return IntPtr.Zero;

                if (strMask.Length != btPattern.Length)
                    return IntPtr.Zero;

                for (int x = 0; x < m_vDumpedRegion.Length; x++)
                    if (MaskCheck(x, btPattern, strMask))
                        return new IntPtr((int)m_vAddress + (x + nOffset));

                return IntPtr.Zero;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        public async static Task<IntPtr> AoBScan(Process process, uint min, int length, string aob)
        {
            return await Task.Run(() =>
             {
                 string[] stringByteArray = aob.Split(' ');
                 byte[] myPattern = new byte[stringByteArray.Length];
                 string mask = "";
                 int i = 0;
                 foreach (string ba in stringByteArray)
                 {
                     if (ba == "??")
                     {
                         myPattern[i] = 0xFF;
                         mask += "?";
                     }
                     else
                     {
                         myPattern[i] = byte.Parse(ba, NumberStyles.HexNumber);
                         mask += "x";
                     }
                     i++;
                 }

                 SigScan _sigScan = new SigScan(process, new UIntPtr(min), length);
                 IntPtr pAddr = _sigScan.FindPattern(myPattern, mask, 0);
                 return pAddr;
             });

        }
    }
}