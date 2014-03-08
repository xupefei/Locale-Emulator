using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LECommonLibrary
{
    public class LERegistry
    {
        public static string GlobalRegistryPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                         "LERegistry.xml");

        public int Version { get; set; }

        public LERegistryEntry[] GetRegistryEntries()
        {
            return GetRegistryEntries(GlobalRegistryPath);
        }

        public LERegistryEntry[] GetRegistryEntries(string configPath)
        {
            try
            {
                XDocument dict = XDocument.Load(configPath);

                Version =
                    Int32.Parse(dict.Descendants("LERegistry").Elements("Entries").First().Attribute("Version").Value);

                IEnumerable<XElement> pros = from i in dict.Descendants("LERegistry").Elements("Entries").Elements()
                                             select i;

                LERegistryEntry[] profiles =
                    pros.Select(p => new LERegistryEntry(p.Attribute("Root").Value,
                                                         p.Attribute("Key").Value,
                                                         p.Attribute("Name").Value,
                                                         p.Attribute("Type").Value,
                                                         p.Element("Data").Value
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