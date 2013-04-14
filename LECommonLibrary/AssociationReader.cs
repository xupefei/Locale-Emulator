using System;
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

        private static string[] SplitExecutableAndArgument(string line)
        {
            string[] ret = line.StartsWith("\"")
                               ? line.Split(new[] {"\" "}, 2, StringSplitOptions.None)
                               : line.Split(new[] {' '}, 2, StringSplitOptions.None);

            if (ret.Length == 2)
            {
                ret[0] = ret[0].StartsWith("\"") ? ret[0].Substring(1) : ret[0];
                return ret;
            }
            return null;
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