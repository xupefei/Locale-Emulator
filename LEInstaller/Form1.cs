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
using System.Security.Principal;

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
                RemoveADS(f);

            DisableDPIScale();

            InitializeComponent();
        }

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

            Text += " - V" + GetLEVersion();

            if (IsAdministrator())
            {
                Text += " (Administrator)";

                buttonInstall.Enabled = false;
            }

            DeleteOldFiles();
            ReplaceDll();

            AddShieldToButton(buttonInstallAllUsers);
            AddShieldToButton(buttonUninstallAllUsers);

            buttonUninstall.Enabled = ShellExtensionManager.IsInstalled(false);
            buttonUninstallAllUsers.Enabled = ShellExtensionManager.IsInstalled(true);
            buttonEditGlobal.Enabled = ShellExtensionManager.IsInstalled(true) || ShellExtensionManager.IsInstalled(false);
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            DoRegister(false);

            NotifyShell();

            MessageBox.Show("Install finished. Right click any executable and enjoy :)\r\n" +
                            "\r\n" +
                            "PS: A reboot (or restart of \"explorer.exe\") is required if you are upgrading from an old version.",
                "LE Context Menu Installer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            buttonUninstall.Enabled = true;
            buttonEditGlobal.Enabled = true;
        }

        private void buttonInstallAllUsers_Click(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("Please run this application as administrator and try again.",
                    "LE Context Menu Installer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            DoRegister(true);

            NotifyShell();

            MessageBox.Show("Install finished. Right click any executable and enjoy :)\r\n" +
                            "\r\n" +
                            "PS: A reboot (or restart of \"explorer.exe\") is required if you are upgrading from an old version.",
                "LE Context Menu Installer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            buttonUninstallAllUsers.Enabled = true;
            buttonEditGlobal.Enabled = true;
        }

        private void buttonEditGlobal_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(crtDir, "LEGUI.exe"));
            Application.Exit();
        }

        public static void DisableDPIScale()
        {
            SetProcessDPIAware();
        }

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
            DoUnRegister(false);

            NotifyShell();

            MessageBox.Show("Uninstall finished. Thanks for using Locale Emulator :)",
                "LE Context Menu Installer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            buttonUninstall.Enabled = false;
        }

        private void buttonUninstallAllUsers_Click(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("Please run this application as administrator and try again.",
                    "LE Context Menu Installer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            DoUnRegister(true);

            NotifyShell();

            MessageBox.Show("Uninstall finished. Thanks for using Locale Emulator :)",
                "LE Context Menu Installer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            buttonUninstallAllUsers.Enabled = false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void DoRegister(bool allUsers)
        {
            try
            {
                if (!allUsers)
                    OverrideHKCR();

                var rs = new RegistrationServices();
                rs.RegisterAssembly(Assembly.LoadFrom(Path.Combine(crtDir, @"LEContextMenuHandler.dll")),
                    AssemblyRegistrationFlags.SetCodeBase);

                ShellExtensionManager.RegisterShellExtContextMenuHandler(allUsers);

                if (!allUsers)
                    OverrideHKCR(true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\r\n\r\n" + e.StackTrace);
            }
        }

        private void DoUnRegister(bool allUsers)
        {
            try
            {
                if (!allUsers)
                    OverrideHKCR();

                var rs = new RegistrationServices();
                rs.UnregisterAssembly(Assembly.LoadFrom(Path.Combine(crtDir, @"LEContextMenuHandler.dll")));

                ShellExtensionManager.UnregisterShellExtContextMenuHandler(allUsers);

                if (!allUsers)
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
            RegOverridePredefKey(HKEY_CLASSES_ROOT, restore ? UIntPtr.Zero : key);
        }

        private void DeleteOldFiles()
        {
            foreach (var file in Directory.EnumerateFiles(crtDir, "*.installer.bak"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }
        }

        private bool ReplaceDll()
        {
            var dllPath1 = Path.Combine(crtDir, @"LEContextMenuHandler.dll");
            var dllPath2 = Path.Combine(crtDir, @"LECommonLibrary.dll");

            // try delete old files. If failed, rename them
            try
            {
                File.Delete(dllPath1);
                File.Delete(dllPath2);
            }
            catch (Exception)
            {
                try
                {
                    File.Move(dllPath1, $"{Guid.NewGuid()}.installer.bak");
                    File.Move(dllPath2, $"{Guid.NewGuid()}.installer.bak");
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message + "\r\nPlease try rebooting your PC.");
                    return false;
                }
            }

            // Write new files
            try
            {
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

        private void NotifyShell()
        {
            const uint SHCNE_ASSOCCHANGED = 0x08000000;
            const ushort SHCNF_IDLIST = 0x0000;

            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        static internal void AddShieldToButton(Button b)
        {
            const uint BCM_FIRST = 0x1600; //Normal button
            const uint BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
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

        public static bool IsAdministrator()
        {
            var wp = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int DeleteFile(string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void SetLastError(int errorCode);

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, ushort uFlags, IntPtr dwItem1, IntPtr dwItem2);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern UIntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern UIntPtr GetProcAddress(UIntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(UIntPtr hProcess, out bool wow64Process);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, uint samDesired,
            out UIntPtr hkResult);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegOverridePredefKey(UIntPtr hKey, UIntPtr hNewKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegCloseKey(UIntPtr hKey);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}