using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using LECommonLibrary;
using LEUpdater.Properties;

namespace LEUpdater
{
    internal static class Program
    {
        private static readonly NotifyIcon _notifyIcon = new NotifyIcon {Icon = Resources.icon, Visible = true};

        private static void Main(string[] args)
        {
            var reg = new LERegistry();
            reg.GetRegistryEntries();

            // Check new version every week.
            if (Int32.Parse(DateTime.Now.ToString("yyyyMMdd")) - reg.Version < 7)
            {
                return;
            }

            if (!SystemHelper.CheckPermission(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
            {
                if (!SystemHelper.IsAdministrator())
                {
                    SystemHelper.RunWithElevatedProcess(
                                                        Path.Combine(Assembly.GetExecutingAssembly().Location),
                                                        args);

                    Environment.Exit(0);
                }
                else
                {
                    _notifyIcon.ShowBalloonTip(0,
                                               "Locale Emulator V" + Application.ProductVersion,
                                               "Current directory is read-only, update process aborted.\r\n" +
                                               "Please move LE to a new location and try again.",
                                               ToolTipIcon.Info);

                    Thread.Sleep(5000);
                    _notifyIcon.Visible = false;
                    Environment.Exit(0);
                }
            }

            CheckUpdates(Application.ProductVersion, reg.Version);
        }

        private static void CheckUpdates(string appVer, int regVer)
        {
            _notifyIcon.DoubleClick += _notifyIcon_DoubleClick;

            _notifyIcon.ShowBalloonTip(0,
                                       "Locale Emulator V" + Application.ProductVersion,
                                       "Checking for updates ...",
                                       ToolTipIcon.Info);

            RegistryUpdater.CheckRegistryUpdate(regVer, _notifyIcon);
            ApplicationUpdater.CheckApplicationUpdate(appVer, _notifyIcon);

            Application.Run();
        }

        private static void _notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            Environment.Exit(0);
        }
    }
}