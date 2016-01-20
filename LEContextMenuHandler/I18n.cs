using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace LEContextMenuHandler
{
    internal class I18n
    {
        internal static readonly CultureInfo CurrentCultureInfo = CultureInfo.CurrentUICulture;
        //internal static readonly CultureInfo CurrentCultureInfo = CultureInfo.GetCultureInfo("zh-CN");

        private static XDocument cacheDictionary;

        internal static string GetString(string key)
        {
            var dict = LoadDictionary();
            try
            {
                var strings = from i in dict.Descendants("Strings").Elements() select i;

                var str = (from s in strings where s.Name == key select s.Value).FirstOrDefault();

                if (string.IsNullOrEmpty(str))
                    return key;

                return str;
            }
            catch
            {
                return key;
            }
        }

        private static XDocument LoadDictionary()
        {
            if (cacheDictionary != null)
                return cacheDictionary;

            XDocument dictionary = null;
            try
            {
                var langDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Lang\");

                var firstLangPath = Path.Combine(langDir,CurrentCultureInfo.Name + ".xml");
                var fallbackLangPath = Path.Combine(langDir,
                                                    $@"{(CurrentCultureInfo.Name.Contains("-")
                                                             ? CurrentCultureInfo.Name.Split('-')[0]
                                                             : CurrentCultureInfo.Name)}.xml");

                dictionary = XDocument.Load(File.Exists(firstLangPath) ? firstLangPath : fallbackLangPath);
            }
            catch
            {
            }

            //If dictionary is still null, use default language.
            if (dictionary == null)
                dictionary = XDocument.Load(new XmlTextReader(new StringReader(Resource.DefaultLanguage)));

            cacheDictionary = dictionary;

            return cacheDictionary;
        }
    }
}