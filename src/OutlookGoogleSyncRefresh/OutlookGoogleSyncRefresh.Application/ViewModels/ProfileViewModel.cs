using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Foundation;
using CalendarSyncPlus.Application.Services;
using CalendarSyncPlus.Application.Services.ExchangeWeb;
using CalendarSyncPlus.Application.Services.Google;
using CalendarSyncPlus.Application.Services.Outlook;
using CalendarSyncPlus.Application.Utilities;
using CalendarSyncPlus.Application.Wrappers;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using MahApps.Metro.Controls.Dialogs;

namespace CalendarSyncPlus.Application.ViewModels
{
    public class ProfileViewModel : Model
    {
        private bool _addAttachments;
        private bool _addAttendees;
        private bool _addAttendeesToDescription;
        private bool _addDescription;
        private bool _addReminders;
        private bool _allowMasterCalendarSelect;
        private DelegateCommand _autoDetectExchangeServer;
        private List<CalendarSyncDirectionEnum> _calendarSyncModes;
        private List<Category> _categories;
        private bool _confirmOnDelete;
        private int _daysInFuture = 7;
        private int _daysInPast = 1;
        private bool _disableDelete;
        private List<OutlookCalendar> _exchangeCalendarList;
        private string _exchangeServerUrl;
        private DelegateCommand _getGoogleCalendarCommand;
        private DelegateCommand _getOutlookMailboxCommand;
        private DelegateCommand _getOutlookProfileLIstCommand;
        private List<Calendar> _googleCalenders;
        private bool _isDefault;
        private bool _isDefaultMailBox = true;
        private bool _isDefaultProfile = true;
        private bool _isExchangeWebServices;
        private bool _isLoading;
        private bool _isSyncEnabled;
        private bool _keepLastModifiedCopy;
        private CalendarServiceType _masterCalendarServiceType;
        private string _name;
        private OutlookCalendar _outlookCalendar;
        private OutlookMailBox _outlookMailBox;
        private List<OutlookMailBox> _outlookMailBoxes;
        private List<string> _outlookProfileList;
        private string _password;
        private DelegateCommand _resetGoogleCalendar;
        private DelegateCommand _resetOutlookCalendarCommand;
        private Calendar _selectedCalendar;
        private CalendarSyncDirectionEnum _selectedCalendarSyncDirection;
        private Category _selectedCategory;
        private OutlookCalendar _selectedExchangeCalendar;
        private string _selectedOutlookProfileName;
        private bool _setCategory;
        private List<string> _syncFrequencies;
        private string _syncFrequency;
        private SyncFrequencyViewModel _syncFrequencyViewModel;
        private string _username;

        public ProfileViewModel(CalendarSyncProfile syncProfile, IGoogleCalendarService googleCalendarService,
            IOutlookCalendarService outlookCalendarService,
            IMessageService messageService, IExchangeWebCalendarService exchangeWebCalendarService,
            ApplicationLogger applicationLogger)
        {
            SyncProfile = syncProfile;
            ExchangeWebCalendarService = exchangeWebCalendarService;
            ApplicationLogger = applicationLogger;
            GoogleCalendarService = googleCalendarService;
            OutlookCalendarService = outlookCalendarService;
            MessageService = messageService;
            Initialize();
        }

        public CalendarSyncProfile SyncProfile { get; set; }
        public IGoogleCalendarService GoogleCalendarService { get; set; }
        public IOutlookCalendarService OutlookCalendarService { get; set; }
        public IMessageService MessageService { get; set; }
        public IExchangeWebCalendarService ExchangeWebCalendarService { get; private set; }
        public ApplicationLogger ApplicationLogger { get; private set; }

