using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace LEGUI
{
    internal class I18n
    {
        internal static readonly CultureInfo CurrentCultureInfo = CultureInfo.CurrentUICulture;
        //internal static readonly CultureInfo CurrentCultureInfo = CultureInfo.GetCultureInfo("fr-FR");

        private static ResourceDictionary cacheDictionary;

        internal static string GetString(string key)
        {
            ResourceDictionary dict = LoadDictionary();
            try
            {
                var s = (string) dict[key];

                if (String.IsNullOrEmpty(s)) return key;

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
                dictionary =
                    (ResourceDictionary) Application.LoadComponent(
                        new Uri(@"Lang\" + CurrentCultureInfo.Name + ".xaml", UriKind.Relative));
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
            ResourceDictionary dict = LoadDictionary();

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}