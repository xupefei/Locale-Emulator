using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LECommonLibrary
{
    public struct LEProfile
    {
        public string DefaultFont;
        public string Guid;
        public bool ShowInMainMenu;
        public string Location;
        public string Name;
        public string Parameter;
        public bool RunWithSuspend;
        public string Timezone;

        public LEProfile(string name, string guid, bool showInMainMenu, string parameter,
                         string location, string defaultFont, string timezone, bool runWithSuspend)
        {
            Name = name;
            Guid = guid;
            ShowInMainMenu = showInMainMenu;
            Parameter = parameter;
            Location = location;
            DefaultFont = defaultFont;
            Timezone = timezone;
            RunWithSuspend = runWithSuspend;
        }
    }
}
