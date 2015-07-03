using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OpenJigWare
{
    partial class Ojw
    {
        // if you make your class, just write in here
        public class CSystem
        {
            [DllImport("kernel32.dll")]
            public static extern bool IsWow64Process(System.IntPtr hProcess, out bool lpSystemInfo);

            public static bool Is64Bits_byApi()
            {                
                bool retVal = false;
                IsWow64Process(System.Diagnostics.Process.GetCurrentProcess().Handle, out retVal);
                return retVal;
            }
            public static bool Is32Bits_byApi()
            {
                bool retVal = false;
                IsWow64Process(System.Diagnostics.Process.GetCurrentProcess().Handle, out retVal);
                return !retVal;
            }

            public static bool Is64Bits()
             {
                 if ((IntPtr.Size == 8) || ((IntPtr.Size == 4) && (Is64Bits_byApi() == true))) return true;
                 return false;
             }
            public static bool Is32Bits() { return !Is64Bits(); }
        }
    }
}
