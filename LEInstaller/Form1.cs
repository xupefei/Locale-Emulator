using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using LEInstaller.Properties;

namespace LEInstaller
{
    public partial class Form1 : Form
    {
        private readonly string crtDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public Form1()
        {
            // We need to remove all ADS first.
            // https://github.com/xupefei/Locale-Emulator/issues/22.
            foreach (var f in Directory.GetFiles(crtDir, "*", SearchOption.AllDirectories))
            {
                RemoveADS(f);
            }

            InitializeComponent();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            IndicateBusy();

            KillExplorer();

            ReplaceDll(true);

            DoRegister();

            StartExplorer();

            IndicateBusy(true);

            MessageBox.Show("Install finished. Right click any executable and enjoy :)",
                            "LE Context Menu Installer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int DeleteFile(string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);

        private void RemoveADS(string s)
        {
            File.SetAttributes(s, FileAttributes.Normal);

            if (DeleteFile(s + ":Zone.Identifier") == 0)
            {
                SetLastError(0);
            }
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            IndicateBusy();

            KillExplorer();

            ReplaceDll(false);

            DoUnRegister();

            // Clean up CLSID
            /*var key = Registry.ClassesRoot;
            try
            {
                key.DeleteSubKeyTree(@"\CLSID\{C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE}\");
            }
            catch
            {
            }
            finally
            {
                key.Close();
            }*/

            StartExplorer();
            
            IndicateBusy(true);

            MessageBox.Show("Uninstall finished. Thanks for using Locale Emulator :)",
                            "LE Context Menu Installer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void IndicateBusy(bool finish = false)
        {
            if (!finish)
            {
                Cursor.Current = Cursors.WaitCursor;

                buttonInstall.Enabled = false;
                buttonUninstall.Enabled = false;
            }
            else
            {
                Cursor.Current = Cursors.Default;

                buttonInstall.Enabled = true;
                buttonUninstall.Enabled = true;
            }
        }

        private void DoRegister()
        {
            try
            {
                OverrideHKCR();

                var rs = new RegistrationServices();
                rs.RegisterAssembly(Assembly.LoadFrom(Path.Combine(crtDir, @"LEContextMenuHandler.dll")), AssemblyRegistrationFlags.SetCodeBase);

                OverrideHKCR(true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\n\r\n" + e.StackTrace);
            }
        }

        private void DoUnRegister()
        {
            try
            {
                OverrideHKCR();

                var rs = new RegistrationServices();
                rs.UnregisterAssembly(Assembly.LoadFrom(Path.Combine(crtDir, @"LEContextMenuHandler.dll")));

                OverrideHKCR(true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\n\r\n" + e.StackTrace);
            }
        }

        private void OverrideHKCR(bool restore = false)
        {
            UIntPtr HKEY_CLASSES_ROOT = Is64BitOS() ? new UIntPtr(0xFFFFFFFF80000000) : new UIntPtr(0x80000000);
            UIntPtr HKEY_CURRENT_USER = Is64BitOS() ? new UIntPtr(0xFFFFFFFF80000001) : new UIntPtr(0x80000001);
            
            // 0xF003F = KEY_ALL_ACCESS
            UIntPtr key = UIntPtr.Zero;

            RegOpenKeyEx(HKEY_CURRENT_USER, @"Software\Classes", 0, 0xF003F, out key);
            RegOverridePredefKey(HKEY_CLASSES_ROOT, restore?UIntPtr.Zero:key);
        }

        private bool ReplaceDll(bool overwrite)
        {
            var dllPath1 = Path.Combine(crtDir, @"LEContextMenuHandler.dll");
            var dllPath2 = Path.Combine(crtDir, @"LECommonLibrary.dll");

            if (!overwrite)
            {
                if (!File.Exists(dllPath1))
                    File.WriteAllBytes(dllPath1, Resources.LEContextMenuHandler);
                if (!File.Exists(dllPath2))
                    File.WriteAllBytes(dllPath1, Resources.LECommonLibrary);

                    return true;
            }

            try
            {
                File.Delete(dllPath1);
                File.Delete(dllPath2);

                File.WriteAllBytes(dllPath1, Resources.LEContextMenuHandler);
                File.WriteAllBytes(dllPath2, Resources.LECommonLibrary);

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\nPlease try install/uninstall again.");
                return false;
            }
        }

        private void KillExplorer()
        {
            const int WM_USER = 0x0400;

            try
            {
                var ptr = FindWindow("Shell_TrayWnd", null);

                PostMessage(ptr, WM_USER + 436, (IntPtr)0, (IntPtr)0);

                // wait until exit
                while (ptr.ToInt32() != 0)
                {
                    Thread.Sleep(1000);

                    ptr = FindWindow("Shell_TrayWnd", null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void StartExplorer()
        {
            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\explorer.exe",
                        UseShellExecute = true
                    }
                };

                process.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }
        
        // We should not use the LECommonLibrary.
        private static string GetLEVersion()
        {
            try
            {
                var versionPath =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                 "LEVersion.xml");

                var doc = XDocument.Load(versionPath);

                return doc.Descendants("LEVersion").First().Attribute("Version").Value;
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        private static bool Is64BitOS()
        {
            //The code below is from http://1code.codeplex.com/SourceControl/changeset/view/39074#842775
            //which is under the Microsoft Public License: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.

            if (UIntPtr.Size == 8) // 64-bit programs run only on Win64
            {
                return true;
            }
            // Detect whether the current process is a 32-bit process 
            // running on a 64-bit system.
            bool flag;
            return DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                   IsWow64Process(GetCurrentProcess(), out flag) && flag;
        }

        private static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            var moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == UIntPtr.Zero)
            {
                return false;
            }
            return GetProcAddress(moduleHandle, methodName) != UIntPtr.Zero;
        }

        [DllImport("kernel32.dll")]
        private static extern UIntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern UIntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern UIntPtr GetProcAddress(UIntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(UIntPtr hProcess, out bool wow64Process);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, uint samDesired, out UIntPtr hkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegOverridePredefKey(UIntPtr hKey, UIntPtr hNewKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(UIntPtr hKey);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Environment.OSVersion.Version.CompareTo(new Version(6, 0)) <= 0)
            {
                MessageBox.Show("Sorry, Locale Emulator is only for Windows 7, 8/8.1 and 10.",
                                "OS Outdated",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                Environment.Exit(0);
            }

            if (!File.Exists(Path.Combine(Environment.SystemDirectory + @"\..\Fonts\msgothic.ttc")))
            {
                if (MessageBox.Show(
                                    "Japanese font \"MS Gothic\" (\"ＭＳ ゴシック\") font is not found on\r\n" +
                                    "your system. If you use LE on Japanese applications, you may consider \r\n" +
                                    "installing \"Japanese Supplemental Fonts\" package.\r\n" +
                                    "\r\n" +
                                    "Do you want to read the installation guide?",
                                    "Font missing warning",
                                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Process.Start("http://www.tenforums.com/tutorials/7565-optional-features-manage-windows-10-a.html");
                }
            }
            
            Text += " - V" + GetLEVersion();
        }
    }
}