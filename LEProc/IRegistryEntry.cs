using System.Globalization;

namespace LEProc
{
    public interface IRegistryEntry
    {
        /// <summary>
        /// Use only when in advanced mode?
        /// </summary>
        bool IsAdvanced { get; }

        /// <summary>
        /// e.g. HKEY_LOCAL_MACHINE
        /// </summary>
        string Root { get; }

        /// <summary>
        /// e.g. System\CurrentControlSet\Control\Nls\CodePage
        /// </summary>
        string Key { get; }

        /// <summary>
        /// e.g. ACP
        /// </summary>
        string Name { get; }

        /// <summary>
        /// e.g. REG_SZ
        /// </summary>
        string Type { get; }
        
        /// <summary>
        /// e.g. 932
        /// </summary>
        string GetValue(CultureInfo culture);
    }
}