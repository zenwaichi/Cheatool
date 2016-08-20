using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using Cheatool.Helpers;

namespace Cheatool
{
    /// <summary>
    /// All Cheatool management
    /// </summary>
    public class MCheat
    {
        private Memory _memory { get; set; }
        public Process MProcess { get; set; }
               
        /// <summary>
        /// Assigns the process to open it and read/edit addresses
        /// </summary>
        /// <param name="process"></param>
        public MCheat(Process process)
        {
            _memory = new Memory(process);
            MProcess = process;
        }
        public MCheat(string processName)
        {
            Process[] processList = Process.GetProcessesByName(processName);
            Process p = processList.OrderByDescending(process => process.PrivateMemorySize64).First();
            _memory = new Memory(p);
            MProcess = p;
        }

        /// <summary>
        /// Get the matching addresses (Based on SigScan and Never Nop Tech)
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public async Task<IntPtr[]> AoBScanAsync(string pattern)
        {
            if (!pattern.Contains("?"))
                return await _memory.AoBScan(pattern);
            else throw new Exception("To use wildcards we need a base address AoBScanAsync(pattern, basseAddress)");
        }
        public async Task<IntPtr[]> AoBScanAsync(string pattern, uint baseAddress, int lenght = 10000)
        {
            return await _memory.AoBScan(pattern, baseAddress, lenght);
        }

        /// <summary>
        /// Read any type corresponding to the address
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="process"></param>
        /// <param name="address"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public T ReadValue<T>(string address)
        {
            IntPtr addressParse = _memory.GetAddress(address);
            object value = _memory.ReadValue<T>(addressParse);
            return (T)value;

        }
        public T ReadValue<T>(IntPtr address)
        {
            object value = _memory.ReadValue<T>(address);
            return (T)value;
        }

        /// <summary>
        /// Replaces a value corresponding to the address
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="replace"></param>
        public void ReplaceValue<T>(string address, T replacement,
            Action<ReplaceValueEventArgs> callback = null)
        {
            IntPtr addressValue = _memory.GetAddress(address);

            callback?.Invoke(new ReplaceValueEventArgs(ReadValue<T>(addressValue), 
                replacement, addressValue));
            _memory.ReplaceValue(addressValue, replacement);
        }
        public void ReplaceValue<T>(IntPtr address, T replacement,
            Action<ReplaceValueEventArgs> callback = null)
        {
            callback?.Invoke(new ReplaceValueEventArgs(ReadValue<T>(address),
                 replacement, address));
            _memory.ReplaceValue(address, replacement);
        }
    }
}