using System;
using System.Collections.Generic;
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
            XDocument dict = LoadDictionary();
            try
            {
                IEnumerable<XElement> strings = from i in dict.Descendants("Strings").Elements() select i;

                string str = (from s in strings where s.Name == key select s.Value).FirstOrDefault();

                if (String.IsNullOrEmpty(str))
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
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                path = Path.Combine(path, @"Lang\" + CurrentCultureInfo.Name + ".xml");

                dictionary =
                    XDocument.Load(path);
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