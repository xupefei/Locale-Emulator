using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using LECommonLibrary;
using LEInstaller.Properties;
using Microsoft.Win32;

namespace LEInstaller
{
    public partial class Form1 : Form
    {
        private readonly string crtDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            string exe = ExtractRegAsm();

            var psi = new ProcessStartInfo(exe, string.Format("\"{0}\" /codebase",
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
                MessageBox.Show(String.Format(
                    "==STD_OUT=============\r\n{0}\r\n==STD_ERR=============\r\n{1}", output, error));

            AskForKillExplorer();
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            string exe = ExtractRegAsm();

            var psi = new ProcessStartInfo(exe, string.Format("/unregister \"{0}\" /codebase",
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
            key.DeleteSubKeyTree(@"\CLSID\{C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE}\");
            key.Close();

            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();

            if (output.ToLower().IndexOf("error") != -1 || error.ToLower().IndexOf("error") != -1)
                MessageBox.Show(String.Format(
                    "==STD_OUT=============\r\n{0}\r\n==STD_ERR=============\r\n{1}", output, error));

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
                    "Shell extension works (or disappears) only after restarting \"explorer.exe\".\r\nDo you want to do it now?",
                    "LE Context Menu Installer",
                    MessageBoxButtons.YesNo))
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

            Process.Start(Environment.SystemDirectory + "\\..\\explorer.exe", string.Format("/root,{0}", crtDir));
        }

        private string ExtractRegAsm()
        {
            try
            {
                string tempFile = Path.GetTempFileName();

                File.WriteAllBytes(tempFile, SystemHelper.Is64BitOS() ? Resources.RegAsm64 : Resources.RegAsm);

                return tempFile;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }
    }
}