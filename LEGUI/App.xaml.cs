using System;
using System.Windows;
using LECommonLibrary;

namespace LEGUI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static string StandaloneFilePath = string.Empty;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 0)
                StandaloneFilePath = e.Args[0];

            I18n.LoadLanguage();

            LEConfig.CheckGlobalConfigFile(true);

            Current.StartupUri = String.IsNullOrEmpty(StandaloneFilePath)
                                     ? new Uri("GlobalConfig.xaml", UriKind.RelativeOrAbsolute)
                                     : new Uri("AppConfig.xaml", UriKind.RelativeOrAbsolute);
        }
    }
}