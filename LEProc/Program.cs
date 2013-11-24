using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using LECommonLibrary;

namespace LEProc
{
    internal static class Program
    {
        public enum ShowCommands
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }

        internal static string[] Args;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox.Show(
                                "Welcome to Locale Emulator command line tool.\r\n" +
                                "\r\n" +
                                "Usage: LEProc.exe\r\n" +
                                "\t[-run <APP_PATH>]\r\n" +
                                "\t[-manage <APP_PATH>]\r\n" +
                                "\t[-global]\r\n" +
                                "\t[-runas <PROFILE_GUID> <APP_PATH>]\r\n" +
                                "\r\n" +
                                "-run: Run an application with it's own profile.\r\n" +
                                "-manage: Modify the profile of one application.\r\n" +
                                "-global: Open Global Profile Manager.\r\n" +
                                "-runas: Run an application with a global profile of specific Guid.\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "Have a suggestion? Want to report a bug? You are welcome! \r\n" +
                                "Go to https://github.com/xupefei/Locale-Emulator/issues,\r\n" +
                                "or send a message to https://google.com/+PaddyXu.\r\n" +
                                "\r\n" +
                                "\r\n" +
                                "You can press CTRL+C to copy this message to your clipboard.",
                                "Locale Emulator Version " + Application.ProductVersion
                    );

