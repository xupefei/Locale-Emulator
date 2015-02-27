using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace LEGUI
{
    internal class I18n
    {
        internal static readonly CultureInfo CurrentCultureInfo = CultureInfo.CurrentUICulture;
        //internal static readonly CultureInfo CurrentCultureInfo = CultureInfo.GetCultureInfo("fr-FR");

        private static ResourceDictionary cacheDictionary;

        internal static string GetString(string key)
        {
            var dict = LoadDictionary();
            try
            {
                var s = (string)dict[key];

                if (String.IsNullOrEmpty(s))
                    return key;

                return s;
            }
            catch
            {
                return key;
            }
        }

        private static ResourceDictionary LoadDictionary()
        {
            if (cacheDictionary != null)
                return cacheDictionary;

            ResourceDictionary dictionary = null;
            try
            {
                Application.Current.Resources.MergedDictionaries
                           .Insert(0,
                                   XamlReader.Load(
                                                   new FileStream(
                                                       Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                                       + @"\Lang\" + CurrentCultureInfo.Name + ".xaml",
                                                       FileMode.Open)) as ResourceDictionary);
            }
            catch
            {
            }

            //If dictionary is still null, use default language.
            if (dictionary == null)
                if (Application.Current.Resources.MergedDictionaries.Count > 0)
                    dictionary = Application.Current.Resources.MergedDictionaries[0];
                else
                    throw new Exception("No language file.");

            cacheDictionary = dictionary;

            return cacheDictionary;
        }

        internal static void LoadLanguage()
        {
            var dict = LoadDictionary();

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}