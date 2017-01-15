using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using Amemiya.Net;
using LECommonLibrary;

namespace LEUpdater
{
    internal static class ApplicationUpdater
    {
        private static string url = string.Empty;

        internal static void CheckApplicationUpdate(string version, NotifyIcon notifyIcon)
        {
            var url = @"http://xupefei.github.io/Locale-Emulator/VersionInfo.xml";

            try
            {
                var client = new WebClientEx(10 * 1000);
                var stream = client.DownloadDataStream(url);

                var xmlContent = new XmlDocument();
                xmlContent.Load(stream);

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
            var newVer = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;
            var crtVer = GlobalHelper.GetLEVersion();
            
            GlobalHelper.SetLastUpdate(int.Parse(DateTime.Now.ToString("yyyyMMdd")));
            
            if (CompareVersion(crtVer, newVer))
            {
                try
                {
                    var version = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;
                    var date = xmlContent.SelectSingleNode(@"/VersionInfo/Date/text()").Value;
                    url = xmlContent.SelectSingleNode(@"/VersionInfo/Url/text()").Value;
                    var note = xmlContent.SelectSingleNode(@"/VersionInfo/Note/text()").Value;

                    notifyIcon.BalloonTipClicked += (sender, e) =>
                                                    {
                                                        Process.Start(url);

                                                        notifyIcon.Visible = false;
                                                        Environment.Exit(0);
                                                    };
                    notifyIcon.BalloonTipClosed += (sender, e) =>
                                                   {
                                                       notifyIcon.Visible = false;
                                                       Environment.Exit(0);
                                                   };

                    notifyIcon.ShowBalloonTip(0,
                                              $"New Version {version} Available (Current: {GlobalHelper.GetLEVersion()})",
                                              $"{note}\r\n" + "\r\n" + "Click here to open download page.",
                                              ToolTipIcon.Info);

                    notifyIcon.Text = $"New Version {version} Available.";
                }
                catch (Exception)
                {
                    notifyIcon.Visible = false;
                    Environment.Exit(0);
                }
            }
            else
            {
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        /// <summary>
        ///     If newVer is bigger than oldVer, return true.
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