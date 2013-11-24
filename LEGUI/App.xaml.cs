using System;
using System.IO;
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

            bool isGlobalProfile = String.IsNullOrEmpty(StandaloneFilePath);

            LEConfig.CheckGlobalConfigFile(true);

            // We check StandaloneFilePath before loading UI, because this wil be faster.
            if (
                !SystemHelper.CheckPermission(isGlobalProfile
                                                  ? Path.GetDirectoryName(LEConfig.GlobalConfigPath)
                                                  : Path.GetDirectoryName(StandaloneFilePath)))
            {
                if (SystemHelper.IsAdministrator())
                {
                    // We can do nothing now.
                    if (isGlobalProfile)
                        MessageBox.Show(
                                        string.Format("Home directory is not writable. \r\n" +
                                                      "Please move LE to another location and try again.\r\n" +
                                                      "Home directory: {0}",
                                                      Path.GetDirectoryName(LEConfig.GlobalConfigPath)),
                                        "Locale Emulator",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    else
                        MessageBox.Show(string.Format("The directory is not writable.\r\n" +
                                                      "Please use global profile instead.\r\n" +
                                                      "Current Directory: {0}",
                                                      Path.GetDirectoryName(StandaloneFilePath)),
                                        "Locale Emulator",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                    Current.Shutdown();
                }
                else
                {
                    // If we are not administrator, we can ask for administrator permission.
                    try
                    {
                        SystemHelper.RunWithElevatedProcess(
                                                            Path.Combine(
                                                                         Path.GetDirectoryName(LEConfig.GlobalConfigPath),
                                                                         "LEGUI.exe"),
                                                            e.Args);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Locale Emulator", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        Current.Shutdown();
                    }
                }
            }

            I18n.LoadLanguage();

            Current.StartupUri = isGlobalProfile
                                     ? new Uri("GlobalConfig.xaml", UriKind.RelativeOrAbsolute)
                                     : new Uri("AppConfig.xaml", UriKind.RelativeOrAbsolute);
        }
    }
}