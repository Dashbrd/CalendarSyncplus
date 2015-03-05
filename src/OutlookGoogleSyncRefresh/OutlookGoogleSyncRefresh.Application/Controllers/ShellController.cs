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
//  *      Created On:     05-02-2015 1:25 PM
//  *      Modified On:    05-02-2015 1:25 PM
//  *      FileName:       ShellController.cs
//  * 
//  *****************************************************************************/

#endregion

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.ViewModels;
using OutlookGoogleSyncRefresh.Common.Log;

namespace OutlookGoogleSyncRefresh.Application.Controllers
{
    [Export(typeof(IShellController))]
    public class ShellController : IShellController
    {
        private readonly ApplicationLogger _applicationLogger;
        private readonly ISettingsSerializationService _settingsSerializationService;
        private readonly ShellViewModel _shellViewModel;
        private readonly ISyncService _syncService;
        private readonly SystemTrayNotifierViewModel _systemTrayNotifierViewModel;

        [ImportingConstructor]
        public ShellController(ShellViewModel shellViewModel, ISyncService syncService,
            ISettingsSerializationService settingsSerializationService,
            SystemTrayNotifierViewModel systemTrayNotifierViewModel,
            ApplicationLogger applicationLogger)
        {
            _shellViewModel = shellViewModel;
            _syncService = syncService;
            _settingsSerializationService = settingsSerializationService;

            _systemTrayNotifierViewModel = systemTrayNotifierViewModel;
            _applicationLogger = applicationLogger;
        }

        public ShellViewModel ShellViewModel
        {
            get { return _shellViewModel; }
        }

        public ISyncService SyncService
        {
            get { return _syncService; }
        }

        #region IShellController Members

        public void Initialize()
        {
            //Initialize Services for Notification
            PropertyChangedEventManager.AddHandler(SyncService, SyncServiceNotificationHandler, "");
        }


        public void Run()
        {
        }

        public void Shutdown()
        {
            //ShutDown Services
            ShellViewModel.Shutdown();
            SyncService.Shutdown();

            PropertyChangedEventManager.RemoveHandler(SyncService, SyncServiceNotificationHandler, "");

            ShellViewModel.Settings.LastSuccessfulSync = ShellViewModel.LastSyncTime.GetValueOrDefault();
            _settingsSerializationService.SerializeSettings(ShellViewModel.Settings);
            _systemTrayNotifierViewModel.Quit();
        }

        #endregion

        private void SyncServiceNotificationHandler(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "SyncStatus":
                    SyncStatusChangedHandler();
                    break;
            }
        }

        private void SyncStatusChangedHandler()
        {
            ShellViewModel.ErrorMessageChanged(SyncService.SyncStatus);
        }
    }
}