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

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => MessageBox.Show(((Exception) args.ExceptionObject).Message);

            base.OnStartup(e);
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 0)
            {
                StandaloneFilePath = SystemHelper.EnsureAbsolutePath(e.Args[0]);

                // This happens when user is trying to drop a exe onto LEGUI.
                if (!StandaloneFilePath.EndsWith(".le.config", true, null))
                    StandaloneFilePath += ".le.config";
            }

            var isGlobalProfile = string.IsNullOrEmpty(StandaloneFilePath);

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
                                        "Home directory is not writable. \r\n"
                                        + "Please move LE to another location and try again.\r\n"
                                        + $"Home directory: {Path.GetDirectoryName(LEConfig.GlobalConfigPath)}",
                                        "Locale Emulator",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    else
                        MessageBox.Show(
                                        "The directory is not writable.\r\n" + "Please use global profile instead.\r\n"
                                        + $"Current Directory: {Path.GetDirectoryName(StandaloneFilePath)}",
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
                        MessageBox.Show("LEGUI requires administrator privilege to write to the current directory.",
                                        "Locale Emulator",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
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