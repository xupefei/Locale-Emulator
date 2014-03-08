namespace LECommonLibrary
{
    public struct LEProfile
    {
        public string DefaultFont;
        public string Guid;
        public string Location;
        public string Name;
        public string Parameter;
        public bool RedirectRegistry;
        public bool RunAsAdmin;
        public bool RunWithSuspend;
        public bool ShowInMainMenu;
        public string Timezone;

        public LEProfile(string name,
                         string guid,
                         bool showInMainMenu,
                         string parameter,
                         string location,
                         string defaultFont,
                         string timezone,
                         bool runAsAdmin,
                         bool redirectRegistry,
                         bool runWithSuspend)
        {
            Name = name;
            Guid = guid;
            ShowInMainMenu = showInMainMenu;
            Parameter = parameter;
            Location = location;
            DefaultFont = defaultFont;
            Timezone = timezone;
            RunAsAdmin = runAsAdmin;
            RedirectRegistry = redirectRegistry;
            RunWithSuspend = runWithSuspend;
        }
    }
}