                return;
            }

            try
            {
                Args = args;

                switch (args[0])
                {
                    case "-run": //-run %APP%
                        RunWithIndependentProfile(args[1]);
                        break;

                    case "-manage": //-manage %APP%
                        Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                                   "LEGUI.exe"),
                                      String.Format("\"{0}.le.config\"", args[1]));
                        break;

                    case "-global": //-global
                        Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                                   "LEGUI.exe"));
                        break;

                    case "-runas": //-runas %GUID% %APP%
                        RunWithGlobalProfile(args[1], args[2]);
                        break;

                    default:
                        return;
                }
            }
            catch
            {
            }
        }

        private static void RunWithGlobalProfile(string guid, string path)
        {
            // We do not check whether the config exists because only when it exists can this method be called.

            LEProfile profile = LEConfig.GetProfiles().First(p => p.Guid == guid);

            DoRunWithLEProfile(path, profile);
        }

        private static void RunWithIndependentProfile(string path)
        {
            string conf = path + ".le.config";

            if (!File.Exists(conf))
            {
                Process.Start(
                              Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LEGUI.exe"),
                              String.Format("\"{0}.le.config\"", path));
            }
            else
            {
                LEProfile profile = LEConfig.GetProfiles(conf)[0];
                DoRunWithLEProfile(path, profile);
            }
        }

        private static void DoRunWithLEProfile(string path, LEProfile profile)
        {
            try
            {
                if (profile.RunWithSuspend)
                {
                    if (DialogResult.No ==
                        MessageBox.Show(
                                        "You are running a exectuable with CREATE_SUSPEND flag.\r\n" +
                                        "\r\nThe exectuable will be executed after you click the \"Yes\" button,\r\n" +
                                        "But as a background process which has no notfications at all." +
                                        "You can attach it by using OllyDbg, or stop it with Task Manager.\r\n",
                                        "Locale Emulator Debug Mode Warning",
                                        MessageBoxButtons.YesNo
                            ))
                        return;
                }

                string applicationName = string.Empty;
                string commandLine = string.Empty;

                if (Path.GetExtension(path).ToLower() == ".exe")
                {
                    applicationName = path;

                    commandLine = path.StartsWith("\"")
                                      ? string.Format("{0} ", path)
                                      : String.Format("\"{0}\" ", path);

                    commandLine += profile.Parameter;
                }
                else
                {
                    string[] jb = AssociationReader.GetAssociatedProgram(Path.GetExtension(path));

                    if (jb == null)
                        return;

                    applicationName = jb[0];

                    commandLine = jb[0].StartsWith("\"")
                                      ? string.Format("{0} ", jb[0])
                                      : String.Format("\"{0}\" ", jb[0]);
                    commandLine += jb[1].Replace("%1", path).Replace("%*", profile.Parameter);
                }

                string currentDirectory = Path.GetDirectoryName(path);
                bool debugMode = profile.RunWithSuspend;
                var ansiCodePage = (uint)CultureInfo.GetCultureInfo(profile.Location).TextInfo.ANSICodePage;
                var oemCodePage = (uint)CultureInfo.GetCultureInfo(profile.Location).TextInfo.OEMCodePage;
                var localeID = (uint)CultureInfo.GetCultureInfo(profile.Location).TextInfo.LCID;
                var defaultCharset = (uint)
                                     GetCharsetFromANSICodepage(CultureInfo.GetCultureInfo(profile.Location)
                                                                           .TextInfo.ANSICodePage);
                string defaultFaceName = profile.DefaultFont;

                TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(profile.Timezone);
                var timezoneBias = (int)-tzi.BaseUtcOffset.TotalMinutes;

                TimeZoneInfo.AdjustmentRule[] adjustmentRules = tzi.GetAdjustmentRules();
                TimeZoneInfo.AdjustmentRule adjustmentRule = null;
                if (adjustmentRules.Length > 0)
                {
                    // Find the single record that encompasses today's date. If none exists, sets adjustmentRule to null.
                    adjustmentRule =
                        adjustmentRules.SingleOrDefault(ar => ar.DateStart <= DateTime.Now && DateTime.Now <= ar.DateEnd);
                }
                int timezoneDaylightBias = adjustmentRule == null ? 0 : (int)-adjustmentRule.DaylightDelta.TotalMinutes;

                string timezoneStandardName = tzi.StandardName;

                string timezoneDaylightName = tzi.DaylightName;

                var l = new LoaderWrapper
                        {
                            ApplicationName = applicationName,
                            CommandLine = commandLine,
                            CurrentDirectory = currentDirectory,
                            AnsiCodePage = ansiCodePage,
                            OemCodePage = oemCodePage,
                            LocaleID = localeID,
                            DefaultCharset = defaultCharset,
                            DefaultFaceName = defaultFaceName,
                            TimezoneBias = timezoneBias,
                            TimezoneDaylightBias = timezoneDaylightBias,
                            TimezoneStandardName = timezoneStandardName,
                            TimezoneDaylightName = timezoneDaylightName,
                            DebugMode = debugMode,
                        };

                uint ret;
                if ((ret = l.Start()) != 0)
                {
                    if (SystemHelper.IsAdministrator())
                    {
                        MessageBox.Show(String.Format(
                                                      "Launch failed.\r\n" +
                                                      "\r\n" +
                                                      "Commands: {1}\r\n" +
                                                      "Error Number: {2}\r\n" +
                                                      "\r\n" +
                                                      "If you have any antivirus software running, please turn it off and try again.\r\n"
                                                      +
                                                      "If you think this error is related to LE itself, feel free to submit an issue at\r\n"
                                                      +
                                                      "https://github.com/xupefei/Locale-Emulator/issues.\r\n" +
                                                      "\r\n" +
                                                      "\r\n" +
                                                      "You can press CTRL+C to copy this message to your clipboard.",
                                                      applicationName,
                                                      commandLine,
                                                      Convert.ToString(ret, 16).ToUpper()),
                                        "Locale Emulator");
                    }
                    else
                    {
                        try
                        {
                            SystemHelper.RunWithElevatedProcess(
                                                                Path.Combine(
                                                                             Path.GetDirectoryName(
                                                                                                   Assembly
                                                                                                       .GetExecutingAssembly
                                                                                                       ()
                                                                                                       .Location),
                                                                             "LEProc.exe"),
                                                                Args);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private static int GetCharsetFromANSICodepage(int ansicp)
        {
            const int ANSI_CHARSET = 0;
            const int DEFAULT_CHARSET = 1;
            const int SYMBOL_CHARSET = 2;
            const int SHIFTJIS_CHARSET = 128;
            const int HANGEUL_CHARSET = 129;
            const int HANGUL_CHARSET = 129;
            const int GB2312_CHARSET = 134;
            const int CHINESEBIG5_CHARSET = 136;
            const int OEM_CHARSET = 255;
            const int JOHAB_CHARSET = 130;
            const int HEBREW_CHARSET = 177;
            const int ARABIC_CHARSET = 178;
            const int GREEK_CHARSET = 161;
            const int TURKISH_CHARSET = 162;
            const int VIETNAMESE_CHARSET = 163;
            const int THAI_CHARSET = 222;
            const int EASTEUROPE_CHARSET = 238;
            const int RUSSIAN_CHARSET = 204;
            const int MAC_CHARSET = 77;
            const int BALTIC_CHARSET = 186;

            int charset = ANSI_CHARSET;

            switch (ansicp)
            {
                case 932: // Japanese
                    charset = SHIFTJIS_CHARSET;
                    break;
                case 936: // Simplified Chinese
                    charset = GB2312_CHARSET;
                    break;
                case 949: // Korean
                    charset = HANGEUL_CHARSET;
                    break;
                case 950: // Traditional Chinese
                    charset = CHINESEBIG5_CHARSET;
                    break;
                case 1250: // Eastern Europe
                    charset = EASTEUROPE_CHARSET;
                    break;
                case 1251: // Russian
                    charset = RUSSIAN_CHARSET;
                    break;
                case 1252: // Western European Languages
                    charset = ANSI_CHARSET;
                    break;
                case 1253: // Greek
                    charset = GREEK_CHARSET;
                    break;
                case 1254: // Turkish
                    charset = TURKISH_CHARSET;
                    break;
                case 1255: // Hebrew
                    charset = HEBREW_CHARSET;
                    break;
                case 1256: // Arabic
                    charset = ARABIC_CHARSET;
                    break;
                case 1257: // Baltic
                    charset = BALTIC_CHARSET;
                    break;
            }

            return charset;
        }
    }
}