        #region Properties

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public bool IsSyncEnabled
        {
            get { return _isSyncEnabled; }
            set { SetProperty(ref _isSyncEnabled, value); }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
            set { SetProperty(ref _isDefault, value); }
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


        public DelegateCommand GetOutlookProfileListCommand
        {
            get
            {
                return _getOutlookProfileLIstCommand ??
                       (_getOutlookProfileLIstCommand = new DelegateCommand(GetOutlookProfileList));
            }
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

        public SyncFrequencyViewModel SyncFrequencyViewModel
        {
            get { return _syncFrequencyViewModel; }
            set { SetProperty(ref _syncFrequencyViewModel, value); }
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
                else
                {
                    AllowMasterCalendarSelect = false;
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public bool SetCategory
        {
            get { return _setCategory; }
            set { SetProperty(ref _setCategory, value); }
        }

        public List<Category> Categories
        {
            get { return _categories; }
            set { SetProperty(ref _categories, value); }
        }

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set { SetProperty(ref _selectedCategory, value); }
        }

        #endregion

        #region Commands

        public DelegateCommand GetOutlookMailBoxesCommand
        {
            get
            {
                return _getOutlookMailboxCommand ??
                       (_getOutlookMailboxCommand = new DelegateCommand(GetOutlookMailBoxes));
            }
        }

        public DelegateCommand GetGoogleCalendarCommand
        {
            get
            {
                return _getGoogleCalendarCommand ?? (_getGoogleCalendarCommand = new DelegateCommand(GetGoogleCalendar));
            }
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

        #endregion

        #region Private Methods

        private void Initialize()
        {
            Name = SyncProfile.Name;
            IsSyncEnabled = SyncProfile.IsSyncEnabled;
            IsDefault = SyncProfile.IsDefault;

            CalendarSyncModes = new List<CalendarSyncDirectionEnum>
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
            Categories = CategoryHelper.GetCategories();
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
                if (SyncProfile.OutlookSettings.OutlookMailBox != null)
                {
                    SelectedOutlookMailBox =
                        OutlookMailBoxes.FirstOrDefault(
                            t => t.EntryId.Equals(SyncProfile.OutlookSettings.OutlookMailBox.EntryId));
                    if (SyncProfile.OutlookSettings.OutlookCalendar != null && SelectedOutlookMailBox != null)
                    {
                        SelectedOutlookCalendar =
                            SelectedOutlookMailBox.Calendars.FirstOrDefault(
                                t => t.EntryId.Equals(SyncProfile.OutlookSettings.OutlookCalendar.EntryId));
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
            List<Calendar> calendars =
                await GoogleCalendarService.GetAvailableCalendars(new Dictionary<string, object>());
            GoogleCalenders = calendars;
            if (GoogleCalenders.Any())
            {
                SelectedCalendar = SyncProfile != null && SyncProfile.GoogleCalendar != null
                    ? GoogleCalenders.FirstOrDefault(t => t.Id.Equals(SyncProfile.GoogleCalendar.Id))
                    : GoogleCalenders.First();
            }
        }


        private async void ResetGoogleCalendarHandler()
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

            MessageDialogResult task =
                await
                    MessageService.ShowConfirmMessage(
                        "Are you sure you want to reset events from 10 year past and 10 year future?");
            if (task != MessageDialogResult.Affirmative)
            {
                return;
            }

            var calendarSpecificData = new Dictionary<string, object> {{"CalendarId", SelectedCalendar.Id}};
            bool result = await GoogleCalendarService.ResetCalendar(calendarSpecificData);
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

            MessageDialogResult task =
                await
                    MessageService.ShowConfirmMessage(
                        "Are you sure you want to reset events from 10 year past and 10 year future?");
            if (task != MessageDialogResult.Affirmative)
            {
                return;
            }

            var calendarSpecificData = new Dictionary<string, object>
            {
                {"ProfileName", SelectedOutlookProfileName},
                {"OutlookCalendar", SelectedOutlookCalendar}
            };

            bool result = await OutlookCalendarService.ResetCalendar(calendarSpecificData);
            if (!result)
            {
                MessageService.ShowMessageAsync("Reset calendar failed.");
            }
        }

        private void OnSyncFrequencyChanged()
        {
            if (SyncProfile != null && SyncProfile.SyncSettings.SyncFrequency != null &&
                SyncFrequency == SyncProfile.SyncSettings.SyncFrequency.Name)
            {
                switch (SyncFrequency)
                {
                    case "Hourly":
                        SyncFrequencyViewModel
                            = new HourlySyncViewModel(SyncProfile.SyncSettings.SyncFrequency as HourlySyncFrequency);
                        break;
                    case "Daily":
                        SyncFrequencyViewModel
                            = new DailySyncViewModel(SyncProfile.SyncSettings.SyncFrequency as DailySyncFrequency);
                        break;
                    case "Weekly":
                        SyncFrequencyViewModel
                            = new WeeklySyncViewModel(SyncProfile.SyncSettings.SyncFrequency as WeeklySyncFrequency);
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


        public async void LoadSyncProfile()
        {
            IsLoading = true;
            if (SyncProfile != null)
            {
                DaysInPast = SyncProfile.DaysInPast;
                DaysInFuture = SyncProfile.DaysInFuture;
                AddAttendees = SyncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees);
                if (AddAttendees)
                {
                    AddAttendeesToDescription =
                        SyncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription);
                }
                AddDescription = SyncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
                AddReminders = SyncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);
                AddAttachments = SyncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attachments);
                IsDefaultProfile = SyncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile);
                IsDefaultMailBox =
                    SyncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultCalendar);
                IsExchangeWebServices =
                    SyncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices);
                SelectedOutlookProfileName = SyncProfile.OutlookSettings.OutlookProfileName;
                SelectedCalendarSyncDirection = SyncProfile.SyncSettings.CalendarSyncDirection;
                MasterCalendarServiceType = SyncProfile.SyncSettings.MasterCalendar;
                DisableDelete = SyncProfile.SyncSettings.DisableDelete;
                ConfirmOnDelete = SyncProfile.SyncSettings.ConfirmOnDelete;
                KeepLastModifiedCopy = SyncProfile.SyncSettings.KeepLastModifiedVersion;
                SyncFrequency = SyncProfile.SyncSettings.SyncFrequency.Name;

                if (!IsDefaultProfile)
                {
                    await GetOutlookProfileListInternal();
                }

                if (!IsDefaultMailBox)
                {
                    await GetOutlookMailBoxesInternal();
                }

                if (SyncProfile != null && SyncProfile.GoogleCalendar != null)
                {
                    await GetGoogleCalendarInternal();
                }
            }
            IsLoading = false;
        }


        public CalendarSyncProfile SaveCurrentSyncProfile()
        {
            SyncProfile.IsSyncEnabled = IsSyncEnabled;
            SyncProfile.GoogleCalendar = SelectedCalendar;
            SyncProfile.DaysInFuture = DaysInFuture;
            SyncProfile.DaysInPast = DaysInPast;
            SyncProfile.SyncSettings.SyncFrequency = SyncFrequencyViewModel.GetFrequency();
            SyncProfile.UpdateEntryOptions(AddDescription, AddReminders, AddAttendees, AddAttendeesToDescription,
                AddAttachments);
            SyncProfile.OutlookSettings.OutlookMailBox = SelectedOutlookMailBox;
            SyncProfile.OutlookSettings.OutlookCalendar = SelectedOutlookCalendar;
            SyncProfile.OutlookSettings.OutlookProfileName = SelectedOutlookProfileName;
            SyncProfile.OutlookSettings.UpdateOutlookOptions(IsDefaultProfile, IsDefaultMailBox,
                IsExchangeWebServices);
            SyncProfile.ExchangeServerSettings.Username = Username;
            SyncProfile.ExchangeServerSettings.Password = Password;
            SyncProfile.ExchangeServerSettings.ExchangeServerUrl = ExchangeServerUrl;
            SyncProfile.SyncSettings.CalendarSyncDirection = SelectedCalendarSyncDirection;
            SyncProfile.SyncSettings.MasterCalendar = MasterCalendarServiceType;
            SyncProfile.SyncSettings.DisableDelete = DisableDelete;
            SyncProfile.SyncSettings.ConfirmOnDelete = ConfirmOnDelete;
            SyncProfile.SyncSettings.KeepLastModifiedVersion = KeepLastModifiedCopy;
            SyncProfile.SetCalendarTypes();

            return SyncProfile;
        }

        #endregion
    }
}