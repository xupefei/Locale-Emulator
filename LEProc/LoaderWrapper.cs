using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Amemiya.Extensions;
using Microsoft.Win32;

namespace LEProc
{
    internal class LoaderWrapper
    {
        private const uint CREATE_NORMAL = 0x00000000;
        private const uint CREATE_SUSPENDED = 0x00000004;
        private byte[] _defaultFaceName = new byte[64];
        private LEB _leb;
        private LERegistryRedirector _registry = new LERegistryRedirector(0);

        internal LoaderWrapper()
            : this(null, null, null)
        {
        }

        internal LoaderWrapper(string applicationName)
            : this(applicationName, null, null)
        {
        }

        internal LoaderWrapper(string applicationName, string commandLine, string currentDirectory)
        {
            ApplicationName = applicationName;
            CommandLine = commandLine;
            CurrentDirectory = currentDirectory;

            _leb = new LEB
                   {
                       AnsiCodePage = 932,
                       OemCodePage = 932,
                       LocaleID = 0x411,
                       DefaultCharset = 128,
                       HookUILanguageAPI = 0,
                       // As we have abandoned the "default font" parameter,
                       // we decide to put here some empty bytes.
                       DefaultFaceName = new byte[64]
                   };
            Timezone = "Tokyo Standard Time";
        }

        /// <summary>
        ///     Application that will be run.
        /// </summary>
        internal string ApplicationName { get; set; }

        /// <summary>
        ///     Command arguments.
        /// </summary>
        internal string CommandLine { get; set; }

        /// <summary>
        ///     Working directory.
        /// </summary>
        internal string CurrentDirectory { get; set; }

        /// <summary>
        ///     Whether the process should be created with CREATE_SUSPENDED. Useful when debugging with OllyDbg.
        /// </summary>
        internal bool DebugMode { get; set; }

        /// <summary>
        ///     New AnsiCodePage. Default value is 932.
        /// </summary>
        internal uint AnsiCodePage
        {
            get { return _leb.AnsiCodePage; }
            set { _leb.AnsiCodePage = value; }
        }

        /// <summary>
        ///     New OemCodePage. Default value is 932.
        /// </summary>
        internal uint OemCodePage
        {
            get { return _leb.OemCodePage; }
            set { _leb.OemCodePage = value; }
        }

        /// <summary>
        ///     New LocaleID. Default value is 0x411(1041).
        /// </summary>
        internal uint LocaleID
        {
            get { return _leb.LocaleID; }
            set { _leb.LocaleID = value; }
        }

        /// <summary>
        ///     New DefaultCharset. Default value is 128(Shift-JIS).
        /// </summary>
        internal uint DefaultCharset
        {
            get { return _leb.DefaultCharset; }
            set { _leb.DefaultCharset = value; }
        }

        /// <summary>
        ///     Should we hook UI-related API? Default value is 0.
        /// </summary>
        internal uint HookUILanguageAPI
        {
            get { return _leb.HookUILanguageAPI; }
            set { _leb.HookUILanguageAPI = value; }
        }

        /// <summary>
        ///     String that represents a Timezone.
        ///     This can be found in HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones.
        /// </summary>
        internal string Timezone
        {
            get { return _leb.Timezone.GetStandardName(); }
            set
            {
                if (value.Length > 32)
                    throw new Exception("String too long.");

                if (false == Enumerable.Any(TimeZoneInfo.GetSystemTimeZones(), item => item.Id == value))
                    throw new Exception($"Timezone \"{value}\" not found in your system.");

                var tzi = TimeZoneInfo.FindSystemTimeZoneById(value);
                _leb.Timezone.SetStandardName(tzi.StandardName);
                _leb.Timezone.SetDaylightName(tzi.StandardName);

                var tzi2 = ReadTZIFromRegistry(value);
                _leb.Timezone.Bias = tzi2.Bias;
                _leb.Timezone.StandardBias = tzi2.StandardBias;
                _leb.Timezone.DaylightBias = 0; //tzi2.DaylightBias;

                //SYSTEMTIME is not the same with TIME_FIELDS (from RTL)
                //_leb.Timezone.StandardDate = SYSTEMTIME_To_TIME_FIELDS(tzi2.StandardDate);
                //_leb.Timezone.DaylightDate = SYSTEMTIME_To_TIME_FIELDS(tzi2.DaylightDate);
            }
        }

        /// <summary>
        ///     Get or set the number of registry redirection entries.
        /// </summary>
        internal int NumberOfRegistryRedirectionEntries
        {
            get { return _registry.NumberOfRegistryRedirectionEntries; }
            set { _registry = new LERegistryRedirector(value); }
        }

        internal bool AddRegistryRedirectEntry(
            string root,
            string subkey,
            string valueName,
            string dataType,
            string data)
        {
            return _registry.AddRegistryEntry(root,
                                              subkey,
                                              valueName,
                                              dataType,
                                              data);
        }

