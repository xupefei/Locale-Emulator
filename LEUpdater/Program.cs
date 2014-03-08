using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using LECommonLibrary;

namespace LEUpdater
{
    internal static class Program
    {
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
                    MessageBox.Show("Current directory is read-only, update process aborted.\r\n" +
                                    "Please move LE to a new location and try again.",
                                    "Locale Emulator Updater",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);

                    Environment.Exit(0);
                }
            }

            CheckUpdates(Application.ProductVersion, reg.Version);
        }

        private static void CheckUpdates(string appVer, int regVer)
        {
            RegistryUpdater.CheckRegistryUpdate(regVer);
            ApplicationUpdater.CheckApplicationUpdate(appVer);
        }
    }
}