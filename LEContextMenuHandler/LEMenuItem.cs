using System;

namespace LEContextMenuHandler
{
    internal struct LEMenuItem
    {
        internal IntPtr Bitmap;
        internal string Commands;
        internal bool? ShowInMainMenu;
        internal string Text;
        internal bool Enabled;

        internal LEMenuItem(string text, bool enabled, bool? showInMainMenu, IntPtr bitmap, string commands)
        {
            ShowInMainMenu = showInMainMenu;
            Text = text;
            Enabled = enabled;
            Bitmap = bitmap;
            Commands = commands;
        }
    }
}