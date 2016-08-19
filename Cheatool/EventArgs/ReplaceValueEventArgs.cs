using System;

namespace Cheatool
{
    public class ReplaceValueEventArgs : EventArgs
    {
        /// <summary>
        /// The address where the value was change
        /// </summary>
        public IntPtr Address { get; set; }
        public object PreviousValue { get; set; }
        public object CurrentValue { get; set; }

        /// <summary>
        /// Returns memory edition args
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <param name="address"></param>
        public ReplaceValueEventArgs(object previous, object current, IntPtr address)
        {
            PreviousValue = previous;
            CurrentValue = current;
            Address = address;
        }

        /// <summary>
        /// Returns all the arguments
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Address: {Address.ToString("x8").ToUpper()}, " +
                $"Previous value: {PreviousValue}, Current value: {CurrentValue}";
        }
    }
}