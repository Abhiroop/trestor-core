using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TNetD.Helpers
{
    public static class HighResolutionDateTime
    {
        public static bool IsAvailable { get; private set; }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static long UtcNow
        {
            get
            {
                if(!IsAvailable)
                {
                    throw new InvalidOperationException("High Resolution Clock isn't available");
                }

                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);

                return filetime;
            }
        }

        static HighResolutionDateTime()
        {
            try
            {
                long filetime;
                GetSystemTimePreciseAsFileTime(out filetime);
                IsAvailable = true;
            }
            catch (EntryPointNotFoundException)
            {             
                // Not running Windows 8 or higher.             
                IsAvailable = false;
            }
        }
    }
}
