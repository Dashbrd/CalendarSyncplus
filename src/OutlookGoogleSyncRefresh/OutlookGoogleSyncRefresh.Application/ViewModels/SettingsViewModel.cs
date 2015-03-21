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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Applications;
using MahApps.Metro.Controls.Dialogs;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Services.ExchangeWeb;
using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.Services.Outlook;
using OutlookGoogleSyncRefresh.Application.Views;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Common.MetaData;
using OutlookGoogleSyncRefresh.Domain.Helpers;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.ViewModels
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
            _settings = settings;
            ExchangeWebCalendarService = exchangeWebCalendarService;
            ApplicationLogger = applicationLogger;
            WindowsStartupService = windowsStartupService;
            GoogleCalendarService = googleCalendarService;
            SettingsSerializationService = serializationService;
            OutlookCalendarService = outlookCalendarService;
            MessageService = messageService;
            Initialize();
        }

        private void Initialize()
        {
            CalendarSyncModes = new List<CalendarSyncDirectionEnum>()
                {
                    CalendarSyncDirectionEnum.OutlookGoogleOneWay,
                    CalendarSyncDirectionEnum.OutlookGoogleOneWayToSource,
                    CalendarSyncDirectionEnum.OutlookGoogleTwoWay
                };
            SyncFrequencies = new List<string>
                {
                    "Hourly",
                    "Daily",
                    "Weekly"
                };
            SyncFrequency = "Hourly";
        }

        #endregion

        #region Fields

        private bool _addAttachments;
        private bool _addAttendees;
        private bool _addDescription;
        private bool _addReminders;
        private DelegateCommand _autoDetectExchangeServer;
        private bool _checkForUpdates = true;
        private bool _createNewFileForEverySync;
        private int _daysInFuture = 7;
        private int _daysInPast = 1;
        private string _exchangeServerUrl;
        private DelegateCommand _getGoogleCalendarCommand;
        private DelegateCommand _getOutlookMailboxCommand;
        private DelegateCommand _getOutlookProfileLIstCommand;
        private List<Calendar> _googleCalenders;
        private bool _hideSystemTrayTooltip;
        private bool _isDefaultMailBox = true;
        private bool _isDefaultProfile = true;
        private bool _isExchangeWebServices;
        private bool _isLoading;
        private bool _logSyncInfo;
        private bool _minimizeToSystemTray = true;
        private OutlookCalendar _outlookCalendar;
        private OutlookMailBox _outlookMailBox;
        private List<OutlookMailBox> _outlookMailBoxes;
        private List<string> _outlookProfileList;
        private string _password;
        private bool _rememberPeriodicSyncOn = true;
        private bool _runApplicationAtSystemStartup = true;
        private DelegateCommand _saveCommand;
        private Calendar _selectedCalendar;
        private string _selectedOutlookProfileName;
        private Settings _settings;
        private bool _settingsSaved;
        private List<string> _syncFrequencies;
        private string _syncFrequency;
        private SyncFrequencyViewModel _syncFrequencyViewModel;
        private string _username;
        private List<CalendarSyncDirectionEnum> _calendarSyncModes;
        private CalendarSyncDirectionEnum _selectedCalendarSyncDirection;

        #endregion

        #region Properties

        public IGoogleCalendarService GoogleCalendarService { get; set; }
        public ISettingsSerializationService SettingsSerializationService { get; set; }
        public IOutlookCalendarService OutlookCalendarService { get; set; }
        public IMessageService MessageService { get; set; }
        public IExchangeWebCalendarService ExchangeWebCalendarService { get; private set; }
        public ApplicationLogger ApplicationLogger { get; private set; }
        public IWindowsStartupService WindowsStartupService { get; set; }

        public Calendar SelectedCalendar
        {
            get { return _selectedCalendar; }
            set { SetProperty(ref _selectedCalendar, value); }
        }

        public List<Calendar> GoogleCalenders
        {
            get { return _googleCalenders; }
            set { SetProperty(ref _googleCalenders, value); }
        }

        public int DaysInFuture
        {
            get { return _daysInFuture; }
            set { SetProperty(ref _daysInFuture, value); }
        }

        public int DaysInPast
        {
            get { return _daysInPast; }
            set { SetProperty(ref _daysInPast, value); }
        }

        public List<string> SyncFrequencies
        {
            get { return _syncFrequencies; }
            set { SetProperty(ref _syncFrequencies, value); }
        }

        public string SyncFrequency
        {
            get { return _syncFrequency; }
            set
            {
                SetProperty(ref _syncFrequency, value);
                OnSyncFrequencyChanged();
            }
        }

        public SyncFrequencyViewModel SyncFrequencyViewModel
        {
            get { return _syncFrequencyViewModel; }
            set { SetProperty(ref _syncFrequencyViewModel, value); }
        }

        public DelegateCommand GetGoogleCalendarCommand
        {
            get
            {
                return _getGoogleCalendarCommand ?? (_getGoogleCalendarCommand = new DelegateCommand(GetGoogleCalendar));
            }
        }

        public DelegateCommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new DelegateCommand(SaveSettings)); }
        }

        public DelegateCommand GetOutlookMailBoxesCommand
        {
            get
            {
                return _getOutlookMailboxCommand ??
                       (_getOutlookMailboxCommand = new DelegateCommand(GetOutlookMailBoxes));
            }
        }

        public bool AddDescription
        {
            get { return _addDescription; }
            set { SetProperty(ref _addDescription, value); }
        }

        public bool AddAttendeesToDescription
        {
            get { return _addAttendeesToDescription; }
            set { SetProperty(ref _addAttendeesToDescription, value); }
        }

        public bool AddAttendees
        {
            get { return _addAttendees; }
            set
            {
                SetProperty(ref _addAttendees, value);
                if (_addAttendees == false)
                {
                    AddAttendeesToDescription = false;
                }
            }
        }

        public bool AddReminders
        {
            get { return _addReminders; }
            set { SetProperty(ref _addReminders, value); }
        }

        public bool AddAttachments
        {
            get { return _addAttachments; }
            set { SetProperty(ref _addAttachments, value); }
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public List<OutlookMailBox> OutlookMailBoxes
        {
            get { return _outlookMailBoxes; }
            set { SetProperty(ref _outlookMailBoxes, value); }
        }

        public OutlookCalendar SelectedOutlookCalendar
        {
            get { return _outlookCalendar; }
            set { SetProperty(ref _outlookCalendar, value); }
        }

        public OutlookMailBox SelectedOutlookMailBox
        {
            get { return _outlookMailBox; }
            set { SetProperty(ref _outlookMailBox, value); }
        }

        public bool IsDefaultMailBox
        {
            get { return _isDefaultMailBox; }
            set { SetProperty(ref _isDefaultMailBox, value); }
        }

        public bool IsDefaultProfile
        {
            get { return _isDefaultProfile; }
            set { SetProperty(ref _isDefaultProfile, value); }
        }

        public bool IsExchangeWebServices
        {
            get { return _isExchangeWebServices; }
            set { SetProperty(ref _isExchangeWebServices, value); }
        }

        public List<CalendarSyncDirectionEnum> CalendarSyncModes
        {
            get { return _calendarSyncModes; }
            set { SetProperty(ref _calendarSyncModes, value); }
        }

        public CalendarSyncDirectionEnum SelectedCalendarSyncDirection
        {
            get { return _selectedCalendarSyncDirection; }
            set
            {
                SetProperty(ref _selectedCalendarSyncDirection, value);
                if (_selectedCalendarSyncDirection == CalendarSyncDirectionEnum.OutlookGoogleTwoWay)
                {
                    MasterCalendarServiceType = CalendarServiceType.OutlookDesktop;
                    AllowMasterCalendarSelect = true;
                }
            }
        }

        public bool AllowMasterCalendarSelect
        {
            get { return _allowMasterCalendarSelect; }
            set { SetProperty(ref _allowMasterCalendarSelect, value); }
        }

        public CalendarServiceType MasterCalendarServiceType
        {
            get { return _masterCalendarServiceType; }
            set { SetProperty(ref _masterCalendarServiceType, value); }
        }

        public bool DisableDelete
        {
            get { return _disableDelete; }
            set { SetProperty(ref _disableDelete, value); }
        }

        public bool ConfirmOnDelete
        {
            get { return _confirmOnDelete; }
            set { SetProperty(ref _confirmOnDelete, value); }
        }

        public bool KeepLastModifiedCopy
        {
            get { return _keepLastModifiedCopy; }
            set { SetProperty(ref _keepLastModifiedCopy, value); }
        }

        public DelegateCommand AutoDetectExchangeServer
        {
            get
            {
                return
                    _autoDetectExchangeServer = _autoDetectExchangeServer ?? new DelegateCommand(AutoDetectEWSSettings);
            }
        }

        public DelegateCommand ResetOutlookCalendarCommand
        {
            get
            {
                return _resetOutlookCalendarCommand = _resetOutlookCalendarCommand ??
                    new DelegateCommand(ResetOutlookCalendarHandler);
            }
        }

        public DelegateCommand ResetGoogleCalendarCommand
        {
            get
            {
                return _resetGoogleCalendar = _resetGoogleCalendar ?? new DelegateCommand(ResetGoogleCalendarHandler);
            }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public string ExchangeServerUrl
        {
            get { return _exchangeServerUrl; }
            set { SetProperty(ref _exchangeServerUrl, value); }
        }

        public string SelectedOutlookProfileName
        {
            get { return _selectedOutlookProfileName; }
            set { SetProperty(ref _selectedOutlookProfileName, value); }
        }

        public List<string> OutlookProfileList
        {
            get { return _outlookProfileList; }
            set { SetProperty(ref _outlookProfileList, value); }
        }

        public List<OutlookCalendar> ExchangeCalendarList
        {
            get { return _exchangeCalendarList; }
            set { SetProperty(ref _exchangeCalendarList, value); }
        }

        public OutlookCalendar SelectedExchangeCalendar
        {
            get { return _selectedExchangeCalendar; }
            set { SetProperty(ref _selectedExchangeCalendar, value); }
        }

        public bool CheckForUpdates
        {
            get { return _checkForUpdates; }
            set { SetProperty(ref _checkForUpdates, value); }
        }

        public bool RunApplicationAtSystemStartup
        {
            get { return _runApplicationAtSystemStartup; }
            set
            {
                SetProperty(ref _runApplicationAtSystemStartup, value);
            }
        }

        public bool RememberPeriodicSyncOn
        {
            get { return _rememberPeriodicSyncOn; }
            set { SetProperty(ref _rememberPeriodicSyncOn, value); }
        }

        public DelegateCommand GetOutlookProfileListCommand
        {
            get
            {
                return _getOutlookProfileLIstCommand ??
                       (_getOutlookProfileLIstCommand = new DelegateCommand(GetOutlookProfileList));
            }
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

        private async void GetOutlookMailBoxes()
        {
            IsLoading = true;
            await GetOutlookMailBoxesInternal();
            IsLoading = false;
        }

        private async Task GetOutlookMailBoxesInternal()
        {
            try
            {
                OutlookMailBoxes = await Task<List<OutlookMailBox>>.Factory.StartNew(GetOutlookMailBox);
                if (Settings.OutlookSettings.OutlookMailBox != null)
                {
                    SelectedOutlookMailBox =
                        OutlookMailBoxes.FirstOrDefault(
                            t => t.EntryId.Equals(Settings.OutlookSettings.OutlookMailBox.EntryId));
                    if (Settings.OutlookSettings.OutlookCalendar != null && SelectedOutlookMailBox != null)
                    {
                        SelectedOutlookCalendar =
                            SelectedOutlookMailBox.Calendars.FirstOrDefault(
                                t => t.EntryId.Equals(Settings.OutlookSettings.OutlookCalendar.EntryId));
                    }
                }
            }
            catch (Exception aggregateException)
            {
                string exception = aggregateException.ToString();
                ApplicationLogger.LogError(exception);
            }
        }

        private List<OutlookMailBox> GetOutlookMailBox()
        {
            return OutlookCalendarService.GetAllMailBoxes(SelectedOutlookProfileName ?? string.Empty);
        }

        private async void AutoDetectEWSSettings()
        {
            IsLoading = true;

            IsLoading = false;
        }

        private async void GetOutlookProfileList()
        {
            IsLoading = true;

            await GetOutlookProfileListInternal();

            IsLoading = false;
        }

        private async Task GetOutlookProfileListInternal()
        {
            OutlookProfileList = await OutlookCalendarService.GetOutLookProfieListAsync();
        }

        #endregion

        #region Private Methods

        private void OnSyncFrequencyChanged()
        {
            if (Settings != null && Settings.SyncSettings.SyncFrequency != null &&
                SyncFrequency == Settings.SyncSettings.SyncFrequency.Name)
            {
                switch (SyncFrequency)
                {
                    case "Hourly":
                        SyncFrequencyViewModel
                            = new HourlySyncViewModel(Settings.SyncSettings.SyncFrequency as HourlySyncFrequency);
                        break;
                    case "Daily":
                        SyncFrequencyViewModel
                            = new DailySyncViewModel(Settings.SyncSettings.SyncFrequency as DailySyncFrequency);
                        break;
                    case "Weekly":
                        SyncFrequencyViewModel
                            = new WeeklySyncViewModel(Settings.SyncSettings.SyncFrequency as WeeklySyncFrequency);
                        break;
                }
            }
            else
            {
                switch (SyncFrequency)
                {
                    case "Hourly":
                        SyncFrequencyViewModel = new HourlySyncViewModel();
                        break;
                    case "Daily":
                        SyncFrequencyViewModel = new DailySyncViewModel();
                        break;
                    case "Weekly":
                        SyncFrequencyViewModel = new WeeklySyncViewModel();
                        break;
                }
            }
        }

        private async void LoadSettingsAndGetCalenders()
        {
            IsLoading = true;
            try
            {
                if (Settings != null)
                {
                    if (Settings.SyncSettings.SyncFrequency != null)
                    {
                        SyncFrequency = Settings.SyncSettings.SyncFrequency.Name;
                    }

                    DaysInPast = Settings.DaysInPast;
                    DaysInFuture = Settings.DaysInFuture;
                    AddAttendees = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees);
                    if (AddAttendees)
                    {
                        AddAttendeesToDescription = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription);
                    }
                    AddDescription = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
                    AddReminders = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);
                    AddAttachments = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attachments);
                    LogSyncInfo = Settings.LogSettings.LogSyncInfo;
                    CreateNewFileForEverySync = CreateNewFileForEverySync;
                    IsDefaultProfile = Settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile);
                    IsDefaultMailBox =
                        Settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultCalendar);
                    IsExchangeWebServices =
                        Settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices);
                    SelectedOutlookProfileName = Settings.OutlookSettings.OutlookProfileName;
                    MinimizeToSystemTray = Settings.MinimizeToSystemTray;
                    HideSystemTrayTooltip = Settings.HideSystemTrayTooltip;
                    CheckForUpdates = Settings.CheckForUpdates;
                    RunApplicationAtSystemStartup = Settings.RunApplicationAtSystemStartup;
                    RememberPeriodicSyncOn = Settings.RememberPeriodicSyncOn;
                    SelectedCalendarSyncDirection = Settings.SyncSettings.CalendarSyncDirection;
                    MasterCalendarServiceType = Settings.SyncSettings.MasterCalendar;
                    DisableDelete = Settings.SyncSettings.DisableDelete;
                    ConfirmOnDelete = Settings.SyncSettings.ConfirmOnDelete;
                }
                else
                {
                    DaysInPast = 1;
                    DaysInFuture = 7;
                    LogSyncInfo = true;
                    AddDescription = true;
                    CreateNewFileForEverySync = false;
                    IsDefaultProfile = true;
                    IsDefaultMailBox = true;
                    MinimizeToSystemTray = true;
                    HideSystemTrayTooltip = false;
                    CheckForUpdates = true;
                    SelectedCalendarSyncDirection = CalendarSyncDirectionEnum.OutlookGoogleOneWay;
                }

                if (!IsDefaultProfile)
                {
                    await GetOutlookProfileListInternal();
                }

                if (!IsDefaultMailBox)
                {
                    await GetOutlookMailBoxesInternal();
                }

                if (Settings != null && Settings.GoogleCalendar != null)
                {
                    await GetGoogleCalendarInternal();
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

        private async void SaveSettings()
        {
            IsLoading = true;
            SettingsSaved = false;
            Settings.IsFirstSave = false;
            Settings.GoogleCalendar = SelectedCalendar;
            Settings.DaysInFuture = DaysInFuture;
            Settings.DaysInPast = DaysInPast;
            Settings.SyncSettings.SyncFrequency = SyncFrequencyViewModel.GetFrequency();
            Settings.UpdateEntryOptions(AddDescription, AddReminders, AddAttendees, AddAttendeesToDescription, AddAttachments);
            Settings.LogSettings.LogSyncInfo = LogSyncInfo;
            Settings.LogSettings.CreateNewFileForEverySync = CreateNewFileForEverySync;
            Settings.OutlookSettings.OutlookMailBox = SelectedOutlookMailBox;
            Settings.OutlookSettings.OutlookCalendar = SelectedOutlookCalendar;
            Settings.OutlookSettings.OutlookProfileName = SelectedOutlookProfileName;
            Settings.MinimizeToSystemTray = MinimizeToSystemTray;
            Settings.HideSystemTrayTooltip = HideSystemTrayTooltip;
            Settings.CheckForUpdates = CheckForUpdates;
            Settings.RunApplicationAtSystemStartup = RunApplicationAtSystemStartup;
            Settings.RememberPeriodicSyncOn = RememberPeriodicSyncOn;
            Settings.OutlookSettings.UpdateOutlookOptions(IsDefaultProfile, IsDefaultMailBox, IsExchangeWebServices);
            Settings.ExchangeServerSettings.Username = Username;
            Settings.ExchangeServerSettings.Password = Password;
            Settings.ExchangeServerSettings.ExchangeServerUrl = ExchangeServerUrl;
            Settings.SyncSettings.CalendarSyncDirection = SelectedCalendarSyncDirection;
            Settings.SyncSettings.MasterCalendar = MasterCalendarServiceType;
            Settings.SyncSettings.DisableDelete = DisableDelete;
            Settings.SyncSettings.ConfirmOnDelete = ConfirmOnDelete;
            Settings.SetCalendarTypes();

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


        private async void GetGoogleCalendar()
        {
            IsLoading = true;
            try
            {
                ApplicationLogger.LogInfo("Loading Google calendars...");
                await GetGoogleCalendarInternal();
                ApplicationLogger.LogInfo("Google calendars loaded...");
            }
            catch (AggregateException exception)
            {
                AggregateException flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
                ApplicationLogger.LogError(flattenException.ToString());
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync(exception.Message);
                ApplicationLogger.LogError(exception.ToString());
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GetGoogleCalendarInternal()
        {
            List<Calendar> calendars = await GoogleCalendarService.GetAvailableCalendars(new Dictionary<string, object>());
            GoogleCalenders = calendars;
            if (GoogleCalenders.Any())
            {
                SelectedCalendar = Settings != null && Settings.GoogleCalendar != null
                    ? GoogleCalenders.FirstOrDefault(t => t.Id.Equals(Settings.GoogleCalendar.Id))
                    : GoogleCalenders.First();
            }
        }


        async void ResetGoogleCalendarHandler()
        {
            IsLoading = true;
            await ResetGoogleCalendarInternal();
            IsLoading = false;
        }

        private async Task ResetGoogleCalendarInternal()
        {
            if (SelectedCalendar == null)
            {
                MessageService.ShowMessageAsync("Please select a Google calendar to wipe");
                return;
            }

            var task = await MessageService.ShowConfirmMessage("Are you sure you want to reset events from 10 year past and 10 year future?");
            if (task != MessageDialogResult.Affirmative)
            {
                return;
            }

            var calendarSpecificData = new Dictionary<string, object>() { { "CalendarId", SelectedCalendar.Id } };
            var result = await GoogleCalendarService.ResetCalendar(calendarSpecificData);
            if (!result)
            {
                MessageService.ShowMessageAsync("Reset calendar failed.");
            }
        }

        private async void ResetOutlookCalendarHandler()
        {
            IsLoading = true;
            await ResetOutlookCalendarInternal();
            IsLoading = false;
        }

        private async Task ResetOutlookCalendarInternal()
        {
            if ((!IsDefaultMailBox && (SelectedOutlookMailBox == null || SelectedOutlookCalendar == null)) ||
                (!IsDefaultProfile && string.IsNullOrEmpty(SelectedOutlookProfileName)))
            {
                MessageService.ShowMessageAsync("Please select a Outlook calendar to reset.");
                return;
            }

            var task = await MessageService.ShowConfirmMessage("Are you sure you want to reset events from 10 year past and 10 year future?");
            if (task != MessageDialogResult.Affirmative)
            {
                return;
            }

            var calendarSpecificData = new Dictionary<string, object>()
            {
                {"ProfileName",  SelectedOutlookProfileName},
                {"OutlookCalendar", SelectedOutlookCalendar}
            };

            var result = await OutlookCalendarService.ResetCalendar(calendarSpecificData);
            if (!result)
            {
                MessageService.ShowMessageAsync("Reset calendar failed.");
            }

        }
        #endregion

        private bool _isloaded = false;
        private bool _addAttendeesToDescription;
        private bool _allowMasterCalendarSelect;
        private CalendarServiceType _masterCalendarServiceType;
        private bool _confirmOnDelete;
        private bool _disableDelete;
        private bool _keepLastModifiedCopy;
        private List<OutlookCalendar> _exchangeCalendarList;
        private OutlookCalendar _selectedExchangeCalendar;
        private DelegateCommand _resetGoogleCalendar;
        private DelegateCommand _resetOutlookCalendarCommand;

        public void Load()
        {
            if (_isloaded)
                return;
            LoadSettingsAndGetCalenders();
            _isloaded = true;
        }
    }
}