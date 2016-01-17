namespace LECommonLibrary
{
    public struct LEProfile
    {
        public string Guid;
        public bool IsAdvancedRedirection;
        public string Location;
        public string Name;
        public string Parameter;
        public bool RedirectRegistry;
        public bool RunAsAdmin;
        public bool RunWithSuspend;
        public bool ShowInMainMenu;
        public string Timezone;

        /// <summary>
        ///     Create a new LEProfile object using default(ja-JP) settings.
        /// </summary>
        /// <param name="isDefault">A placeholder, not used at all.</param>
        public LEProfile(bool isDefault)
            : this(
                "ja-JP",
                System.Guid.NewGuid().ToString(),
                false,
                string.Empty,
                "ja-JP",
                "Tokyo Standard Time",
                false,
                true,
                false,
                false)
        {
        }

        /// <summary>
        ///     Create a new LEProfile using arguments.
        /// </summary>
        public LEProfile(string name,
                         string guid,
                         bool showInMainMenu,
                         string parameter,
                         string location,
                         string timezone,
                         bool runAsAdmin,
                         bool redirectRegistry,
                         bool isAdvancedRedirection,
                         bool runWithSuspend)
        {
            Name = name;
            Guid = guid;
            ShowInMainMenu = showInMainMenu;
            Parameter = parameter;
            Location = location;
            Timezone = timezone;
            RunAsAdmin = runAsAdmin;
            RedirectRegistry = redirectRegistry;
            IsAdvancedRedirection = isAdvancedRedirection;
            RunWithSuspend = runWithSuspend;
        }
    }
}