using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using LECommonLibrary;

namespace LEProc
{
    internal static class Program
    {
        internal static string[] Args;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            SystemHelper.DisableDPIScale();

            try
            {
                Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "LEUpdater.exe"),
                    "schedule");
            }
            catch
            {
            }

            if (!GlobalHelper.CheckCoreDLLs())
            {
                MessageBox.Show(
                    "Some of the core Dlls are missing.\r\n" +
                    "Please whitelist these Dlls in your antivirus software, then download and re-install LE.\r\n"
                    +
                    "\r\n" +
                    "These Dlls are:\r\n" +
                    "LoaderDll.dll\r\n" +
                    "LocaleEmulator.dll",
                    "Locale Emulator Version " + GlobalHelper.GetLEVersion(),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }
            
            //If global config does not exist, create a new one.
            LEConfig.CheckGlobalConfigFile(true);

            if (args.Length == 0)
            {
                MessageBox.Show(
                    "Welcome to Locale Emulator command line tool.\r\n" +
                    "\r\n" +
                    "Usage: LEProc.exe\r\n" +
                    "\tpath\r\n" +
                    "\t-run path [args]\r\n" +
                    "\t-runas guid path [args]\r\n" +
                    "\t-manage path\r\n" +
                    "\t-global\r\n" +
                    "\r\n" +
                    "path\tFull path of the target application.\r\n" +
                    "guid\tGuid of the target profile (in LEConfig.xml).\r\n" +
                    "args\tAdditional arguments will be passed to the application.\r\n" +
                    "\r\n" +
                    "path\tRun an application with \r\n" +
                    "\t\t(i) its own profile (if any) or \r\n" +
                    "\t\t(ii) first global profile (if any) or \r\n" +
                    "\t\t(iii) default ja-JP profile.\r\n" +
                    "-run\tRun an application with it's own profile.\r\n" +
                    "-runas\tRun an application with a global profile of specific Guid.\r\n" +
                    "-manage\tModify the profile of one application.\r\n" +
                    "-global\tOpen Global Profile Manager.\r\n" +
                    "\r\n" +
                    "\r\n" +
                    "Have a suggestion? Want to report a bug? You're welcome! \r\n" +
                    "Go to https://github.com/xupefei/Locale-Emulator/issues.\r\n" +
                    "\r\n" +
                    "\r\n" +
                    "You can press CTRL+C to copy this message to your clipboard.\r\n",
                    "Locale Emulator Version " + GlobalHelper.GetLEVersion()
                );

                GlobalHelper.ShowErrorDebugMessageBox("SYSTEM_REPORT", 0);

                return;
            }

            try
            {
                Args = args;

                switch (Args[0])
                {
                    case "-run": //-run %APP%
                        RunWithIndependentProfile(Args[1]);
                        break;

                    case "-manage": //-manage %APP%
                        Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                "LEGUI.exe"),
                            $"\"{Args[1]}.le.config\"");
                        break;

                    case "-global": //-global
                        Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            "LEGUI.exe"));
                        break;

                    case "-runas": //-runas %GUID% %APP%
                        RunWithGlobalProfile(Args[1], Args[2]);
                        break;

                    default:
                        if (File.Exists(Args[0]))
                        {
                            RunWithDefaultProfile(Args[0]);
                        }
                        break;
                }
            }
            catch
            {
            }
        }

        private static void RunWithDefaultProfile(string path)
        {
            path = SystemHelper.EnsureAbsolutePath(path);

            var conf = path + ".le.config";

            var appProfile = LEConfig.GetProfiles(conf);
            var globalProfiles = LEConfig.GetProfiles();

            // app profile > first global profile > new profile(ja-JP)
            var profile = appProfile.Any()
                ? appProfile.First()
                : globalProfiles.Any() ? globalProfiles.First() : new LEProfile(true);

            DoRunWithLEProfile(path, 1, profile);
        }

        private static void RunWithGlobalProfile(string guid, string path)
        {
            path = SystemHelper.EnsureAbsolutePath(path);

            // We do not check whether the config exists because only when it exists can this method be called.

            var profile = LEConfig.GetProfiles().First(p => p.Guid == guid);

            DoRunWithLEProfile(path, 3, profile);
        }

        private static void RunWithIndependentProfile(string path)
        {
            path = SystemHelper.EnsureAbsolutePath(path);

            var conf = path + ".le.config";

            if (!File.Exists(conf))
            {
                Process.Start(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LEGUI.exe"),
                    $"\"{path}.le.config\"");
            }
            else
            {
                var profile = LEConfig.GetProfiles(conf)[0];
                DoRunWithLEProfile(path, 2, profile);
            }
        }

        private static void DoRunWithLEProfile(string absPath, int argumentsStart, LEProfile profile)
        {
            try
            {
                if (profile.RunAsAdmin && !SystemHelper.IsAdministrator())
                {
                    ElevateProcess();

                    return;
                }

                if (profile.RunWithSuspend)
                {
                    if (DialogResult.No ==
                        MessageBox.Show(
                            "You are running a exectuable with CREATE_SUSPENDED flag.\r\n" +
                            "\r\n" +
                            "The exectuable will be executed after you click the \"Yes\" button, " +
                            "but as a background process which has no notifications at all." +
                            "You can attach it by using OllyDbg, or stop it with Task Manager.\r\n",
                            "Locale Emulator Debug Mode Warning",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information
                            ))
                        return;
                }

                var applicationName = string.Empty;
                var commandLine = string.Empty;

                if (Path.GetExtension(absPath).ToLower() == ".exe")
                {
                    applicationName = absPath;

                    commandLine = absPath.StartsWith("\"")
                        ? $"{absPath} "
                        : $"\"{absPath}\" ";

                    // use arguments in le.config, prior to command line arguments
                    commandLine += string.IsNullOrEmpty(profile.Parameter) && Args.Skip(argumentsStart).Any()
                        ? Args.Skip(argumentsStart).Aggregate((a, b) => $"{a} {b}")
                        : profile.Parameter;
                }
                else
                {
                    var jb = AssociationReader.GetAssociatedProgram(Path.GetExtension(absPath));

                    if (jb == null)
                        return;

                    applicationName = jb[0];

                    commandLine = jb[0].StartsWith("\"")
                        ? $"{jb[0]} "
                        : $"\"{jb[0]}\" ";
                    commandLine += jb[1].Replace("%1", absPath).Replace("%*", profile.Parameter);
                }

                var currentDirectory = Path.GetDirectoryName(absPath);
                var ansiCodePage = (uint) CultureInfo.GetCultureInfo(profile.Location).TextInfo.ANSICodePage;
                var oemCodePage = (uint) CultureInfo.GetCultureInfo(profile.Location).TextInfo.OEMCodePage;
                var localeID = (uint) CultureInfo.GetCultureInfo(profile.Location).TextInfo.LCID;
                var defaultCharset = (uint)
                    GetCharsetFromANSICodepage(CultureInfo.GetCultureInfo(profile.Location)
                        .TextInfo.ANSICodePage);

                var registries = profile.RedirectRegistry
                    ? RegistryEntriesLoader.GetRegistryEntries(profile.IsAdvancedRedirection)
                    : null;

                var l = new LoaderWrapper
                {
                    ApplicationName = applicationName,
                    CommandLine = commandLine,
                    CurrentDirectory = currentDirectory,
                    AnsiCodePage = ansiCodePage,
                    OemCodePage = oemCodePage,
                    LocaleID = localeID,
                    DefaultCharset = defaultCharset,
                    HookUILanguageAPI = profile.IsAdvancedRedirection ? (uint) 1 : 0,
                    Timezone = profile.Timezone,
                    NumberOfRegistryRedirectionEntries = registries?.Length ?? 0,
                    DebugMode = profile.RunWithSuspend
                };

                registries?.ToList()
                    .ForEach(
                        item =>
                            l.AddRegistryRedirectEntry(item.Root,
                                item.Key,
                                item.Name,
                                item.Type,
                                item.GetValue(CultureInfo.GetCultureInfo(profile.Location))));

                uint ret;
                if ((ret = l.Start()) != 0)
                {
                    if (SystemHelper.IsAdministrator())
                    {
                        GlobalHelper.ShowErrorDebugMessageBox(commandLine, ret);
                    }
                    else
                    {
                        ElevateProcess();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private static void ElevateProcess()
        {
            try
            {
                SystemHelper.RunWithElevatedProcess(
                    Path.Combine(
                        Path.GetDirectoryName(
                            Assembly.GetExecutingAssembly()
                                .Location),
                        "LEProc.exe"),
                    Args);
            }
            catch (Exception)
            {
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

            var charset = ANSI_CHARSET;

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
