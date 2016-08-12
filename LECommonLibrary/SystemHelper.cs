using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace LECommonLibrary
{
    public static class SystemHelper
    {
        public static bool Is64BitOS()
        {
            //The code below is from http://1code.codeplex.com/SourceControl/changeset/view/39074#842775
            //which is under the Microsoft Public License: http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.

            if (IntPtr.Size == 8) // 64-bit programs run only on Win64
            {
                return true;
            }
            // Detect whether the current process is a 32-bit process 
            // running on a 64-bit system.
            bool flag;
            return DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                   IsWow64Process(GetCurrentProcess(), out flag) && flag;
        }

        private static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            var moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return GetProcAddress(moduleHandle, methodName) != IntPtr.Zero;
        }

        public static string RedirectToWow64(string path)
        {
            var system = Environment.ExpandEnvironmentVariables("%SystemRoot%\\System32\\").ToLower();
            var systemWow64 = Environment.ExpandEnvironmentVariables("%SystemRoot%\\SysWOW64\\").ToLower();

            path = Environment.ExpandEnvironmentVariables(path).ToLower();

            return path.Replace(system, systemWow64);
        }

        public static string EnsureAbsolutePath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
        }

        public static bool Is4KDisplay()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();

            // 10 = VERTRES
            // 90 = LOGPIXELSY
            // 117 = DESKTOPVERTRES
            int logicDpi = GetDeviceCaps(desktop, 10);
            int logY = GetDeviceCaps(desktop, 90);
            int realDpi = GetDeviceCaps(desktop, 117);

            g.ReleaseHdc();

            return (realDpi/logicDpi == 2) || (logY/96 == 2);
        }

        public static void DisableDPIScale()
        {
            SetProcessDPIAware();
        }

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        public static bool CheckPermission(string path)
        {
            try
            {
                var tempGuid = Guid.NewGuid().ToString();

                File.Create(Path.Combine(path, tempGuid)).Close();
                File.Delete(Path.Combine(path, tempGuid));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAdministrator()
        {
            var wp = new WindowsPrincipal(WindowsIdentity.GetCurrent());

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        ///     RunWithElevatedProcess
        /// </summary>
        /// <exception cref="Exception">Run error.</exception>
        /// <param name="executable"></param>
        /// <param name="args"></param>
        public static void RunWithElevatedProcess(string executable, string[] args)
        {
            var arg = string.Empty;
            arg = args == null
                      ? string.Empty
                      : args.Aggregate(arg, (current, s) => current + $" \"{s}\"");

            var shExecInfo = new SHELLEXECUTEINFO();

            shExecInfo.cbSize = Marshal.SizeOf(shExecInfo);

            shExecInfo.fMask = 0;
            shExecInfo.hwnd = IntPtr.Zero;
            shExecInfo.lpVerb = "runas";
            shExecInfo.lpFile = executable;
            shExecInfo.lpParameters = arg;
            shExecInfo.lpDirectory = Path.GetDirectoryName(executable);

            ;

            if (ShellExecuteEx(ref shExecInfo) == false)
            {
                throw new Exception("Error when run with elevated LE.\r\n" + $"Executable: {executable}\r\n"
                                    + $"Arguments: {arg}");
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)] public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }
    }
}