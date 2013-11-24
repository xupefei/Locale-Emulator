using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LECommonLibrary
{
    public class LEConfig
    {
        public static string GlobalConfigPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                         "LEConfig.xml");

        public static LEProfile GetProfile(string name)
        {
            try
            {
                return GetProfiles(GlobalConfigPath).Where(p => p.Name == name).ToArray()[0];
            }
            catch
            {
                return new LEProfile();
            }
        }

        public static LEProfile[] GetProfiles()
        {
            return GetProfiles(GlobalConfigPath);
        }

        public static LEProfile[] GetProfiles(string configPath)
        {
            try
            {
                XDocument dict = XDocument.Load(configPath);

                IEnumerable<XElement> pros = from i in dict.Descendants("LEConfig").Elements("Profiles").Elements()
                                             select i;

                LEProfile[] profiles =
                    pros.Select(p => new LEProfile(p.Attribute("Name").Value,
                                                   p.Attribute("Guid").Value,
                                                   bool.Parse(p.Attribute("MainMenu").Value),
                                                   p.Element("Parameter").Value,
                                                   p.Element("Location").Value,
                                                   p.Element("DefaultFont").Value,
                                                   p.Element("Timezone").Value,
                                                   bool.Parse(p.Element("RunWithSuspend").Value)
                                         )
                        ).ToArray();

                return profiles;
            }
            catch
            {
                return new LEProfile[0];
            }
        }

        public static bool CheckGlobalConfigFile(bool buildNewConfig)
        {
            if (buildNewConfig && !File.Exists(GlobalConfigPath))
                BuildGlobalConfigFile();

            return File.Exists(GlobalConfigPath);
        }

        public static void SaveGlobalConfigFile(params LEProfile[] profiles)
        {
            WriteConfig(GlobalConfigPath, profiles);
        }

        public static void SaveApplicationConfigFile(string path, LEProfile profile)
        {
            WriteConfig(path, profile);
        }

        private static void BuildGlobalConfigFile()
        {
            var defaultProfile = new LEProfile("Run in Japanese",
                                               Guid.NewGuid().ToString(),
                                               false,
                                               String.Empty,
                                               "ja-JP",
                                               "MS Gothic",
                                               "Tokyo Standard Time",
                                               false
                );

            WriteConfig(GlobalConfigPath, defaultProfile);
        }

        private static void WriteConfig(string writeTo, params LEProfile[] profiles)
        {
            var baseNode = new XElement("Profiles");

            foreach (LEProfile pro in profiles)
            {
                baseNode.Add(new XElement("Profile",
                                          new XAttribute("Name", pro.Name),
                                          new XAttribute("Guid", pro.Guid),
                                          new XAttribute("MainMenu", pro.ShowInMainMenu),
                                          new XElement("Parameter", pro.Parameter),
                                          new XElement("Location", pro.Location),
                                          new XElement("DefaultFont", pro.DefaultFont),
                                          new XElement("Timezone", pro.Timezone),
                                          new XElement("RunWithSuspend", pro.RunWithSuspend)
                                 )
                    );
            }

            var tree = new XElement("LEConfig", baseNode);

            try
            {
                tree.Save(writeTo);
            }
            catch
            {
            }
        }
    }
}