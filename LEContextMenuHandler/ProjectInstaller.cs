/********************************** Module Header **********************************\
Module Name:  ProjectInstaller.cs
Project:      CSShellExtContextMenuHandler
Copyright (c) Microsoft Corporation.

The installer class defines the custom actions in the setup. We use the custom 
actions to register and unregister the COM-visible classes in the current managed 
assembly.

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***********************************************************************************/

#region Using directives

using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;

#endregion

namespace LEContextMenuHandler
{
    [RunInstaller(true), ComVisible(false)]
    public class ProjectInstaller : Installer
    {
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            // Call RegistrationServices.RegisterAssembly to register the classes in 
            // the current managed assembly to enable creation from COM.
            var regService = new RegistrationServices();
            regService.RegisterAssembly(GetType().Assembly, AssemblyRegistrationFlags.SetCodeBase);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            // Call RegistrationServices.UnregisterAssembly to unregister the classes 
            // in the current managed assembly.
            var regService = new RegistrationServices();
            regService.UnregisterAssembly(GetType().Assembly);
        }
    }
}