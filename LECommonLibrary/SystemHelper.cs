using System;

namespace LECommonLibrary
{
    public class SystemHelper
    {
        public static bool Is64BitOS()
        {
            if (IntPtr.Size == 4)
                return false;
            if (IntPtr.Size == 8)
                return true;

            throw new Exception("OS_ARCH_NOT_DEFINED");
        }

        public static string RedirectToWow64(string path)
        {
            string system = Environment.ExpandEnvironmentVariables("%SystemRoot%\\System32\\").ToLower();
            string systemWow64 = Environment.ExpandEnvironmentVariables("%SystemRoot%\\SysWOW64\\").ToLower();

            path = Environment.ExpandEnvironmentVariables(path).ToLower();

            return path.Replace(system, systemWow64);
        }
    }
}