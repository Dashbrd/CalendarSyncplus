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
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.Services.Outlook;
using OutlookGoogleSyncRefresh.Application.Views;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Helpers;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class SettingsViewModel : Utilities.ViewModel<ISettingsView>
    {
        #region Constructors

        [ImportingConstructor]
        public SettingsViewModel(ISettingsView view,
            IGoogleCalendarService googleCalendarService,
            Settings settings,
            ISettingsSerializationService serializationService, IOutlookCalendarService outlookCalendarService,
            IMessageService messageService,
            ApplicationLogger applicationLogger)
            : base(view)
        {
            _settings = settings;
            _applicationLogger = applicationLogger;
            GoogleCalendarService = googleCalendarService;
            SettingsSerializationService = serializationService;
            OutlookCalendarService = outlookCalendarService;
            MessageService = messageService;
            LoadSettingsAndGetCalenders();
        }

        #endregion

        #region Fields

        private bool _addAttendees;
        private bool _addDescription;
        private bool _addReminders;
        private bool _createNewFileForEverySync;
        private int _daysInFuture;
        private int _daysInPast;
        private DelegateCommand _getGoogleCalendarCommand;
        private List<Calendar> _googleCalenders;
        private bool _logSyncInfo;
        private DelegateCommand _getOutlookMailboxCommand;
        private DelegateCommand _saveCommand;
        private Calendar _selectedCalendar;
        private Settings _settings;
        private readonly ApplicationLogger _applicationLogger;
        private List<string> _syncFrequencies;
        private string _syncFrequency;
        private SyncFrequencyViewModel _syncFrequencyViewModel;
        private bool _isLoading;
        private List<OutlookMailBox> _outlookMailBoxes;
        private OutlookCalendar _outlookCalendar;
        private OutlookMailBox _outlookMailBox;
        private bool _isDefaultMailBox = true;
        private bool _isDefaultProfile;
        private bool _isExchangeWebServices;
        private string _selectedOutlookProfileName;
        private List<string> _outlookProfileList;
        private DelegateCommand _getOutlookProfileLIstCommand;
        private bool _minimizeToSystemTray;
        private bool _hideSystemTrayTooltip;
        private bool _settingsSaved;
        private bool _autoDetectExchangeServer;
        private string _username;
        private string _password;
        private string _exchangeServerUrl;
        private bool _addAttachments;

        #endregion

        #region Properties

        public IGoogleCalendarService GoogleCalendarService { get; set; }
        public ISettingsSerializationService SettingsSerializationService { get; set; }
        public IOutlookCalendarService OutlookCalendarService { get; set; }
        public IMessageService MessageService { get; set; }

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
                        OutlookMailBoxes.FirstOrDefault(t => t.EntryId.Equals(Settings.OutlookSettings.OutlookMailBox.EntryId));
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
                var exception = aggregateException.ToString();
            }
        }

        private List<OutlookMailBox> GetOutlookMailBox()
        {
            return OutlookCalendarService.GetAllMailBoxes(SelectedOutlookProfileName ?? string.Empty);
        }

        public bool AddDescription
        {
            get { return _addDescription; }
            set { SetProperty(ref _addDescription, value); }
        }

        public bool AddAttendees
        {
            get { return _addAttendees; }
            set { SetProperty(ref _addAttendees, value); }
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

        public bool AutoDetectExchangeServer
        {
            get { return _autoDetectExchangeServer; }
            set { SetProperty(ref _autoDetectExchangeServer, value); }
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
            if (Settings != null && Settings.SyncFrequency != null && SyncFrequency == Settings.SyncFrequency.Name)
            {
                switch (SyncFrequency)
                {
                    case "Hourly":
                        SyncFrequencyViewModel
                            = new HourlySyncViewModel(Settings.SyncFrequency as HourlySyncFrequency);
                        break;
                    case "Daily":
                        SyncFrequencyViewModel
                            = new DailySyncViewModel(Settings.SyncFrequency as DailySyncFrequency);
                        break;
                    case "Weekly":
                        SyncFrequencyViewModel
                            = new WeeklySyncViewModel(Settings.SyncFrequency as WeeklySyncFrequency);
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
                SyncFrequencies = new List<string>
                {
                    "Hourly",
                    "Daily",
                    "Weekly"
                };
                SyncFrequency = "Hourly";
                if (Settings != null)
                {
                    if (Settings.SyncFrequency != null)
                    {
                        SyncFrequency = Settings.SyncFrequency.Name;
                    }

                    DaysInPast = Settings.DaysInPast;
                    DaysInFuture = Settings.DaysInFuture;
                    AddAttendees = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees);
                    AddDescription = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
                    AddReminders = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);
                    AddAttachments = Settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attachments);
                    LogSyncInfo = Settings.LogSettings.LogSyncInfo;
                    CreateNewFileForEverySync = CreateNewFileForEverySync;
                    IsDefaultMailBox = Settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultCalendar);
                    IsDefaultProfile = Settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile);
                    IsExchangeWebServices = Settings.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices);
                    SelectedOutlookProfileName = Settings.OutlookSettings.OutlookProfileName;
                    MinimizeToSystemTray = Settings.MinimizeToSystemTray;
                    HideSystemTrayTooltip = Settings.HideSystemTrayTooltip;
                }
                else
                {
                    DaysInPast = 0;
                    DaysInFuture = 1;
                    LogSyncInfo = true;
                    AddDescription = true;
                    CreateNewFileForEverySync = false;
                    IsDefaultMailBox = true;
                    IsDefaultProfile = true;
                    MinimizeToSystemTray = true;
                    HideSystemTrayTooltip = false;
                }

                if (!IsDefaultProfile)
                {
                    await GetOutlookProfileListInternal();
                }

                if (!IsDefaultMailBox)
                {
                    await GetOutlookMailBoxesInternal();
                }

                if (Settings != null && Settings.SavedCalendar != null)
                {
                    await GetGoogleCalendarInternal();
                }
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
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
            Settings.SavedCalendar = SelectedCalendar;
            Settings.DaysInFuture = DaysInFuture;
            Settings.DaysInPast = DaysInPast;
            Settings.SyncFrequency = SyncFrequencyViewModel.GetFrequency();
            Settings.UpdateEntryOptions(AddDescription, AddReminders, AddAttendees, AddAttachments);
            Settings.LogSettings.LogSyncInfo = LogSyncInfo;
            Settings.LogSettings.CreateNewFileForEverySync = CreateNewFileForEverySync;
            Settings.OutlookSettings.OutlookMailBox = SelectedOutlookMailBox;
            Settings.OutlookSettings.OutlookCalendar = SelectedOutlookCalendar;
            Settings.OutlookSettings.OutlookProfileName = SelectedOutlookProfileName;
            Settings.MinimizeToSystemTray = MinimizeToSystemTray;
            Settings.HideSystemTrayTooltip = HideSystemTrayTooltip;
            Settings.OutlookSettings.UpdateOutlookOptions(IsDefaultProfile, IsDefaultMailBox, IsExchangeWebServices);
            Settings.ExchangeServerSettings.AutoDetectExchangeServer = AutoDetectExchangeServer;
            Settings.ExchangeServerSettings.Username = Username;
            Settings.ExchangeServerSettings.Password = Password;
            Settings.ExchangeServerSettings.ExchangeServerUrl = ExchangeServerUrl;
            bool result;
            try
            {
                result = await SettingsSerializationService.SerializeSettingsAsync(Settings);
                await MessageService.ShowMessage(result ? "Settings Saved Successfully" : "Error Saving Settings", "Settings");
                SettingsSaved = true;
            }
            catch (AggregateException exception)
            {
                SettingsSaved = false;
                var flattenException = exception.Flatten();
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
                _applicationLogger.LogInfo("Loading Google calendars...");
                await GetGoogleCalendarInternal();
                _applicationLogger.LogInfo("Google calendars loaded...");
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
                _applicationLogger.LogError(flattenException.ToString());
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync(exception.Message);
                _applicationLogger.LogError(exception.ToString());
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GetGoogleCalendarInternal()
        {
            var calendars = await GoogleCalendarService.GetAvailableCalendars();
            GoogleCalenders = calendars;
            if (GoogleCalenders.Any())
            {
                SelectedCalendar = Settings != null && Settings.SavedCalendar != null
                    ? GoogleCalenders.FirstOrDefault(t => t.Id.Equals(Settings.SavedCalendar.Id))
                    : GoogleCalenders.First();
            }
        }

        #endregion
    }
}