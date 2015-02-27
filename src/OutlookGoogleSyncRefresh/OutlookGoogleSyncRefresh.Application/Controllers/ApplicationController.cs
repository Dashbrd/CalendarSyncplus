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
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.ViewModels;

namespace OutlookGoogleSyncRefresh.Application.Controllers
{
    [Export(typeof (IApplicationController))]
    public class ApplicationController : Controller, IApplicationController
    {
        private readonly AboutViewModel _aboutViewModel;
        private readonly IGuiInteractionService _guiInteractionService;
        private readonly HelpViewModel _helpViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly IShellController _shellController;
        private readonly ShellService _shellService;
        private readonly ShellViewModel _shellViewModel;
        private readonly SystemTrayNotifierViewModel _systemTrayNotifierViewModel;
        private readonly DelegateCommand exitCommand;
        private bool _isApplicationExiting;

        [ImportingConstructor]
        public ApplicationController(Lazy<ShellViewModel> shellViewModelLazy,
            Lazy<SettingsViewModel> settingsViewModelLazy,
            Lazy<AboutViewModel> aboutViewModelLazy, Lazy<HelpViewModel> helpViewModelLazy,
            Lazy<ShellService> shellServiceLazy, CompositionContainer compositionContainer,
            Lazy<IAccountAuthenticationService> accountAuthenticationServiceLazy, IShellController shellController,
            Lazy<SystemTrayNotifierViewModel> lazySystemTrayNotifierViewModel,
            IGuiInteractionService guiInteractionService)
        {
            //ViewModels
            _shellViewModel = shellViewModelLazy.Value;
            _settingsViewModel = settingsViewModelLazy.Value;
            _aboutViewModel = aboutViewModelLazy.Value;
            _helpViewModel = helpViewModelLazy.Value;
            _systemTrayNotifierViewModel = lazySystemTrayNotifierViewModel.Value;

            //Commands
            _shellViewModel.Closing += ShellViewModelClosing;
            exitCommand = new DelegateCommand(Close);

            //Services
            AccountAuthenticationService = accountAuthenticationServiceLazy.Value;

            _shellService = shellServiceLazy.Value;
            _shellService.ShellView = _shellViewModel.View;
            _shellService.SettingsView = _settingsViewModel.View;
            _shellService.AboutView = _aboutViewModel.View;
            _shellService.HelpView = _helpViewModel.View;
            _shellController = shellController;
            _guiInteractionService = guiInteractionService;
        }

        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        #region IApplicationController Members

        public void Initialize()
        {
            _shellViewModel.ExitCommand = exitCommand;
            _systemTrayNotifierViewModel.ExitCommand = exitCommand;
            //Initialize Other Controllers if Any
            _shellController.Initialize();

            AddWeakEventListener(_settingsViewModel, SettingsChangedHandler);
            AddWeakEventListener(_shellViewModel, ShellViewUpdatedHandler);
        }

        public void Run()
        {
            //Perform Other assignments if required
            _shellViewModel.Show();
        }

        public void Shutdown()
        {
            //Close All controllers if required
            _shellController.Shutdown();

            //Save Settings if any
        }

        #endregion

        private void ShellViewUpdatedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsAboutVisible":
                    if (_shellViewModel.IsAboutVisible)
                    {
                        if (_shellViewModel.Settings.CheckForUpdates)
                        {
                            _aboutViewModel.CheckForUpdatesCommand.Execute(null);
                        }
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
                        if (_settingsViewModel.Settings.SyncFrequency != null)
                        {
                            _shellViewModel.NextSyncTime = _shellViewModel.Settings.SyncFrequency.GetNextSyncTime();
                        }
                    }
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
            if (!_isApplicationExiting && _shellViewModel.Settings.MinimizeToSystemTray)
            {
                _guiInteractionService.HideApplication();
                e.Cancel = true;
            }
        }
    }
}