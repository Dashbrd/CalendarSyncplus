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
//  *      Created On:     02-02-2015 11:15 AM
//  *      Modified On:    04-02-2015 12:40 PM
//  *      FileName:       SettingsViewModel.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Services;
using CalendarSyncPlus.Application.Services.ExchangeWeb;
using CalendarSyncPlus.Application.Services.Google;
using CalendarSyncPlus.Application.Services.Outlook;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class SettingsViewModel : ViewModel<ISettingsView>
    {
        #region Constructors

        [ImportingConstructor]
        public SettingsViewModel(ISettingsView view,
            IGoogleCalendarService googleCalendarService,
            Settings settings,
            ISettingsSerializationService serializationService, IOutlookCalendarService outlookCalendarService,
            IMessageService messageService, IExchangeWebCalendarService exchangeWebCalendarService,
            ApplicationLogger applicationLogger, IWindowsStartupService windowsStartupService)
            : base(view)
        {
            Settings = settings;
            ExchangeWebCalendarService = exchangeWebCalendarService;
            ApplicationLogger = applicationLogger;
            WindowsStartupService = windowsStartupService;
            GoogleCalendarService = googleCalendarService;
            SettingsSerializationService = serializationService;
            OutlookCalendarService = outlookCalendarService;
            MessageService = messageService;
        }

        #endregion

        #region Fields

        private bool _checkForUpdates = true;
        private bool _createNewFileForEverySync;
        private DelegateCommand _createProfileCommand;
        private DelegateCommand _deleteProfileCommand;
        private bool _hideSystemTrayTooltip;
        private bool _isLoading;
        private bool _isloaded;
        private bool _logSyncInfo;
        private bool _minimizeToSystemTray = true;
        private DelegateCommand _moveDownCommand;
        private DelegateCommand _moveUpCommand;
        private bool _rememberPeriodicSyncOn = true;
        private bool _runApplicationAtSystemStartup = true;
        private DelegateCommand _saveCommand;
        private ProfileViewModel _selectedProfile;
        private Settings _settings;
        private bool _settingsSaved;
        private ObservableCollection<ProfileViewModel> _syncProfileList;

        #endregion

        #region Properties

        public IGoogleCalendarService GoogleCalendarService { get; set; }
        public ISettingsSerializationService SettingsSerializationService { get; set; }
        public IOutlookCalendarService OutlookCalendarService { get; set; }
        public IMessageService MessageService { get; set; }
        public IExchangeWebCalendarService ExchangeWebCalendarService { get; private set; }
        public ApplicationLogger ApplicationLogger { get; private set; }
        public IWindowsStartupService WindowsStartupService { get; set; }


        public DelegateCommand CreateProfileCommand
        {
            get { return _createProfileCommand ?? (_createProfileCommand = new DelegateCommand(CreateProfile)); }
        }

        public DelegateCommand DeleteProfileCommand
        {
            get { return _deleteProfileCommand ?? (_deleteProfileCommand = new DelegateCommand(DeleteProfile)); }
        }

        public DelegateCommand MoveUpCommand
        {
            get { return _moveUpCommand ?? (_moveUpCommand = new DelegateCommand(MoveProfileUp)); }
        }

        public DelegateCommand MoveDownCommand
        {
            get { return _moveDownCommand ?? (_moveDownCommand = new DelegateCommand(MoveProfileDown)); }
        }

        public DelegateCommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new DelegateCommand(SaveSettings)); }
        }

        public bool LogSyncInfo
        {
            get { return _logSyncInfo; }
            set { SetProperty(ref _logSyncInfo, value); }
        }

        public bool CreateNewFileForEverySync
        {
            get { return _createNewFileForEverySync; }
            set { SetProperty(ref _createNewFileForEverySync, value); }
        }

        public Settings Settings
        {
            get { return _settings; }
            set { SetProperty(ref _settings, value); }
        }

        public ObservableCollection<ProfileViewModel> SyncProfileList
        {
            get { return _syncProfileList; }
            set { SetProperty(ref _syncProfileList, value); }
        }

        public ProfileViewModel SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                SetProperty(ref _selectedProfile, value);
                if (_selectedProfile != null)
                {
                    _selectedProfile.LoadSyncProfile();
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }


        public bool CheckForUpdates
        {
            get { return _checkForUpdates; }
            set { SetProperty(ref _checkForUpdates, value); }
        }

        public bool RunApplicationAtSystemStartup
        {
            get { return _runApplicationAtSystemStartup; }
            set { SetProperty(ref _runApplicationAtSystemStartup, value); }
        }

        public bool RememberPeriodicSyncOn
        {
            get { return _rememberPeriodicSyncOn; }
            set { SetProperty(ref _rememberPeriodicSyncOn, value); }
        }

        public bool SettingsSaved
        {
            get { return _settingsSaved; }
            set { SetProperty(ref _settingsSaved, value); }
        }

        public bool MinimizeToSystemTray
        {
            get { return _minimizeToSystemTray; }
            set { SetProperty(ref _minimizeToSystemTray, value); }
        }

        public bool HideSystemTrayTooltip
        {
            get { return _hideSystemTrayTooltip; }
            set { SetProperty(ref _hideSystemTrayTooltip, value); }
        }


        private async void CreateProfile()
        {
            string result = await MessageService.ShowInput("Please enter profile name.");

            if (!string.IsNullOrEmpty(result))
            {
                if (Settings.SyncProfiles.Any(t => t.Name.Equals(result)))
                {
                    MessageService.ShowMessageAsync(
                        string.Format("A Profile with name '{0}' already exists. Please try again.", result));
                    return;
                }

                if (Settings.SyncProfiles.Count > 4)
                {
                    MessageService.ShowMessageAsync("You have reached the maximum number of profiles.");
                    return;
                }
                CalendarSyncProfile syncProfile = CalendarSyncProfile.GetDefaultSyncProfile();
                syncProfile.Name = result;
                syncProfile.IsDefault = false;
                var viewModel = new ProfileViewModel(syncProfile, GoogleCalendarService, OutlookCalendarService,
                    MessageService,
                    ExchangeWebCalendarService, ApplicationLogger);
                SyncProfileList.Add(viewModel);
                PropertyChangedEventManager.AddHandler(viewModel, ProfilePropertyChangedHandler, "IsLoading");
            }
        }

        private async void DeleteProfile(object parameter)
        {
            var profile = parameter as ProfileViewModel;
            if (profile != null)
            {
                MessageDialogResult task =
                    await MessageService.ShowConfirmMessage("Are you sure you want to delete the profile?");
                if (task == MessageDialogResult.Affirmative)
                {
                    SyncProfileList.Remove(profile);
                    PropertyChangedEventManager.RemoveHandler(profile, ProfilePropertyChangedHandler, "IsLoading");
                    SelectedProfile = SyncProfileList.FirstOrDefault();
                }
            }
        }

        private void MoveProfileUp(object parameter)
        {
            var profile = parameter as ProfileViewModel;
            if (profile != null)
            {
                int index = SyncProfileList.IndexOf(profile);
                if (index > 0)
                {
                    SyncProfileList.Move(index, index - 1);
                }
            }
        }

        private void MoveProfileDown(object parameter)
        {
            var profile = parameter as ProfileViewModel;
            if (profile != null)
            {
                int index = SyncProfileList.IndexOf(profile);
                if (index < SyncProfileList.Count - 1)
                {
                    SyncProfileList.Move(index, index + 1);
                }
            }
        }

        #endregion

        #region Private Methods

        private async void SaveSettings()
        {
            IsLoading = true;
            SettingsSaved = false;
            Settings.AppSettings.IsFirstSave = false;
            Settings.AppSettings.MinimizeToSystemTray = MinimizeToSystemTray;
            Settings.AppSettings.HideSystemTrayTooltip = HideSystemTrayTooltip;
            Settings.AppSettings.CheckForUpdates = CheckForUpdates;
            Settings.AppSettings.RunApplicationAtSystemStartup = RunApplicationAtSystemStartup;
            Settings.AppSettings.RememberPeriodicSyncOn = RememberPeriodicSyncOn;
            Settings.SyncProfiles.Clear();
            foreach (ProfileViewModel profileViewModel in SyncProfileList)
            {
                Settings.SyncProfiles.Add(profileViewModel.SaveCurrentSyncProfile());
            }

            if (RunApplicationAtSystemStartup)
            {
                WindowsStartupService.RunAtWindowsStartup();
            }
            else
            {
                WindowsStartupService.RemoveFromWindowsStartup();
            }

            try
            {
                bool result = await SettingsSerializationService.SerializeSettingsAsync(Settings);
                await
                    MessageService.ShowMessage(result ? "Settings Saved Successfully" : "Error Saving Settings",
                        "Settings");
                SettingsSaved = true;
            }
            catch (AggregateException exception)
            {
                SettingsSaved = false;
                AggregateException flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
            }
            catch (Exception exception)
            {
                SettingsSaved = false;
                MessageService.ShowMessageAsync(exception.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        public void Load()
        {
            if (_isloaded)
            {
                return;
            }
            LoadSettingsAndGetCalenders();
            _isloaded = true;
        }

        private void LoadSettingsAndGetCalenders()
        {
            IsLoading = true;
            try
            {
                if (Settings != null)
                {
                    MinimizeToSystemTray = Settings.AppSettings.MinimizeToSystemTray;
                    HideSystemTrayTooltip = Settings.AppSettings.HideSystemTrayTooltip;
                    CheckForUpdates = Settings.AppSettings.CheckForUpdates;
                    RunApplicationAtSystemStartup = Settings.AppSettings.RunApplicationAtSystemStartup;
                    RememberPeriodicSyncOn = Settings.AppSettings.RememberPeriodicSyncOn;
                    var profileList = new ObservableCollection<ProfileViewModel>();
                    foreach (CalendarSyncProfile syncProfile in Settings.SyncProfiles)
                    {
                        var viewModel = new ProfileViewModel(syncProfile, GoogleCalendarService, OutlookCalendarService,
                            MessageService,
                            ExchangeWebCalendarService, ApplicationLogger);
                        profileList.Add(viewModel);
                        PropertyChangedEventManager.AddHandler(viewModel, ProfilePropertyChangedHandler, "IsLoading");
                    }
                    SyncProfileList = profileList;
                    SelectedProfile = SyncProfileList.FirstOrDefault();
                }
                else
                {
                    LogSyncInfo = true;
                    CreateNewFileForEverySync = false;
                    MinimizeToSystemTray = true;
                    HideSystemTrayTooltip = false;
                    CheckForUpdates = true;
                }
            }
            catch (AggregateException exception)
            {
                AggregateException flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync(exception.Message);
            }
            IsLoading = false;
        }

        private void ProfilePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsLoading":
                    var viewModel = sender as ProfileViewModel;
                    if (viewModel != null)
                    {
                        IsLoading = viewModel.IsLoading;
                    }
                    break;
            }
        }

        public void Shutdown()
        {
            if (SyncProfileList != null)
            {
                foreach (ProfileViewModel profileViewModel in SyncProfileList)
                {
                    PropertyChangedEventManager.RemoveHandler(profileViewModel, ProfilePropertyChangedHandler,
                        "IsLoading");
                }
            }
        }
    }
}