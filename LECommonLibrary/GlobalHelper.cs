using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LECommonLibrary
{
    public static class GlobalHelper
    {
        public static string GlobalVersionPath =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                         "LEVersion.xml");

        public static string GetLEVersion()
        {
            try
            {
                var doc = XDocument.Load(GlobalVersionPath);

                return doc.Descendants("LEVersion").First().Attribute("Version").Value;
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        public static void ShowErrorDebugMessageBox(string commandLine, uint errorCode)
        {
            MessageBox.Show(String.Format("Error Number: {0}\r\n" +
                                          "Commands: {1}\r\n" +
                                          "\r\n" +
                                          "{2}\r\n" +
                                          "{3}\r\n" +
                                          "If you have any antivirus software running, please turn it off and try again.\r\n"
                                          +
                                          "If you think this error is related to LE itself, feel free to submit an issue at\r\n"
                                          +
                                          "https://github.com/xupefei/Locale-Emulator/issues.\r\n" +
                                          "\r\n" +
                                          "\r\n" +
                                          "You can press CTRL+C to copy this message to your clipboard.\r\n",
                                          Convert.ToString(errorCode, 16).ToUpper(),
                                          commandLine,
                                          String.Format("{0} {1}",
                                                        Environment.OSVersion,
                                                        SystemHelper.Is64BitOS() ? "x64" : "x86"),
                                          GenerateSystemDllVersionList()
                                ),
                            "Locale Emulator Version " + GetLEVersion());
        }

        private static string GenerateSystemDllVersionList()
        {
            string[] dlls = {"NTDLL.DLL", "KERNELBASE.DLL", "KERNEL32.DLL", "USER32.DLL", "GDI32.DLL"};

            var result = new StringBuilder();

            foreach (var dll in dlls)
            {
                result.Append(dll);
                result.Append(": ");
                result.Append(FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, dll)).FileVersion);
                result.Append("\r\n");
            }

            return result.ToString();
        }

        public static bool CheckCoreDLLs()
        {
            string[] dlls = {"LoaderDll.dll", "LocaleEmulator.dll"};

            return
                dlls.Select(dll => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dll))
                    .All(File.Exists);
        }
    }
}