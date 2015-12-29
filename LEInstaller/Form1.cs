using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using LEInstaller.Properties;
using Microsoft.Win32;

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
            KillExplorer();

            ReplaceDll(true);

            #region Do register

            var exe = ExtractRegAsm();

            var psi = new ProcessStartInfo(exe,
                                           $"\"{Path.Combine(crtDir, "LEContextMenuHandler.dll")}\" /codebase")
                      {
                          CreateNoWindow = true,
                          WindowStyle = ProcessWindowStyle.Hidden,
                          RedirectStandardInput = false,
                          RedirectStandardOutput = true,
                          RedirectStandardError = true,
                          UseShellExecute = false
                      };

            var p = Process.Start(psi);

            p.WaitForExit(10000);

            var output = p.StandardOutput.ReadToEnd();
            var error = p.StandardError.ReadToEnd();

            if (output.ToLower().IndexOf("error") != -1 || error.ToLower().IndexOf("error") != -1)
            {
                MessageBox.Show($"==STD_OUT=============\r\n{output}\r\n==STD_ERR=============\r\n{error}");

                return;
            }

            #endregion

            StartExplorer();

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
            KillExplorer();

            ReplaceDll(false);

            #region Do un-register

            var exe = ExtractRegAsm();

            var psi = new ProcessStartInfo(exe,
                                           $"/unregister \"{Path.Combine(crtDir, "LEContextMenuHandler.dll")}\" /codebase")
                      {
                          CreateNoWindow = true,
                          WindowStyle = ProcessWindowStyle.Hidden,
                          RedirectStandardInput = false,
                          RedirectStandardOutput = true,
                          RedirectStandardError = true,
                          UseShellExecute = false
                      };

            var p = Process.Start(psi);

            p.WaitForExit(5000);

            // Clean up CLSID
            var key = Registry.ClassesRoot;
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
            }

            var output = p.StandardOutput.ReadToEnd();
            var error = p.StandardError.ReadToEnd();

            if (output.ToLower().IndexOf("error") != -1 || error.ToLower().IndexOf("error") != -1)
                MessageBox.Show($"==STD_OUT=============\r\n{output}\r\n==STD_ERR=============\r\n{error}");

            #endregion

            StartExplorer();

            MessageBox.Show("Uninstall finished. Thanks for using Locale Emulator :)",
                            "LE Context Menu Installer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private bool ReplaceDll(bool overwrite)
        {
            var dllPath1 = Path.Combine(crtDir, @"LEContextMenuHandler.dll");
            var dllPath2 = Path.Combine(crtDir, @"LECommonLibrary.dll");

            if (!overwrite)
            {
                if (File.Exists(dllPath1) || File.Exists(dllPath2))
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
            try
            {
                foreach (var p in Process.GetProcessesByName("explorer"))
                {
                    p.Kill();
                    p.WaitForExit(5000);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }

        private void StartExplorer()
        {
            try
            {
                Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe",
                              $"/select,{Assembly.GetExecutingAssembly().Location}");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }

        private string ExtractRegAsm()
        {
            try
            {
                var tempFile = Path.GetTempFileName();

                File.WriteAllBytes(tempFile, Is64BitOS() ? Resources.RegAsm64 : Resources.RegAsm);

                RemoveADS(tempFile);

                return tempFile;
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

            if (IntPtr.Size == 8) // 64-bit programs run only on Win64
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
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return GetProcAddress(moduleHandle, methodName) != IntPtr.Zero;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

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
        }
    }
}