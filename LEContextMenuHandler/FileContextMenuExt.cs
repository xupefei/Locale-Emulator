/********************************** Module Header **********************************\
Module Name:  FileContextMenuExt.cs
Project:      CSShellExtContextMenuHandler
Copyright (c) Microsoft Corporation.

The FileContextMenuExt.cs file defines a context menu handler by implementing the 
IShellExtInit and IContextMenu interfaces.

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***********************************************************************************/

#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using LECommonLibrary;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

#endregion

namespace LEContextMenuHandler
{
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("C52B9871-E5E9-41FD-B84D-C5ACADBEC7AE"), ComVisible(true)]
    public class FileContextMenuExt : IShellExtInit, IContextMenu
    {
        // The name of the selected file.
        private readonly IntPtr _menuBmpBlue = IntPtr.Zero;
        private readonly IntPtr _menuBmpYellow = IntPtr.Zero;
        private readonly List<LEMenuItem> menuItems = new List<LEMenuItem>();
        private IntPtr _menuBmpGray = IntPtr.Zero;
        private IntPtr _menuBmpPink = IntPtr.Zero;
        private string _selectedFile;

        public FileContextMenuExt()
        {
            var is4K = SystemHelper.Is4KDisplay();

            //Load the bitmap for the menu item.
            _menuBmpPink = is4K ? Resource.purple_200.GetHbitmap() : Resource.purple.GetHbitmap();
            _menuBmpGray = is4K ? Resource.gray_200.GetHbitmap() : Resource.gray.GetHbitmap();
            _menuBmpBlue = is4K ? Resource.blue_200.GetHbitmap() : Resource.blue.GetHbitmap();
            _menuBmpYellow = is4K ? Resource.yellow_200.GetHbitmap() : Resource.yellow.GetHbitmap();

            //Load default items.
            menuItems.Add(new LEMenuItem(I18n.GetString("Submenu"), true, null, _menuBmpYellow, ""));
            menuItems.Add(new LEMenuItem(I18n.GetString("RunDefault"), true, null, _menuBmpYellow, "-run \"%APP%\""));
            menuItems.Add(new LEMenuItem(I18n.GetString("ManageApp"), true, null, _menuBmpGray, "-manage \"%APP%\""));
            menuItems.Add(new LEMenuItem(I18n.GetString("ManageAll"), true, null, _menuBmpBlue, "-global"));

            //If global config does not exist, create a new one.
            LEConfig.CheckGlobalConfigFile(true);

            //Load global profiles.
            Array.ForEach(LEConfig.GetProfiles(),
                          p =>
                          menuItems.Add(new LEMenuItem(p.Name,
                                                       true,
                                                       p.ShowInMainMenu,
                                                       _menuBmpPink,
                                                       $"-runas \"{p.Guid}\" \"%APP%\"")));
        }

        #region IShellExtInit Members

        /// <summary>
        ///     Initialize the context menu handler.
        /// </summary>
        /// <param name="pidlFolder">
        ///     A pointer to an ITEMIDLIST structure that uniquely identifies a folder.
        /// </param>
        /// <param name="pDataObj">
        ///     A pointer to an IDataObject interface object that can be used to retrieve
        ///     the objects being acted upon.
        /// </param>
        /// <param name="hKeyProgId">
        ///     The registry key for the file object or folder type.
        /// </param>
        public void Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hKeyProgId)
        {
            if (pDataObj == IntPtr.Zero)
            {
                throw new ArgumentException();
            }

            var fe = new FORMATETC
                     {
                         cfFormat = (short)CLIPFORMAT.CF_HDROP,
                         ptd = IntPtr.Zero,
                         dwAspect = DVASPECT.DVASPECT_CONTENT,
                         lindex = -1,
                         tymed = TYMED.TYMED_HGLOBAL
                     };
            STGMEDIUM stm;

            // The pDataObj pointer contains the objects being acted upon. In this 
            // example, we get an HDROP handle for enumerating the selected files 
            // and folders.
            var dataObject = (IDataObject)Marshal.GetObjectForIUnknown(pDataObj);
            dataObject.GetData(ref fe, out stm);

            try
            {
                // Get an HDROP handle.
                var hDrop = stm.unionmember;
                if (hDrop == IntPtr.Zero)
                {
                    throw new ArgumentException();
                }

                // Determine how many files are involved in this operation.
                var nFiles = NativeMethods.DragQueryFile(hDrop, uint.MaxValue, null, 0);

                // This code sample displays the custom context menu item when only 
                // one file is selected. 
                if (nFiles == 1)
                {
                    // Get the path of the file.
                    var fileName = new StringBuilder(260);
                    if (0 == NativeMethods.DragQueryFile(hDrop,
                                                         0,
                                                         fileName,
                                                         fileName.Capacity))
                    {
                        Marshal.ThrowExceptionForHR(WinError.E_FAIL);
                    }

                    _selectedFile = fileName.ToString();

                    // Check exe binary type
                    var path = string.Empty;
                    var ext = Path.GetExtension(_selectedFile).ToLower();

                    if (ext == ".exe")
                    {
                        path = _selectedFile;
                    }
                    else
                    {
                        path = AssociationReader.HaveAssociatedProgram(ext)
                                   ? AssociationReader.GetAssociatedProgram(ext)[0]
                                   : string.Empty;

                        if (SystemHelper.Is64BitOS())
                        {
                            path = SystemHelper.RedirectToWow64(path);
                        }
                    }
                    // Do not display context menus for 64bit exe.
                    switch (PEFileReader.GetPEType(path))
                    {
                        case PEType.Unknown:
                        case PEType.X64:
                            Marshal.ThrowExceptionForHR(WinError.E_FAIL);
                            break;
                        case PEType.X32:
                            break;
                    }
                }
                else
                {
                    Marshal.ThrowExceptionForHR(WinError.E_FAIL);
                }
            }
            finally
            {
                NativeMethods.ReleaseStgMedium(ref stm);
            }
        }

        #endregion

        ~FileContextMenuExt()
        {
            if (_menuBmpPink != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(_menuBmpPink);
                _menuBmpPink = IntPtr.Zero;
            }
            if (_menuBmpGray != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(_menuBmpGray);
                _menuBmpGray = IntPtr.Zero;
            }
        }

        private void OnVerbDisplayFileName(string cmd)
        {
            Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LEProc.exe"),
                          cmd.Replace("%APP%", _selectedFile));
        }

        private int RegisterMenuItem(uint id,
                                     uint idCmdFirst,
                                     string text,
                                     bool enabled,
                                     IntPtr bitmap,
                                     IntPtr subMenu,
                                     uint position,
                                     IntPtr registerTo)
        {
            var sub = new MENUITEMINFO();
            sub.cbSize = (uint)Marshal.SizeOf(sub);

            var m = MIIM.MIIM_STRING | MIIM.MIIM_FTYPE | MIIM.MIIM_ID | MIIM.MIIM_STATE;
            if (bitmap != IntPtr.Zero)
                m |= MIIM.MIIM_BITMAP;
            if (subMenu != IntPtr.Zero)
                m |= MIIM.MIIM_SUBMENU;
            sub.fMask = m;

            sub.wID = idCmdFirst + id;
            sub.fType = MFT.MFT_STRING;
            sub.dwTypeData = text;
            sub.hSubMenu = subMenu;
            sub.fState = enabled ? MFS.MFS_ENABLED : MFS.MFS_DISABLED;
            sub.hbmpItem = bitmap;

            if (!NativeMethods.InsertMenuItem(registerTo, position, true, ref sub))
                return Marshal.GetHRForLastWin32Error();
            return 0;
        }

        #region Shell Extension Registration

        [ComRegisterFunction]
        public static void Register(Type t)
        {
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t)
        {
        }

        #endregion

        #region IContextMenu Members

        /// <summary>
        ///     Add commands to a shortcut menu.
        /// </summary>
        /// <param name="hMenu">A handle to the shortcut menu.</param>
        /// <param name="iMenu">
        ///     The zero-based position at which to insert the first new menu item.
        /// </param>
        /// <param name="idCmdFirst">
        ///     The minimum value that the handler can specify for a menu item ID.
        /// </param>
        /// <param name="idCmdLast">
        ///     The maximum value that the handler can specify for a menu item ID.
        /// </param>
        /// <param name="uFlags">
        ///     Optional flags that specify how the shortcut menu can be changed.
        /// </param>
        /// <returns>
        ///     If successful, returns an HRESULT value that has its severity value set
        ///     to SEVERITY_SUCCESS and its code value set to the offset of the largest
        ///     command identifier that was assigned, plus one.
        /// </returns>
        public int QueryContextMenu(
            IntPtr hMenu,
            uint iMenu,
            uint idCmdFirst,
            uint idCmdLast,
            uint uFlags)
        {
            // If uFlags include CMF_DEFAULTONLY then we should not do anything.
            if (((uint)CMF.CMF_DEFAULTONLY & uFlags) != 0)
            {
                return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, 0);
            }

            // Add a separator.
            var sep = new MENUITEMINFO();
            sep.cbSize = (uint)Marshal.SizeOf(sep);
            sep.fMask = MIIM.MIIM_TYPE;
            sep.fType = MFT.MFT_SEPARATOR;
            if (!NativeMethods.InsertMenuItem(hMenu, 0, true, ref sep))
                return Marshal.GetHRForLastWin32Error();

            // Register item 0: Submenu
            var hSubMenu = NativeMethods.CreatePopupMenu();
            var item = menuItems[0];
            RegisterMenuItem(0, idCmdFirst, item.Text, true, item.Bitmap, hSubMenu, 1, hMenu);

            // Register item 1: RunDefault
            item = menuItems[1];
            RegisterMenuItem(1, idCmdFirst, item.Text, true, item.Bitmap, IntPtr.Zero, 0, hSubMenu);

            // Add a separator.
            sep = new MENUITEMINFO();
            sep.cbSize = (uint)Marshal.SizeOf(sep);
            sep.fMask = MIIM.MIIM_TYPE;
            sep.fType = MFT.MFT_SEPARATOR;
            NativeMethods.InsertMenuItem(hSubMenu, 1, true, ref sep);

            // Register item 2 (Submenu->ManageApp).
            item = menuItems[2];
            RegisterMenuItem(2, idCmdFirst, item.Text, true, item.Bitmap, IntPtr.Zero, 2, hSubMenu);

            // Register item 3 (Submenu->ManageAll).
            item = menuItems[3];
            RegisterMenuItem(3, idCmdFirst, item.Text, true, item.Bitmap, IntPtr.Zero, 3, hSubMenu);

            //Register user-defined profiles.
            //We should count down to 4.
            for (var i = menuItems.Count - 1; i > 3; i--)
            {
                item = menuItems[i];
                if (item.ShowInMainMenu == true)
                {
                    RegisterMenuItem((uint)i, idCmdFirst, item.Text, item.Enabled, item.Bitmap, IntPtr.Zero, 1, hMenu);
                }
                else
                {
                    RegisterMenuItem((uint)i, idCmdFirst, item.Text, item.Enabled, item.Bitmap, IntPtr.Zero, 0, hSubMenu);
                }
            }

            // Add a separator.
            sep = new MENUITEMINFO();
            sep.cbSize = (uint)Marshal.SizeOf(sep);
            sep.fMask = MIIM.MIIM_TYPE;
            sep.fType = MFT.MFT_SEPARATOR;
            NativeMethods.InsertMenuItem(hSubMenu,
                                         (uint)menuItems.FindAll(t => t.ShowInMainMenu != true).Count - 4,
                                         true,
                                         ref sep);

            // Return an HRESULT value with the severity set to SEVERITY_SUCCESS. 
            // Set the code value to the total number of items added.
            return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, 3 + (uint)menuItems.Count);
        }

        /// <summary>
        ///     Carry out the command associated with a shortcut menu item.
        /// </summary>
        /// <param name="pici">
        ///     A pointer to a CMINVOKECOMMANDINFO or CMINVOKECOMMANDINFOEX structure
        ///     containing information about the command.
        /// </param>
        public void InvokeCommand(IntPtr pici)
        {
            var ici = (CMINVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typeof (CMINVOKECOMMANDINFO));

            var item = menuItems[NativeMethods.LowWord(ici.verb.ToInt32())];

            OnVerbDisplayFileName(item.Commands);
        }

        /// <summary>
        ///     Get information about a shortcut menu command, including the help string
        ///     and the language-independent, or canonical, name for the command.
        /// </summary>
        /// <param name="idCmd">Menu command identifier offset.</param>
        /// <param name="uFlags">
        ///     Flags specifying the information to return. This parameter can have one
        ///     of the following values: GCS_HELPTEXTA, GCS_HELPTEXTW, GCS_VALIDATEA,
        ///     GCS_VALIDATEW, GCS_VERBA, GCS_VERBW.
        /// </param>
        /// <param name="pReserved">Reserved. Must be IntPtr.Zero</param>
        /// <param name="pszName">
        ///     The address of the buffer to receive the null-terminated string being
        ///     retrieved.
        /// </param>
        /// <param name="cchMax">
        ///     Size of the buffer, in characters, to receive the null-terminated string.
        /// </param>
        public void GetCommandString(
            UIntPtr idCmd,
            uint uFlags,
            IntPtr pReserved,
            StringBuilder pszName,
            uint cchMax)
        {
        }

        #endregion
    }
}