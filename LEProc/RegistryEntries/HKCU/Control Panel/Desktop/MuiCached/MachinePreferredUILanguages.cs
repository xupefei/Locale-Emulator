using System.Globalization;

namespace LEProc.RegistryEntries.HKCU.Control_Panel.Desktop.MuiCached
{
    class MachinePreferredUILanguages : IRegistryEntry
    {
        public bool IsAdvanced => true;

        public string Root => "HKEY_CURRENT_USER";

        public string Key => @"Control Panel\Desktop\MuiCached";

        public string Name => "MachinePreferredUILanguages";

        public string Type => "REG_MULTI_SZ";

        public string GetValue(CultureInfo culture)
        {
            return $"{culture.TextInfo.CultureName}\x00";
        }
    }
}
