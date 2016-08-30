using System;
using System.Linq;
using System.Diagnostics;

using Cheatool.Memory;
using Cheatool.Memory.Imports;

namespace Cheatool
{
    public class Cheatbox : Manager
    {
        public Inspector Inspect { get; set; }
        public BoyerMoore BoyerScan { get; set; }
        public Editor Editor { get; set;}

        IntPtr _processHandle;

        public Cheatbox(Process process)
        {
            _processHandle = OpenProcess((uint)Access.AllAccess, false, (uint)process.Id);

            Inspect = new Inspector(_processHandle);
            BoyerScan = new BoyerMoore(_processHandle);
            Editor = new Editor(_processHandle);
        }

        public Cheatbox(string processName)
        {
            Process[] processList = Process.GetProcessesByName(processName);
            Process p = processList.OrderByDescending(process => process.PrivateMemorySize64).First();
            
            _processHandle = OpenProcess((uint)Access.AllAccess, false, (uint)p.Id);

            Inspect = new Inspector(_processHandle);
            BoyerScan = new BoyerMoore(_processHandle);
            Editor = new Editor(_processHandle);
        }

        ~Cheatbox()
        {
            CloseHandle(_processHandle);
        }
    }
}