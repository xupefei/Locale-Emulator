using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using IWshRuntimeLibrary;
using LECommonLibrary;
using File = System.IO.File;

namespace LEGUI
{
    /// <summary>
    ///     Interaction logic for AppConfig.xaml
    /// </summary>
    public partial class AppConfig
    {
        private readonly List<CultureInfo> _cultureInfos = new List<CultureInfo>();
        private readonly List<TimeZoneInfo> _timezones = new List<TimeZoneInfo>();

        public AppConfig()
        {
            InitializeComponent();

            // Region.
            _cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(i => i.DisplayName).ToList();
            cbLocation.ItemsSource = _cultureInfos.Select(c => c.DisplayName);
            cbLocation.SelectedIndex = _cultureInfos.FindIndex(c => c.Name == "ja-JP");

            //Timezone.
            _timezones = TimeZoneInfo.GetSystemTimeZones().ToList();
            cbTimezone.ItemsSource = _timezones.Select(t => t.DisplayName);
            cbTimezone.SelectedIndex = _timezones.FindIndex(tz => tz.Id == "Tokyo Standard Time");

            // Load exists config.
            LEProfile[] configs = LEConfig.GetProfiles(App.StandaloneFilePath);
            if (configs.Length > 0)
            {
                LEProfile conf = configs[0];

                if (!String.IsNullOrEmpty(conf.Parameter))
                {
                    tbAppParameter.FontStyle = FontStyles.Normal;
                    tbAppParameter.Text = conf.Parameter;
                }
                cbTimezone.SelectedIndex = _timezones.FindIndex(tz => tz.Id == conf.Timezone);
                cbLocation.SelectedIndex = _cultureInfos.FindIndex(ci => ci.Name == conf.Location);

                cbStartAsAdmin.IsChecked = conf.RunAsAdmin;
                cbRedirectRegistry.IsChecked = conf.RedirectRegistry;
                cbStartAsSuspend.IsChecked = conf.RunWithSuspend;
            }
        }

        private void SaveSetting()
        {
            var crt = new LEProfile(Path.GetFileName(App.StandaloneFilePath),
                                    Guid.NewGuid().ToString(),
                                    false,
                                    tbAppParameter.Text,
                                    _cultureInfos[cbLocation.SelectedIndex].Name,
                                    _timezones[cbTimezone.SelectedIndex].Id,
                                    cbStartAsAdmin.IsChecked != null && (bool)cbStartAsAdmin.IsChecked,
                                    cbRedirectRegistry.IsChecked != null && (bool)cbRedirectRegistry.IsChecked,
                                    cbStartAsSuspend.IsChecked != null && (bool)cbStartAsSuspend.IsChecked
                );

            LEConfig.SaveApplicationConfigFile(App.StandaloneFilePath, crt);
        }

        private void CreateShortcut(string path)
        {
            var shortcut =
                new WshShell().CreateShortcut(string.Format("{0}\\{1}.lnk",
                                                            Environment.GetFolderPath(Environment.SpecialFolder
                                                                                                 .DesktopDirectory),
                                                            Path.GetFileNameWithoutExtension(path))) as IWshShortcut;

            if (shortcut == null)
            {
                MessageBox.Show("Create shortcut error", "LE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            shortcut.TargetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                               "LEProc.exe");
            shortcut.Arguments = String.Format("-run \"{0}\"", path);
            shortcut.WorkingDirectory = Path.GetDirectoryName(path);
            shortcut.WindowStyle = 1;
            shortcut.Description = string.Format("Run {0} with Locale Emulator", Path.GetFileName(path));
            shortcut.IconLocation = AssociationReader.GetAssociatedIcon(Path.GetExtension(path)).Replace("%1", path);
            shortcut.Save();
        }

        private void RunAndShutdown()
        {
            //Run the application.
            Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LEProc.exe"),
                          string.Format("-run \"{0}\"", App.StandaloneFilePath.Replace(".le.config", "")));

            Application.Current.Shutdown();
        }

        private void bSaveAppSetting_Click(object sender, RoutedEventArgs e)
        {
            SaveSetting();

            RunAndShutdown();
        }

        private void bShortcut_Click(object sender, RoutedEventArgs e)
        {
            SaveSetting();

            CreateShortcut(App.StandaloneFilePath.Replace(".le.config", ""));

            RunAndShutdown();
        }

        private void bDeleteAppSetting_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.No == MessageBox.Show(I18n.GetString("ConfirmDel"), "", MessageBoxButton.YesNo))
                return;

            if (File.Exists(App.StandaloneFilePath))
                File.Delete(App.StandaloneFilePath);

            Application.Current.Shutdown();
        }
    }
}