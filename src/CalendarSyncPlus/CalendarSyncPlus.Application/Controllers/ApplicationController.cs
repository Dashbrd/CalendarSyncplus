#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     02-02-2015 2:55 PM
//  *      Modified On:    02-02-2015 2:55 PM
//  *      FileName:       ApplicationController.cs
//  * 
//  *****************************************************************************/

#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Controllers.Interfaces;
using CalendarSyncPlus.Application.ViewModels;
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Services;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.Application.Controllers
{
    [Export(typeof (IApplicationController))]
    public class ApplicationController : IApplicationController
    {
        private readonly AboutViewModel _aboutViewModel;
        private readonly DelegateCommand _exitCommand;
        private readonly IGuiInteractionService _guiInteractionService;
        private readonly HelpViewModel _helpViewModel;
        private readonly ILogController _logController;
        private readonly LogViewModel _logViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly IShellController _shellController;
        private readonly ISettingsController _settingsController;
        private readonly ShellService _shellService;
        private readonly ShellViewModel _shellViewModel;
        private readonly SystemTrayNotifierViewModel _systemTrayNotifierViewModel;
        private bool _isApplicationExiting;

        [ImportingConstructor]
        public ApplicationController(Lazy<ShellViewModel> shellViewModelLazy,
            Lazy<SettingsViewModel> settingsViewModelLazy,
            Lazy<AboutViewModel> aboutViewModelLazy, Lazy<HelpViewModel> helpViewModelLazy,
            Lazy<LogViewModel> logViewModelLazy,
            Lazy<ShellService> shellServiceLazy, CompositionContainer compositionContainer,
            Lazy<IAccountAuthenticationService> accountAuthenticationServiceLazy,
            IShellController shellController,
            ISettingsController settingsController,
            Lazy<SystemTrayNotifierViewModel> lazySystemTrayNotifierViewModel,
            IGuiInteractionService guiInteractionService, ILogController logController)
        {
            //ViewModels
            _shellViewModel = shellViewModelLazy.Value;
            _settingsViewModel = settingsViewModelLazy.Value;
            _aboutViewModel = aboutViewModelLazy.Value;
            _helpViewModel = helpViewModelLazy.Value;
            _logViewModel = logViewModelLazy.Value;
            _systemTrayNotifierViewModel = lazySystemTrayNotifierViewModel.Value;
            //Commands
            _shellViewModel.Closing += ShellViewModelClosing;
            _exitCommand = new DelegateCommand(Close);

            //Services
            AccountAuthenticationService = accountAuthenticationServiceLazy.Value;

            _shellService = shellServiceLazy.Value;
            _shellService.ShellView = _shellViewModel.View;
            _shellService.SettingsView = _settingsViewModel.View;
            _shellService.AboutView = _aboutViewModel.View;
            _shellService.HelpView = _helpViewModel.View;
            _shellService.LogView = _logViewModel.View;
            _shellController = shellController;
            _settingsController = settingsController;
            _guiInteractionService = guiInteractionService;
            _logController = logController;
        }

        public ILocalizationService LocalizationService { get; set; }
        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        private void ShellViewUpdatedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsAboutVisible":
                    if (_shellViewModel.IsAboutVisible)
                    {
                        if (_shellViewModel.Settings.AppSettings.CheckForUpdates)
                        {
                            _aboutViewModel.CheckForUpdatesCommand.Execute(null);
                        }
                    }
                    break;
                case "IsSettingsVisible":
                    if (_shellViewModel.IsSettingsVisible)
                    {
                        _settingsViewModel.Load();
                    }
                    break;
            }
        }

        private void SettingsChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SettingsSaved":
                    if (_settingsViewModel.SettingsSaved)
                    {
                        _shellViewModel.IsSettingsVisible = false;
                        _shellViewModel.Settings = _settingsViewModel.Settings;
                        foreach (var syncProfile in _settingsViewModel.Settings.CalendarSyncProfiles)
                        {
                            if (syncProfile.IsSyncEnabled && syncProfile.SyncFrequency != null)
                            {
                                syncProfile.NextSync =
                                    syncProfile.SyncFrequency.GetNextSyncTime(DateTime.Now);
                            }
                        }
                    }
                    break;
                case "IsLoading":
                    _shellViewModel.IsSettingsLoading = _settingsViewModel.IsLoading;
                    break;
            }
        }

        private void Close()
        {
            _isApplicationExiting = true;
            _shellViewModel.Close();
        }

        private void ShellViewModelClosing(object sender, CancelEventArgs e)
        {
            // Try to  user has already saved settings or pending operation are left.
            if (_isApplicationExiting || !_shellViewModel.Settings.AppSettings.MinimizeToSystemTray)
            {
                return;
            }
            _guiInteractionService.HideApplication();
            e.Cancel = true;
        }

        #region IApplicationController Members

        public void Initialize()
        {
            _shellViewModel.ExitCommand = _exitCommand;
            _systemTrayNotifierViewModel.ExitCommand = _exitCommand;
            //Initialize Other Controllers if Any
            _shellController.Initialize();
            _logController.Initialize();
            _settingsController.Initialize();
            PropertyChangedEventManager.AddHandler(_settingsViewModel, SettingsChangedHandler, "");
            PropertyChangedEventManager.AddHandler(_shellViewModel, ShellViewUpdatedHandler, "");
        }

        public void Run(bool startMinimized)
        {
            _logController.Run(startMinimized);
            _settingsController.Run(startMinimized);
            //Perform Other assignments if required
            _shellViewModel.Show(startMinimized);
            _settingsViewModel.ApplyProxySettings();
        }

        public void Shutdown()
        {
            //Close All controllers if required
            _logController.Shutdown();
            _shellController.Shutdown();
            _settingsController.Shutdown();
            PropertyChangedEventManager.RemoveHandler(_settingsViewModel, SettingsChangedHandler, "");
            PropertyChangedEventManager.RemoveHandler(_shellViewModel, ShellViewUpdatedHandler, "");

            //Save Settings if any
        }

        #endregion
    }
}