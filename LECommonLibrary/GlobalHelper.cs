using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LECommonLibrary
{
    public static class GlobalHelper
    {
        public static string GlobalVersionPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                         "LEVersion.xml");

        public static string GetLEVersion()
        {
            try
            {
                XDocument doc = XDocument.Load(GlobalVersionPath);

                return doc.Descendants("LEVersion").First().Attribute("Version").Value;
            }
            catch
            {
                return "0.0.0.0";
            }
        }
    }
}