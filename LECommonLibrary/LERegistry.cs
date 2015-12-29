using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LECommonLibrary.Properties;

namespace LECommonLibrary
{
    public class LERegistry
    {
        public LERegistryEntry[] GetRegistryEntries()
        {
            try
            {
                var dict = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(Resources.LERegistry)));

                var pros = from i in dict.Descendants("LERegistry").Elements("Entries").Elements()
                           select i;

                var profiles =
                    pros.Select(p => new LERegistryEntry(p.Attribute("Root").Value,
                                                         p.Attribute("Key").Value,
                                                         p.Attribute("Name").Value,
                                                         p.Attribute("Type").Value,
                                                         p.Element("Data").Value.Replace("[0x00]", "\x00")
                                         )).ToArray();

                return profiles;
            }
            catch
            {
                return new LERegistryEntry[0];
            }
        }
    }
}