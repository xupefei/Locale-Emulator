using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Amemiya.Net;

namespace LEUpdater
{
    internal static class RegistryUpdater
    {
        internal static void CheckRegistryUpdate(int version, NotifyIcon notifyIcon)
        {
            string url = string.Format(@"http://service.watashi.me/le/registry.php?ver={0}&lang={1}",
                                       version,
                                       CultureInfo.CurrentUICulture.LCID);

            string registryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                               "LERegistry.xml");

            try
            {
                var client = new WebClientEx(10 * 1000);
                client.DownloadFile(url, registryPath);
            }
            catch (Exception)
            {
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }
    }
}