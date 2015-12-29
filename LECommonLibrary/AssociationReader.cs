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
            var prog = GetAssociatedProgramFromRegistry(ext);

            if (prog == null)
                return null;

            var jb = SplitExecutableAndArgument(prog);

            return jb;
        }

        public static string GetAssociatedIcon(string ext)
        {
            var prog = GetAssociatedIconFromRegistry(ext);

            if (prog == null)
                return null;

            return prog;
        }

        private static string GetAssociatedIconFromRegistry(string ext)
        {
            var d = (string)Registry.GetValue($"HKEY_CLASSES_ROOT\\{ext}", null, null);

            if (string.IsNullOrEmpty(d))
                return null;

            var prog =
                (string)Registry.GetValue($"HKEY_CLASSES_ROOT\\{d}\\DefaultIcon", null, null);

            return prog;
        }

        private static string[] SplitExecutableAndArgument(string line)
        {
            var ret = line.StartsWith("\"")
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
            var d = (string)Registry.GetValue($"HKEY_CLASSES_ROOT\\{ext}", null, null);

            if (string.IsNullOrEmpty(d))
                return null;

            var prog =
                (string)Registry.GetValue($"HKEY_CLASSES_ROOT\\{d}\\Shell\\Open\\Command", null, null);

            return prog;
        }
    }
}