using System.Globalization;

namespace LEProc
{
    public class RegistryEntry
    {

        public delegate string ValueReceiver(CultureInfo culture);

        /// <summary>
        /// e.g. HKEY_LOCAL_MACHINE
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// e.g. System\CurrentControlSet\Control\Nls\CodePage
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// e.g. ACP
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// e.g. REG_SZ
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// e.g. culture => culture.TextInfo.ANSICodePage.ToString()
        /// In the example above, if the culture corresponds to ja-jp, the result is "932".
        /// </summary>
        public ValueReceiver GetValue { get; private set; }

        public RegistryEntry(string root, string key, string name, string type, ValueReceiver getValue)
        {
            Root = root;
            Key = key;
            Name = name;
            Type = type;
            GetValue = getValue;
        }
    }
}