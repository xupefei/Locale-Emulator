using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using LEInstaller.Properties;
using Microsoft.Win32;

namespace LEInstaller
{
    public partial class Form1 : Form
    {
        private readonly string crtDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private readonly XmlDocument xmlContent = new XmlDocument();
        private bool hasNewVersion;

        public Form1()
        {
            // We need to remove all ADS first.
            // https://github.com/xupefei/Locale-Emulator/issues/22.
            foreach (string f in Directory.GetFiles(crtDir, "*", SearchOption.AllDirectories))
            {
                RemoveADS(f);
            }

            InitializeComponent();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            string exe = ExtractRegAsm();

            var psi = new ProcessStartInfo(exe,
                                           string.Format("\"{0}\" /codebase",
                                                         Path.Combine(crtDir, "LEContextMenuHandler.dll")))
                      {
                          CreateNoWindow = true,
                          WindowStyle = ProcessWindowStyle.Hidden,
                          RedirectStandardInput = false,
                          RedirectStandardOutput = true,
                          RedirectStandardError = true,
                          UseShellExecute = false,
                      };

            Process p = Process.Start(psi);

            p.WaitForExit(10000);

            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            if (output.ToLower().IndexOf("error") != -1 || error.ToLower().IndexOf("error") != -1)
                MessageBox.Show(String.Format("==STD_OUT=============\r\n{0}\r\n==STD_ERR=============\r\n{1}",
                                              output,
                                              error));

            AskForKillExplorer();
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
            string exe = ExtractRegAsm();

            var psi = new ProcessStartInfo(exe,
                                           string.Format("/unregister \"{0}\" /codebase",
                                                         Path.Combine(crtDir, "LEContextMenuHandler.dll")))
                      {
                          CreateNoWindow = true,
                          WindowStyle = ProcessWindowStyle.Hidden,
                          RedirectStandardInput = false,
                          RedirectStandardOutput = true,
                          RedirectStandardError = true,
                          UseShellExecute = false,
                      };

            Process p = Process.Start(psi);

            p.WaitForExit(5000);

            // Clean up CLSID
            RegistryKey key = Registry.ClassesRoot;
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

            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            if (output.ToLower().IndexOf("error") != -1 || error.ToLower().IndexOf("error") != -1)
                MessageBox.Show(String.Format("==STD_OUT=============\r\n{0}\r\n==STD_ERR=============\r\n{1}",
                                              output,
                                              error));

            AskForKillExplorer();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void AskForKillExplorer()
        {
            if (DialogResult.No ==
                MessageBox.Show(
                                "You can start to use LE only after restarting explorer.exe.\r\n" +
                                "\r\n" +
                                "After that, you will see a new item named \"Locale Emulator\" in \r\n" +
                                "the context menu of most file types.\r\n" +
                                "\r\n" +
                                "Do you want me to help you restarting explorer.exe?\r\n" +
                                "If your answer is no, you may need to reboot your computer manually.",
                                "LE Context Menu Installer",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question))
                return;

            try
            {
                foreach (Process p in Process.GetProcessesByName("explorer"))
                {
                    p.Kill();
                    p.WaitForExit(5000);
                }
            }
            catch
            {
            }

            Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe",
                          string.Format("/select,{0}", Assembly.GetExecutingAssembly().Location));
        }

        private string ExtractRegAsm()
        {
            try
            {
                string tempFile = Path.GetTempFileName();

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
            return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                     IsWow64Process(GetCurrentProcess(), out flag)) && flag);
        }

        private static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
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
                MessageBox.Show("Sorry, Locale Emulator is only for Windows 7, 8/8.1 and above.",
                                "OS Outdated",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                Environment.Exit(0);
            }

            Text += @" - Version " + Application.ProductVersion;

            CheckUpdate();
        }

        private void CheckUpdate()
        {
            string url = string.Format(@"http://service.watashi.me/le/check.php?ver={0}&lang={1}",
                                       Application.ProductVersion,
                                       CultureInfo.CurrentUICulture.LCID);

            try
            {
                var webFileUri = new Uri(url);
                WebRequest webRequest = WebRequest.Create(webFileUri);
                webRequest.Timeout = 5 * 1000;

                webRequest.BeginGetResponse(RequestFinished, webRequest);
            }
            catch (Exception ex)
            {
                SetLabelVersion("Error occurs when checking new version: " + ex.Message, false);
            }
        }

        private void RequestFinished(IAsyncResult ar)
        {
            try
            {
                WebResponse response = (ar.AsyncState as WebRequest).EndGetResponse(ar);

                xmlContent.Load(response.GetResponseStream());

                ProcessUpdate(xmlContent);
            }
            catch (Exception ex)
            {
                SetLabelVersion("Error occurs when checking new version: " + ex.Message, false);
            }
        }

        private void ProcessUpdate(XmlDocument xmlContent)
        {
            string newVer = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;

            if (CompareVersion(Application.ProductVersion, newVer))
            {
                hasNewVersion = true;

                SetLabelVersion(string.Format("New version {0} available. Click here for more info.", newVer),
                                true);
            }
            else
            {
                SetLabelVersion("You are using the latest version.", false);
            }
        }

        /// <summary>
        ///     If ver2 is bigger than ver1, return true.
        /// </summary>
        /// <param name="oldVer"></param>
        /// <param name="newVer"></param>
        /// <returns></returns>
        private bool CompareVersion(string oldVer, string newVer)
        {
            var versionOld = new Version(oldVer);
            var versionNew = new Version(newVer);

            return versionOld < versionNew;
        }

        private void SetLabelVersion(string text, bool success)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (labelVersion.InvokeRequired)
            {
                var d = new SetTextCallback(SetLabelVersion);
                Invoke(d, new object[] {text, success});
            }
            else
            {
                labelVersion.Text = text;
                if (success)
                {
                    labelVersion.ForeColor = Color.Blue;
                    labelVersion.Font = new Font(labelVersion.Font, FontStyle.Underline);
                    labelVersion.Cursor = Cursors.Hand;
                }
            }
        }

        private void labelVersion_Click(object sender, EventArgs e)
        {
            if (!hasNewVersion)
                return;

            try
            {
                string version = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;
                string data = xmlContent.SelectSingleNode(@"/VersionInfo/Date/text()").Value;
                string url = xmlContent.SelectSingleNode(@"/VersionInfo/Url/text()").Value;
                string note = xmlContent.SelectSingleNode(@"/VersionInfo/Note/text()").Value;

                if (DialogResult.No == MessageBox.Show(String.Format("Current version: {0}\r\n" +
                                                                     "New version: {1}\r\n" +
                                                                     "Released on: {2}\r\n" +
                                                                     "Release note: {3}\r\n" +
                                                                     "\r\n" +
                                                                     "Do you want to go to the download page now?",
                                                                     Application.ProductVersion,
                                                                     version,
                                                                     data,
                                                                     note),
                                                       "New version available",
                                                       MessageBoxButtons.YesNo,
                                                       MessageBoxIcon.Information))
                {
                    return;
                }

                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private delegate void SetTextCallback(string text, bool success);
    }
}