using System.Globalization;
using LECommonLibrary;

namespace LEProc.RegistryEntries.HKLM.System.CurrentControlSet.Control.Nls.Language
{
    class Default : IRegistryEntry
    {
        public bool IsAdvanced => false;

        public string Root => "HKEY_LOCAL_MACHINE";

        public string Key => @"System\CurrentControlSet\Control\Nls\CodePage";

        public string Name => "Default";

        public string Type => "REG_SZ";

        public string GetValue(CultureInfo culture)
        {
            return culture.TextInfo.LCID.ToString();
        }
    }
}
