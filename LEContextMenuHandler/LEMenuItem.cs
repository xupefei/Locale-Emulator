using System;

namespace LEContextMenuHandler
{
    internal struct LEMenuItem
    {
        internal IntPtr Bitmap;
        internal string Commands;
        internal bool? ShowInMainMenu;
        internal string Text;

        internal LEMenuItem(string text,bool? showInMainMenu, IntPtr bitmap, string commands)
        {
            ShowInMainMenu = showInMainMenu;
            Text = text;
            Bitmap = bitmap;
            Commands = commands;
        }
    }
}