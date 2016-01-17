using System.Globalization;
using LECommonLibrary;

namespace LEProc.RegistryEntries.HKLM.System.CurrentControlSet.Control.Nls.Codepage
{
    class OEMCP : IRegistryEntry
    {
        public bool IsAdvanced => false;

        public string Root => "HKEY_LOCAL_MACHINE";

        public string Key => @"System\CurrentControlSet\Control\Nls\CodePage";

        public string Name => "OEMCP";

        public string Type => "REG_SZ";

        public string GetValue(CultureInfo culture)
        {
            return culture.TextInfo.OEMCodePage.ToString();
        }
    }
}
