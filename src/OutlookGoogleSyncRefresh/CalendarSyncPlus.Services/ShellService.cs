#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     02-02-2015 3:26 PM
//  *      Modified On:    04-02-2015 2:02 PM
//  *      FileName:       ShellService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.ComponentModel.Composition;
using System.Waf.Foundation;
using CalendarSyncPlus.Services.Interfaces;

#endregion

namespace CalendarSyncPlus.Services
{
    [Export, Export(typeof (IShellService))]
    public class ShellService : Model, IShellService
    {
        #region Fields

        private object _aboutView;
        private object _helpView;
        private object _settingsView;
        private object _shellView;

        #endregion

        #region IShellService Members

        public object ShellView
        {
            get { return _shellView; }
            set { SetProperty(ref _shellView, value); }
        }

        public object SettingsView
        {
            get { return _settingsView; }
            set { SetProperty(ref _settingsView, value); }
        }

        public object HelpView
        {
            get { return _helpView; }
            set { SetProperty(ref _helpView, value); }
        }

        public object AboutView
        {
            get { return _aboutView; }
            set { SetProperty(ref _aboutView, value); }
        }

        #endregion
    }
}