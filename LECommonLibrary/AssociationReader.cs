using System;
using System.IO;
using Microsoft.Win32;

namespace LECommonLibrary
{
    public class AssociationReader
    {
        public static bool HaveAssociatedProgram(string ext)
        {
            if (GetAssociatedProgram(ext) == null)
                return false;

            return true;
        }

        public static string[] GetAssociatedProgram(string ext)
        {
            string prog = GetAssociatedProgramFromRegistry(ext);

            if (prog == null)
                return null;

            string[] jb = SplitExecutableAndArgument(prog);

            return jb;
        }

        public static string GetAssociatedIcon(string ext)
        {
            string prog = GetAssociatedIconFromRegistry(ext);

            if (prog == null)
                return null;

            return prog;
        }

        private static string GetAssociatedIconFromRegistry(string ext)
        {
            var d = (string)Registry.GetValue(string.Format("HKEY_CLASSES_ROOT\\{0}", ext), null, null);

            if (string.IsNullOrEmpty(d))
                return null;

            var prog =
                (string)Registry.GetValue(String.Format("HKEY_CLASSES_ROOT\\{0}\\DefaultIcon", d), null, null);

            return prog;
        }

        private static string[] SplitExecutableAndArgument(string line)
        {
            if (line.StartsWith("\""))
            {
                string[] ret = line.Split(new[] {"\" "}, 2, StringSplitOptions.None);
                ret[0] = ret[0].Substring(1);

                return File.Exists(ret[0]) ? ret : null;
            }

            // Else
            string exe = line;
            string arg = string.Empty;

            while (true)
            {
                try
                {
                    if (!line.Contains(" "))
                        return File.Exists(exe) ? new[] {exe, arg} : null;

                    arg = exe.Substring(exe.LastIndexOf(' ')) + " " + arg;
                    exe = exe.Substring(0, exe.LastIndexOf(' '));

                    if (File.Exists(exe))
                        return new[] {exe, arg};
                }
                catch
                {
                    return null;
                }
            }
        }

        private static string GetAssociatedProgramFromRegistry(string ext)
        {
            var d = (string)Registry.GetValue(string.Format("HKEY_CLASSES_ROOT\\{0}", ext), null, null);

            if (string.IsNullOrEmpty(d))
                return null;

            var prog =
                (string)Registry.GetValue(String.Format("HKEY_CLASSES_ROOT\\{0}\\Shell\\Open\\Command", d), null, null);

            return prog;
        }
    }
}