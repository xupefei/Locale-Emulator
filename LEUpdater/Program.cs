using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using LECommonLibrary;
using LEUpdater.Properties;

namespace LEUpdater
{
    internal static class Program
    {
        private static readonly NotifyIcon _notifyIcon = new NotifyIcon {Icon = Resources.icon};
        private static bool byForce;

        private static void Main(string[] args)
        {
            if (args.Length == 0)
                byForce = true;

            var reg = new LERegistry();
            reg.GetRegistryEntries();

            // Check new version every week.
            if (!byForce && Int32.Parse(DateTime.Now.ToString("yyyyMMdd")) - reg.Version < 7)
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
                    _notifyIcon.Visible = false;
                    Environment.Exit(0);
                }
            }

            CheckUpdates(GlobalHelper.GetLEVersion(), reg.Version);
        }

        private static void CheckUpdates(string appVer, int regVer)
        {
            _notifyIcon.Visible = true;

            _notifyIcon.DoubleClick += (sender, e) =>
                                       {
                                           _notifyIcon.Visible = false;
                                           Environment.Exit(0);
                                       };

            RegistryUpdater.CheckRegistryUpdate(regVer, _notifyIcon);
            ApplicationUpdater.CheckApplicationUpdate(appVer, _notifyIcon);

            Application.Run();
        }
    }
}