using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using LECommonLibrary;

namespace LEGUI
{
    /// <summary>
    ///     Interaction logic for GlobalConfig.xaml
    /// </summary>
    public partial class GlobalConfig
    {
        private readonly List<CultureInfo> _cultureInfos = new List<CultureInfo>();
        private readonly List<LEProfile> _profiles = new List<LEProfile>();
        private readonly List<TimeZoneInfo> _timezones = new List<TimeZoneInfo>();

        public GlobalConfig()
        {
            InitializeComponent();

            // Region.
            _cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(i => i.DisplayName).ToList();
            cbLocation.ItemsSource = _cultureInfos.Select(c => c.DisplayName);

            //Timezone.
            _timezones = TimeZoneInfo.GetSystemTimeZones().ToList();
            cbTimezone.ItemsSource = _timezones.Select(t => t.DisplayName);

            //Profiles.
            _profiles = LEConfig.GetProfiles().ToList();
            cbGlobalProfiles.ItemsSource = _profiles.Select(p => p.Name);
        }

        private void bSaveGlobalSetting_Click(object sender, RoutedEventArgs e)
        {
            if (cbGlobalProfiles.Items.Count == 0)
                return;

            LEProfile crt = _profiles[cbGlobalProfiles.SelectedIndex];
            crt.DefaultFont = cbDefaultFont.Text;
            crt.Location = _cultureInfos[cbLocation.SelectedIndex].Name;
            crt.Timezone = _timezones[cbTimezone.SelectedIndex].Id;
            crt.ShowInMainMenu = cbShowInMainMenu.IsChecked != null && (bool)cbShowInMainMenu.IsChecked;

            crt.RunAsAdmin = cbStartAsAdmin.IsChecked != null && (bool)cbStartAsAdmin.IsChecked;
            crt.RedirectRegistry = cbRedirectRegistry.IsChecked != null && (bool)cbRedirectRegistry.IsChecked;
            crt.RunWithSuspend = cbStartAsSuspend.IsChecked != null && (bool)cbStartAsSuspend.IsChecked;

            _profiles[cbGlobalProfiles.SelectedIndex] = crt;

            LEConfig.SaveGlobalConfigFile(_profiles.ToArray());
        }

        private void cbGlobalProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bDeleteGlobalSetting.IsEnabled = cbGlobalProfiles.Items.Count != 0;
            bSaveGlobalSetting.IsEnabled = cbGlobalProfiles.Items.Count != 0;

            if (cbGlobalProfiles.SelectedIndex == -1)
            {
                return;
            }

            LEProfile crt = _profiles[cbGlobalProfiles.SelectedIndex];

            cbTimezone.SelectedIndex = _timezones.FindIndex(tz => tz.Id == crt.Timezone);
            cbDefaultFont.Text = crt.DefaultFont;
            cbLocation.SelectedIndex = _cultureInfos.FindIndex(ci => ci.Name == crt.Location);

            cbShowInMainMenu.IsChecked = crt.ShowInMainMenu;
            cbStartAsAdmin.IsChecked = crt.RunAsAdmin;
            cbRedirectRegistry.IsChecked = crt.RedirectRegistry;
            cbStartAsSuspend.IsChecked = crt.RunWithSuspend;
        }

        private void bSaveGlobalSettingAs_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Effect = new BlurEffect();

            var ib = new InputBox
                     {
                         Owner = this,
                         WindowStartupLocation = WindowStartupLocation.CenterOwner,
                         Instruction = I18n.GetString("SaveAsInstruction"),
                         OkText = I18n.GetString("Save"),
                         CancelText = I18n.GetString("Cancel"),
                     };

            if (ib.ShowDialog() == true && !String.IsNullOrEmpty(ib.Text))
            {
                SaveProfileAs(ib.Text);

                cbGlobalProfiles.SelectedIndex = _profiles.Count - 1;
            }

            mainGrid.Effect = null;
        }

        private void SaveProfileAs(string name)
        {
            var pro = new LEProfile(name,
                                    Guid.NewGuid().ToString(),
                                    cbShowInMainMenu.IsChecked != null && (bool)cbShowInMainMenu.IsChecked,
                                    String.Empty,
                                    _cultureInfos[cbLocation.SelectedIndex].Name,
                                    cbDefaultFont.Text,
                                    _timezones[cbTimezone.SelectedIndex].Id,
                                    cbStartAsAdmin.IsChecked != null && (bool)cbStartAsAdmin.IsChecked,
                                    cbRedirectRegistry.IsChecked != null && (bool)cbRedirectRegistry.IsChecked,
                                    cbStartAsSuspend.IsChecked != null && (bool)cbStartAsSuspend.IsChecked);

            _profiles.Add(pro);

            LEConfig.SaveGlobalConfigFile(_profiles.ToArray());

            // Update cbGlobalProfiles.
            cbGlobalProfiles.ItemsSource = _profiles.Select(p => p.Name);
        }

        private void bDeleteGlobalSetting_Click(object sender, RoutedEventArgs e)
        {
            if (cbGlobalProfiles.SelectedIndex == -1)
                return;

            if (MessageBoxResult.No == MessageBox.Show(I18n.GetString("ConfirmDel"), "", MessageBoxButton.YesNo))
                return;

            _profiles.RemoveAt(cbGlobalProfiles.SelectedIndex);

            LEConfig.SaveGlobalConfigFile(_profiles.ToArray());

            // Update cbGlobalProfiles.
            cbGlobalProfiles.ItemsSource = _profiles.Select(p => p.Name);
            cbGlobalProfiles.SelectedIndex = 0;
        }
    }
}