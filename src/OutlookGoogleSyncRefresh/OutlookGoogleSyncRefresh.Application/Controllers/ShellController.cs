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

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Threading;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.ViewModels;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Controllers
{

    [Export(typeof(IShellController))]
    public class ShellController : Controller, IShellController
    {
        private readonly ShellViewModel _shellViewModel;
        private readonly ISyncService _syncService;
        private readonly ISettingsSerializationService _settingsSerializationService;
        private readonly SystemTrayNotifierViewModel _systemTrayNotifierViewModel;

        [ImportingConstructor]
        public ShellController(ShellViewModel shellViewModel, ISyncService syncService,
            ISettingsSerializationService settingsSerializationService, SystemTrayNotifierViewModel systemTrayNotifierViewModel)
        {
            _shellViewModel = shellViewModel;
            _syncService = syncService;
            _settingsSerializationService = settingsSerializationService;

            _systemTrayNotifierViewModel = systemTrayNotifierViewModel;
        }

        public ShellViewModel ShellViewModel
        {
            get { return _shellViewModel; }
        }

        public ISyncService SyncService
        {
            get { return _syncService; }
        }

        public void Initialize()
        {
            //Initialize Services for Notification
            SyncService.Initialize();
            AddWeakEventListener(SyncService, SyncServiceNotificationHandler);
        }

        private void SyncServiceNotificationHandler(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "IsSyncInProgress":
                    SyncInProgressChangedHandler();
                    break;
                case "SyncStatus":
                    SyncStatusChangedHandler();
                    break;
            }
        }

        private void SyncStatusChangedHandler()
        {
            ShellViewModel.ErrorMessageChanged(SyncService.SyncStatus);
        }

        private void SyncInProgressChangedHandler()
        {
            ShellViewModel.IsSyncInProgress = SyncService.IsSyncInProgress;
            if (!ShellViewModel.IsSyncInProgress)
            {
                if (string.IsNullOrEmpty(SyncService.SyncStatus))
                {
                    ShellViewModel.LastSyncTime = DateTime.Now;
                }

                if (ShellViewModel.Settings.SyncFrequency != null)
                {
                    ShellViewModel.NextSyncTime = ShellViewModel.Settings.SyncFrequency.GetNextSyncTime();
                }
            }
            else
            {
                ShowNotification();
            }
        }

        private void ShowNotification()
        {
            if (!ShellViewModel.Settings.HideSystemTrayTooltip)
            {
                try
                {
                    Dispatcher.CurrentDispatcher.Invoke((Action)(() =>
                        _systemTrayNotifierViewModel.ShowBalloon()), DispatcherPriority.Normal);
                }
                catch (Exception)
                {

                }
            }
        }

        public void Run()
        {

        }

        public void Shutdown()
        {
            //ShutDown Services
            SyncService.Shutdown();

            RemoveWeakEventListener(SyncService, SyncServiceNotificationHandler);

            ShellViewModel.Settings.LastSuccessfulSync = ShellViewModel.LastSyncTime.GetValueOrDefault();
            _settingsSerializationService.SerializeSettings(ShellViewModel.Settings);
        }
    }
}