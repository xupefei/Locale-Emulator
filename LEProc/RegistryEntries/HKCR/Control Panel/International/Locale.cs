using System.Globalization;
using LECommonLibrary;

namespace LEProc.RegistryEntries.HKCR.Control_Panel.International
{
    class Locale : IRegistryEntry
    {
        public bool IsAdvanced => true;

        public string Root => "HKEY_CURRENT_USER";

        public string Key => @"Control Panel\International";

        public string Name => "Locale";

        public string Type => "REG_SZ";

        public string GetValue(CultureInfo culture)
        {
            return culture.TextInfo.LCID.ToString("X8");
        }
    }
}
