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
        private readonly List<LEMenuItem> menuItems = new List<LEMenuItem>();
        private IntPtr _menuBmpGray = IntPtr.Zero;
        private IntPtr _menuBmpPink = IntPtr.Zero;

        private string _selectedFile;

        public FileContextMenuExt()
        {
            //Load the bitmap for the menu item.
            _menuBmpPink = Resource.Pink.GetHbitmap();
            _menuBmpGray = Resource.Gray.GetHbitmap();

            //Load default items.
            menuItems.Add(new LEMenuItem(I18n.GetString("RunDefault"), null, _menuBmpPink, "-run \"%APP%\""));
            menuItems.Add(new LEMenuItem(I18n.GetString("Submenu"), null, _menuBmpGray, ""));
            menuItems.Add(new LEMenuItem(I18n.GetString("ManageApp"), null, _menuBmpGray, "-manage \"%APP%\""));
            menuItems.Add(new LEMenuItem(I18n.GetString("ManageAll"), null, _menuBmpGray, "-global"));

            //Load global profiles.
            Array.ForEach(LEConfig.GetProfiles(),
                          p =>
                          menuItems.Add(new LEMenuItem(p.Name, p.ShowInMainMenu, _menuBmpGray,
                                                       string.Format("-runas \"{0}\" \"%APP%\"", p.Guid))));

        }

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
            System.Diagnostics.Process.Start(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LEProc.exe"),
                cmd.Replace("%APP%", _selectedFile));
        }

        private int RegisterMenuItem(uint id, uint idCmdFirst, string text, IntPtr bitmap, IntPtr subMenu, uint position,
                                     IntPtr registerTo)
        {
            var sub = new MENUITEMINFO();
            sub.cbSize = (uint) Marshal.SizeOf(sub);

            MIIM m = MIIM.MIIM_STRING | MIIM.MIIM_FTYPE | MIIM.MIIM_ID | MIIM.MIIM_STATE;
            if (bitmap != IntPtr.Zero) m |= MIIM.MIIM_BITMAP;
            if (subMenu != IntPtr.Zero) m |= MIIM.MIIM_SUBMENU;
            sub.fMask = m;

            sub.wID = idCmdFirst + id;
            sub.fType = MFT.MFT_STRING;
            sub.dwTypeData = text;
            sub.hSubMenu = subMenu;
            sub.fState = MFS.MFS_ENABLED;
            sub.hbmpItem = bitmap;

            if (!NativeMethods.InsertMenuItem(registerTo, position, true, ref sub))
                return Marshal.GetHRForLastWin32Error();
            return 0;
        }

        #region Shell Extension Registration

        [ComRegisterFunction]
        public static void Register(Type t)
        {
            try
            {
                ShellExtReg.RegisterShellExtContextMenuHandler(t.GUID, "*", "LocaleEmulator.LEContextMenuHandler Class");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Log the error
            }
        }

        [ComUnregisterFunction]
        public static void Unregister(Type t)
        {
            try
            {
                ShellExtReg.UnregisterShellExtContextMenuHandler(t.GUID, "*");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Log the error
            }
        }

        #endregion

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
                    cfFormat = (short) CLIPFORMAT.CF_HDROP,
                    ptd = IntPtr.Zero,
                    dwAspect = DVASPECT.DVASPECT_CONTENT,
                    lindex = -1,
                    tymed = TYMED.TYMED_HGLOBAL
                };
            STGMEDIUM stm;

            // The pDataObj pointer contains the objects being acted upon. In this 
            // example, we get an HDROP handle for enumerating the selected files 
            // and folders.
            var dataObject = (IDataObject) Marshal.GetObjectForIUnknown(pDataObj);
            dataObject.GetData(ref fe, out stm);

            try
            {
                // Get an HDROP handle.
                IntPtr hDrop = stm.unionmember;
                if (hDrop == IntPtr.Zero)
                {
                    throw new ArgumentException();
                }

                // Determine how many files are involved in this operation.
                uint nFiles = NativeMethods.DragQueryFile(hDrop, UInt32.MaxValue, null, 0);

                // This code sample displays the custom context menu item when only 
                // one file is selected. 
                if (nFiles == 1)
                {
                    // Get the path of the file.
                    var fileName = new StringBuilder(260);
                    if (0 == NativeMethods.DragQueryFile(hDrop, 0, fileName,
                                                         fileName.Capacity))
                    {
                        Marshal.ThrowExceptionForHR(WinError.E_FAIL);
                    }
                    _selectedFile = fileName.ToString();
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
            if (((uint) CMF.CMF_DEFAULTONLY & uFlags) != 0)
            {
                return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, 0);
            }

            // Add a separator.
            var sep = new MENUITEMINFO();
            sep.cbSize = (uint) Marshal.SizeOf(sep);
            sep.fMask = MIIM.MIIM_TYPE;
            sep.fType = MFT.MFT_SEPARATOR;
            if (!NativeMethods.InsertMenuItem(hMenu, 0, true, ref sep))
                return Marshal.GetHRForLastWin32Error();

            // Register item 0: RunDefault
            LEMenuItem item = menuItems[0];
            RegisterMenuItem(0, idCmdFirst, item.Text, item.Bitmap, IntPtr.Zero, 1, hMenu);

            // Register item 1: Submenu
            IntPtr hSubMenu = NativeMethods.CreatePopupMenu();
            item = menuItems[1];
            RegisterMenuItem(1, idCmdFirst, item.Text, item.Bitmap, hSubMenu, 2, hMenu);

            // Register item 2 (Submenu->ManageApp).
            item = menuItems[2];
            RegisterMenuItem(2, idCmdFirst, item.Text, item.Bitmap, IntPtr.Zero, 0, hSubMenu);

            // Register item 3 (Submenu->ManageAll).
            item = menuItems[3];
            RegisterMenuItem(3, idCmdFirst, item.Text, item.Bitmap, IntPtr.Zero, 1, hSubMenu);

            //Register user-defined profiles.
            //We should count down to 4.
            for (int i = menuItems.Count - 1; i > 3; i--)
            {
                item = menuItems[i];
                if (item.ShowInMainMenu == true)
                {
                    RegisterMenuItem((uint)i, idCmdFirst, item.Text, item.Bitmap, IntPtr.Zero, 2, hMenu);
                }
                else
                {
                    RegisterMenuItem((uint) i, idCmdFirst, item.Text, item.Bitmap, IntPtr.Zero, 0, hSubMenu);
                }
            }

            // Add a separator.
            sep = new MENUITEMINFO();
            sep.cbSize = (uint) Marshal.SizeOf(sep);
            sep.fMask = MIIM.MIIM_TYPE;
            sep.fType = MFT.MFT_SEPARATOR;
            NativeMethods.InsertMenuItem(hSubMenu,
                                         (uint) menuItems.FindAll(t => t.ShowInMainMenu != true).Count - 4,
                                         true,
                                         ref sep);

            // Return an HRESULT value with the severity set to SEVERITY_SUCCESS. 
            // Set the code value to the total number of items added.
            return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, 2 + (uint) menuItems.Count);
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
            var ici = (CMINVOKECOMMANDINFO) Marshal.PtrToStructure(
                pici, typeof (CMINVOKECOMMANDINFO));

            LEMenuItem item = menuItems[NativeMethods.LowWord(ici.verb.ToInt32())];

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