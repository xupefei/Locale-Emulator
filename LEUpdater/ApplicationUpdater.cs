using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace LEUpdater
{
    internal static class ApplicationUpdater
    {
        internal static void CheckApplicationUpdate(string version)
        {
            string url = string.Format(@"http://service.watashi.me/le/check.php?ver={0}&lang={1}",
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
                MessageBox.Show("Error occurs when checking new version: \r\n" + ex.Message,
                                "Locale Emulator Updater",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private static void ProcessUpdate(XmlDocument xmlContent)
        {
            string newVer = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;

            if (CompareVersion(Application.ProductVersion, newVer))
            {
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
        }

        /// <summary>
        ///     If ver2 is bigger than ver1, return true.
        /// </summary>
        /// <param name="oldVer"></param>
        /// <param name="newVer"></param>
        /// <returns></returns>
        private static bool CompareVersion(string oldVer, string newVer)
        {
            var versionOld = new Version(oldVer);
            var versionNew = new Version(newVer);

            return versionOld < versionNew;
        }
    }
}