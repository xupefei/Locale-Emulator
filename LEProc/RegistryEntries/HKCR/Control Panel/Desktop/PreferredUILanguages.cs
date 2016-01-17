using System.Globalization;
using LECommonLibrary;

namespace LEProc.RegistryEntries.HKCR.Control_Panel.Desktop
{
    class PreferredUILanguages : IRegistryEntry
    {
        public bool IsAdvanced => true;

        public string Root => "HKEY_CURRENT_USER";

        public string Key => @"Control Panel\Desktop";

        public string Name => "PreferredUILanguages";

        public string Type => "REG_MULTI_SZ";

        public string GetValue(CultureInfo culture)
        {
            return $"{culture.TextInfo.CultureName}\x00";
        }
    }
}