        /// <summary>
        ///     Create process.
        /// </summary>
        /// <returns>Error number</returns>
        internal uint Start()
        {
            if (string.IsNullOrEmpty(ApplicationName))
                throw new Exception("ApplicationName cannot null.");

            var newLEB = ArrayExtensions.StructToBytes(_leb);
            newLEB = newLEB.CombineWith(_registry.GetBinaryData());

            var locLEB = Marshal.AllocHGlobal(newLEB.Length);
            Marshal.Copy(newLEB, 0, locLEB, newLEB.Length);

            var ret = LeCreateProcess(locLEB,
                                   ApplicationName,
                                   CommandLine,
                                   CurrentDirectory,
                                   DebugMode ? CREATE_SUSPENDED : CREATE_NORMAL,
                                   IntPtr.Zero,
                                   IntPtr.Zero,
                                   IntPtr.Zero,
                                   IntPtr.Zero,
                                   IntPtr.Zero,
                                   IntPtr.Zero);

            Marshal.FreeHGlobal(locLEB);

            return ret;
        }

        private static byte[] SetBytes(byte[] bytesInput, IEnumerable<byte> bytesValue)
        {
            var i = 0;
            foreach (var byteChar in bytesValue)
            {
                bytesInput[i] = byteChar;
                i++;
            }
            return bytesInput;
        }

        public static T BytesToStruct<T>(byte[] bytes)
        {
            var size = Marshal.SizeOf(typeof (T));
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return (T)Marshal.PtrToStructure(buffer, typeof (T));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private TIME_FIELDS SYSTEMTIME_To_TIME_FIELDS(_SYSTEMTIME st)
        {
            return new TIME_FIELDS
                   {
                       Year = st.wYear,
                       Month = st.wMonth,
                       Day = st.wDay,
                       Hour = st.wHour,
                       Minute = st.wMinute,
                       Second = st.wSecond,
                       Milliseconds = st.wMilliseconds,
                       Weekday = st.wDayOfWeek
                   };
        }

        private _REG_TZI_FORMAT ReadTZIFromRegistry(string id)
        {
            var tzi =
                (byte[])
                Registry.GetValue(
                                  $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Time Zones\\{id}",
                                  "TZI",
                                  null);

            return BytesToStruct<_REG_TZI_FORMAT>(tzi);
        }

        [DllImport("LoaderDll.dll", CharSet = CharSet.Unicode)]
        public static extern uint LeCreateProcess(IntPtr leb,
                                                  [MarshalAs(UnmanagedType.LPWStr), In] string applicationName,
                                                  [MarshalAs(UnmanagedType.LPWStr), In] string commandLine,
                                                  [MarshalAs(UnmanagedType.LPWStr), In] string currentDirectory,
                                                  uint creationFlags,
                                                  IntPtr startupInfo,
                                                  IntPtr processInformation,
                                                  IntPtr processAttributes,
                                                  IntPtr threadAttributes,
                                                  IntPtr environment,
                                                  IntPtr token);

        [StructLayout(LayoutKind.Sequential)]
        internal struct LEB
        {
            internal uint AnsiCodePage;
            internal uint OemCodePage;
            internal uint LocaleID;
            internal uint DefaultCharset;
            internal uint HookUILanguageAPI;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] DefaultFaceName;
            internal RTL_TIME_ZONE_INFORMATION Timezone;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PML_PROCESS_INFORMATION
        {
            internal IntPtr hProcess;
            internal IntPtr hThread;
            internal uint dwProcessId;
            internal uint dwThreadId;
            internal IntPtr FirstCallLdrLoadDll;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RTL_TIME_ZONE_INFORMATION
        {
            internal int Bias;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] StandardName;
            internal TIME_FIELDS StandardDate;
            internal int StandardBias;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] DaylightName;
            internal TIME_FIELDS DaylightDate;
            internal int DaylightBias;

            public override string ToString()
            {
                return
                    $"StandardName={Encoding.Unicode.GetString(StandardName)}";
            }

            public string GetStandardName()
            {
                return Encoding.Unicode.GetString(StandardName).Replace("\x00", "");
            }

            public void SetStandardName(string name)
            {
                StandardName = SetBytes(new byte[64], Encoding.Unicode.GetBytes(name));
            }

            public string GetDaylightName()
            {
                return Encoding.Unicode.GetString(DaylightName).Replace("\x00", "");
            }

            public void SetDaylightName(string name)
            {
                DaylightName = SetBytes(new byte[64], Encoding.Unicode.GetBytes(name));
            }
        }

        private struct _REG_TZI_FORMAT
        {
            internal int Bias;
            internal int StandardBias;
            internal int DaylightBias;
            internal _SYSTEMTIME StandardDate;
            internal _SYSTEMTIME DaylightDate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct _SYSTEMTIME
        {
            internal ushort wYear;
            internal ushort wMonth;
            internal ushort wDayOfWeek;
            internal ushort wDay;
            internal ushort wHour;
            internal ushort wMinute;
            internal ushort wSecond;
            internal ushort wMilliseconds;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TIME_FIELDS
        {
            internal ushort Year; // range [1601...]
            internal ushort Month; // range [1..12]
            internal ushort Day; // range [1..31]
            internal ushort Hour; // range [0..23]
            internal ushort Minute; // range [0..59]
            internal ushort Second; // range [0..59]
            internal ushort Milliseconds; // range [0..999]
            internal ushort Weekday; // range [0..6] == [Sunday..Saturday]
        }
    }
}
