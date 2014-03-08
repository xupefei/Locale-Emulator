using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Amemiya.Extensions;

namespace LEProc
{
    internal class LoaderWrapper
    {
        private const uint CREATE_NORMAL = 0x00000000;
        private const uint CREATE_SUSPENDED = 0x00000004;

        private byte[] _defaultFaceName = new byte[64];
        private LEB _leb;
        private byte[] _timezoneDaylightName = new byte[64];
        private byte[] _timezoneStandardName = new byte[64];

        private RegistryRedirector _registry = new RegistryRedirector(0);

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
                       DefaultFaceName = SetBytes(new byte[64], Encoding.Unicode.GetBytes("MS Gothic")),
                       Timezone =
                       {
                           Bias = -540,
                           DaylightBias = 0,
                           StandardName = SetBytes(new byte[64], Encoding.Unicode.GetBytes("@tzres.dll,-632")),
                           DaylightName = SetBytes(new byte[64], Encoding.Unicode.GetBytes("@tzres.dll,-631"))
                       }
                   };
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
        ///     Default fontname. Default value is "MS Gothic".
        /// </summary>
        internal string DefaultFaceName
        {
            get { return Encoding.Unicode.GetString(_defaultFaceName).Replace("\x00", ""); }
            set
            {
                if (value.Length > 32)
                    throw new Exception("String too long.");
                _defaultFaceName = SetBytes(new byte[64], Encoding.Unicode.GetBytes(value));
            }
        }

        /// <summary>
        ///     Bias of a timezone in minutes. Default value is -540(-60 * 9).
        /// </summary>
        internal int TimezoneBias
        {
            get { return _leb.Timezone.Bias; }
            set { _leb.Timezone.Bias = value; }
        }

        /// <summary>
        ///     Zero.
        /// </summary>
        internal int TimezoneDaylightBias
        {
            get { return _leb.Timezone.DaylightBias; }
            set { _leb.Timezone.DaylightBias = value; }
        }

        /// <summary>
        ///     String that represents a Timezone.
        ///     This can be found in HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones.
        /// </summary>
        internal string TimezoneStandardName
        {
            get { return Encoding.Unicode.GetString(_timezoneStandardName).Replace("\x00", ""); }
            set
            {
                if (value.Length > 32)
                    throw new Exception("String too long.");
                _timezoneStandardName = SetBytes(new byte[64], Encoding.Unicode.GetBytes(value));
            }
        }

        /// <summary>
        ///     String that represents a Daylight.
        ///     This can be found in HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones.
        /// </summary>
        internal string TimezoneDaylightName
        {
            get { return Encoding.Unicode.GetString(_timezoneDaylightName).Replace("\x00", ""); }
            set
            {
                if (value.Length > 32)
                    throw new Exception("String too long.");
                _timezoneDaylightName = SetBytes(new byte[64], Encoding.Unicode.GetBytes(value));
            }
        }

        /// <summary>
        /// Get or set the number of registry redirection entries.
        /// </summary>
        internal int NumberOfRegistryRedirectionEntries
        {
            get { return _registry.NumberOfRegistryRedirectionEntries; }
            set { _registry = new RegistryRedirector(value); }
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
            if (String.IsNullOrEmpty(ApplicationName))
                throw new Exception("ApplicationName cannot null.");

            var newLEB = ArrayExtensions.StructToBytes(_leb);
            newLEB = newLEB.CombineWith(_registry.GetBinaryData());

            var locLEB = Marshal.AllocHGlobal(newLEB.Length);
            Marshal.Copy(newLEB, 0, locLEB, newLEB.Length);

            return LeCreateProcess(locLEB,
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
        }

        private static byte[] SetBytes(byte[] bytesInput, IEnumerable<byte> bytesValue)
        {
            int i = 0;
            foreach (byte byteChar in bytesValue)
            {
                bytesInput[i] = byteChar;
                i++;
            }
            return bytesInput;
        }

        [DllImport("LoaderDll.dll", CharSet = CharSet.Unicode)]
        public static extern uint LeCreateProcess(IntPtr leb,
                                                  [MarshalAs(UnmanagedType.LPWStr), In] string applicationName,
                                                  [MarshalAs(UnmanagedType.LPWStr), In] string commandLine,
                                                  [MarshalAs(UnmanagedType.LPWStr), In] string currentDirectory,
                                                  ulong creationFlags,
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
            internal TIME_FIELDS StandardStart;
            internal uint StandardBias;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] internal byte[] DaylightName;
            internal TIME_FIELDS DaylightStart;
            internal int DaylightBias;
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