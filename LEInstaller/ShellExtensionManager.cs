using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LEInstaller
{
    class ShellExtensionManager
    {
        private static string clsid = "{C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE}";
        private static string fileType = "*";
        private static string friendlyName = "LocaleEmulator.LEContextMenuHandler Class";
        private static string keyName = $@"Software\Classes\{fileType}\shellex\ContextMenuHandlers\{clsid}";

        public static void RegisterShellExtContextMenuHandler(bool allUsers)
        {
            var rootName = allUsers ? Registry.LocalMachine : Registry.CurrentUser;

            using (var key = rootName.CreateSubKey(keyName))
            {
                key?.SetValue(null, friendlyName);
            }
        }

        public static void UnregisterShellExtContextMenuHandler(bool allUsers)
        {
            var rootName = allUsers ? Registry.LocalMachine : Registry.CurrentUser;

            rootName.DeleteSubKeyTree(keyName);
        }

        public static bool IsInstalled(bool allUsers)
        {
            var rootName = allUsers ? Registry.LocalMachine : Registry.CurrentUser;

            return rootName.OpenSubKey(keyName, false) != null;
        }
    }
}