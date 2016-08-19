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
               
        /// <summary>
        /// Assigns the process to get open and read/edit addresses
        /// </summary>
        /// <param name="process"></param>
        public MCheat(Process process)
        {
            _memory = new Memory(process);
        }
        public MCheat(string processName)
        {
            Process[] processList = Process.GetProcessesByName(processName);
            Process p = processList.OrderByDescending(process => process.PrivateMemorySize64).First();
            _memory = new Memory(p);
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
        public async Task<IntPtr[]> AoBScanAsync(string pattern, uint baseAddress)
        {
            return await _memory.AoBScan(pattern, baseAddress);
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
            IntPtr addressValue = _memory.GetAddress(address);

            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        object booleanValue = _memory.ReadBoolean(addressValue);
                        return (T)booleanValue;

                    case TypeCode.UInt16:
                        object ushortValue = _memory.ReadUShort(addressValue);
                        return (T)ushortValue;

                    case TypeCode.Int16:
                        object shortValue = _memory.ReadShort(addressValue);
                        return (T)shortValue;

                    case TypeCode.UInt32:
                        object uintegerValue = _memory.ReadUInteger(addressValue);
                        return (T)uintegerValue;

                    case TypeCode.Int32:
                        object integerValue = _memory.ReadInteger(addressValue);
                        return (T)integerValue;

                    case TypeCode.UInt64:
                        object ulongValue = _memory.ReadULong(addressValue);
                        return (T)ulongValue;

                    case TypeCode.Int64:
                        object longValue = _memory.ReadLong(addressValue);
                        return (T)longValue;

                    case TypeCode.Double:
                        object doubleValue = _memory.ReadDouble(addressValue);
                        return (T)doubleValue;

                    case TypeCode.Decimal:
                        object decimalValue = _memory.ReadDecimal(addressValue);
                        return (T)decimalValue;

                    default:
                        object floatValue = _memory.ReadFloat(addressValue);
                        return (T)floatValue;
                }
            }
            catch (Exception e)
            {
                throw new Exception("You're trying to access to a private/damaged memory", e);
            }
        }
        public T ReadValue<T>(IntPtr address)
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        object booleanValue = _memory.ReadBoolean(address);
                        return (T)booleanValue;

                    case TypeCode.UInt16:
                        object ushortValue = _memory.ReadUShort(address);
                        return (T)ushortValue;

                    case TypeCode.Int16:
                        object shortValue = _memory.ReadShort(address);
                        return (T)shortValue;

                    case TypeCode.UInt32:
                        object uintegerValue = _memory.ReadUInteger(address);
                        return (T)uintegerValue;

                    case TypeCode.Int32:
                        object integerValue = _memory.ReadInteger(address);
                        return (T)integerValue;

                    case TypeCode.UInt64:
                        object ulongValue = _memory.ReadULong(address);
                        return (T)ulongValue;

                    case TypeCode.Int64:
                        object longValue = _memory.ReadLong(address);
                        return (T)longValue;

                    case TypeCode.Double:
                        object doubleValue = _memory.ReadDouble(address);
                        return (T)doubleValue;

                    case TypeCode.Decimal:
                        object decimalValue = _memory.ReadDecimal(address);
                        return (T)decimalValue;

                    default:
                        object floatValue = _memory.ReadFloat(address);
                        return (T)floatValue;
                }
            }
            catch (Exception e)
            {
                throw new Exception("You're trying to access to a private/damaged memory", e);
            }
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

            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        bool booleanValue = bool.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<bool>(addressValue),
                            booleanValue, addressValue));

                        _memory.WriteBoolean(addressValue, booleanValue);
                        break;

                    case TypeCode.UInt16:
                        ushort ushortValue = ushort.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<ushort>(addressValue),
                            ushortValue, addressValue));

                        _memory.WriteUShort(addressValue, ushortValue);
                        break;

                    case TypeCode.Int16:
                        short shortValue = short.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<short>(addressValue),
                            shortValue, addressValue));

                        _memory.WriteShort(addressValue, shortValue);
                        break;

                    case TypeCode.UInt32:
                        uint uintValue = uint.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<uint>(addressValue),
                            uintValue, addressValue));

                        _memory.WriteUInteger(addressValue, uintValue);
                        break;

                    case TypeCode.Int32:
                        int intValue = int.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<int>(addressValue),
                            intValue, addressValue));

                        _memory.WriteInteger(addressValue, intValue);
                        break;

                    case TypeCode.UInt64:
                        ulong ulongValue = ulong.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<ulong>(addressValue),
                            ulongValue, addressValue));

                        _memory.WriteULong(addressValue, ulongValue);
                        break;

                    case TypeCode.Int64:
                        long longValue = long.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<long>(addressValue),
                            longValue, addressValue));

                        _memory.WriteLong(addressValue, longValue);
                        break;

                    case TypeCode.Double:
                        double doubleValue = double.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<double>(addressValue),
                            doubleValue, addressValue));

                        _memory.WriteDouble(addressValue, doubleValue);
                        break;

                    case TypeCode.Decimal:
                        decimal decimalValue = decimal.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<decimal>(addressValue),
                            decimalValue, addressValue));

                        _memory.WriteDecimal(addressValue, decimalValue);
                        break;

                    default:
                        float floatValue = float.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<float>(addressValue),
                            floatValue, addressValue));

                        _memory.WriteFloat(addressValue, floatValue);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception("You're trying to access to a private/damaged memory", e);
            }
        }
        public void ReplaceValue<T>(IntPtr address, T replacement,
            Action<ReplaceValueEventArgs> callback = null)
        {
            try
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        bool booleanValue = bool.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<bool>(address),
                            booleanValue, address));

                        _memory.WriteBoolean(address, booleanValue);
                        break;

                    case TypeCode.UInt16:
                        ushort ushortValue = ushort.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<ushort>(address),
                            ushortValue, address));

                        _memory.WriteUShort(address, ushortValue);
                        break;

                    case TypeCode.Int16:
                        short shortValue = short.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<short>(address),
                            shortValue, address));

                        _memory.WriteShort(address, shortValue);
                        break;

                    case TypeCode.UInt32:
                        uint uintValue = uint.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<uint>(address),
                            uintValue, address));

                        _memory.WriteUInteger(address, uintValue);
                        break;

                    case TypeCode.Int32:
                        int intValue = int.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<int>(address),
                            intValue, address));

                        _memory.WriteInteger(address, intValue);
                        break;

                    case TypeCode.UInt64:
                        ulong ulongValue = ulong.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<ulong>(address),
                            ulongValue, address));

                        _memory.WriteULong(address, ulongValue);
                        break;

                    case TypeCode.Int64:
                        long longValue = long.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<long>(address),
                            longValue, address));

                        _memory.WriteLong(address, longValue);
                        break;

                    case TypeCode.Double:
                        double doubleValue = double.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<double>(address),
                            doubleValue, address));

                        _memory.WriteDouble(address, doubleValue);
                        break;

                    case TypeCode.Decimal:
                        decimal decimalValue = decimal.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<decimal>(address),
                            decimalValue, address));

                        _memory.WriteDecimal(address, decimalValue);
                        break;

                    default:
                        float floatValue = float.Parse(replacement.ToString());

                        callback?.Invoke(new ReplaceValueEventArgs(ReadValue<float>(address),
                            floatValue, address));

                        _memory.WriteFloat(address, floatValue);
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception("You're trying to access to a private/damaged memory", e);
            }
        }
    }
}