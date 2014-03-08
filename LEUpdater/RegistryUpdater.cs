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
        internal static void CheckRegistryUpdate(int version)
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

                ProcessUpdate(xmlContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurs when downloading new registry data: \r\n" + ex.Message,
                                "Locale Emulator Updater",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private static void ProcessUpdate(XmlDocument xmlContent)
        {
            string registryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                               "LERegistry.xml");

            try
            {
                if (File.Exists(registryPath))
                {
                    string old = Path.ChangeExtension(registryPath, "old.xml");

                    if (File.Exists(old))
                        File.Delete(old);
                    File.Move(registryPath, Path.ChangeExtension(registryPath, "old.xml"));
                }

                xmlContent.Save(registryPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurs when saving new registry data: \r\n" + ex.Message,
                                "Locale Emulator Updater",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}