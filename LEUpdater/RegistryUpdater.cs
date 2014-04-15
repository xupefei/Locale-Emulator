using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace LEUpdater
{
    internal static class RegistryUpdater
    {
        internal static void CheckRegistryUpdate(int version, NotifyIcon notifyIcon)
        {
            string url = string.Format(@"http://service.watashi.me/le/registry.php?ver={0}&lang={1}",
                                       version,
                                       CultureInfo.CurrentUICulture.LCID);

            try
            {
                var webFileUri = new Uri(url);
                WebRequest webRequest = WebRequest.Create(webFileUri);
                webRequest.Timeout = 10 * 1000;

                WebResponse response = webRequest.GetResponse();
                var xmlContent = new XmlDocument();
                xmlContent.Load(response.GetResponseStream());

                ProcessUpdate(xmlContent, notifyIcon);
            }
            catch (Exception)
            {
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        private static void ProcessUpdate(XmlDocument xmlContent, NotifyIcon notifyIcon)
        {
            string registryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                               "LERegistry.xml");

            try
            {
                if (File.Exists(registryPath))
                    File.Delete(registryPath);

                xmlContent.Save(registryPath);
            }
            catch (Exception)
            {
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }
    }
}