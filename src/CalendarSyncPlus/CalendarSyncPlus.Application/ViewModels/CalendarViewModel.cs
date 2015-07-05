using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.ExchangeWebServices.Calendar;
using CalendarSyncPlus.GoogleServices.Calendar;
using CalendarSyncPlus.OutlookServices.Calendar;
using CalendarSyncPlus.OutlookServices.Utilities;
using CalendarSyncPlus.Services.Interfaces;
using log4net;
using MahApps.Metro.Controls.Dialogs;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class CalendarViewModel : ViewModel<ICalendarView>
    {
        private bool _allowMasterCalendarSelect;
        private DelegateCommand _autoDetectExchangeServer;
        private List<SyncDirectionEnum> _calendarSyncModes;
        private List<Category> _categories;
        private DelegateCommand _getGoogleCalendarCommand;
        private DelegateCommand _getOutlookMailboxCommand;
        private DelegateCommand _getOutlookProfileLIstCommand;
        private List<GoogleCalendar> _googleCalendars;
        private OutlookOptionsEnum _isDefaultMailBox;
        private OutlookOptionsEnum _isDefaultProfile;
        private List<OutlookMailBox> _outlookMailBoxes;
        private List<string> _outlookProfileList;
        private DelegateCommand _resetGoogleCalendar;
        private DelegateCommand _resetOutlookCalendarCommand;
        private List<string> _syncFrequencies;
        private string _syncFrequency;
        private SyncFrequencyViewModel _syncFrequencyViewModel;
        private List<SyncRangeTypeEnum> _syncRangeTypes;
        private ObservableCollection<CalendarSyncProfile> _calendarSyncProfiles;
        private CalendarSyncProfile _selectedProfile;
        private ObservableCollection<GoogleAccount> _googleAccounts;
        private DelegateCommand _addGoogleAccountCommand;
        private DelegateCommand _disconnectAccountCommand;
        private bool _isLoading;

        [ImportingConstructor]
        public CalendarViewModel(ICalendarView calendarView, 
            IGoogleCalendarService googleCalendarService,
            IOutlookCalendarService outlookCalendarService,
            IMessageService messageService, 
            IExchangeWebCalendarService exchangeWebCalendarService,
            ApplicationLogger applicationLogger, 
            IAccountAuthenticationService accountAuthenticationService)
            :base(calendarView)
        {
            ExchangeWebCalendarService = exchangeWebCalendarService;
            Logger = applicationLogger.GetLogger(GetType());
            AccountAuthenticationService = accountAuthenticationService;
            GoogleCalendarService = googleCalendarService;
            OutlookCalendarService = outlookCalendarService;
            MessageService = messageService;
        }

        public IGoogleCalendarService GoogleCalendarService { get; set; }
        public IOutlookCalendarService OutlookCalendarService { get; set; }
        public IMessageService MessageService { get; set; }
        public IExchangeWebCalendarService ExchangeWebCalendarService { get; private set; }
        public ILog Logger { get; private set; }
        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        #region Properties

        public List<string> OutlookProfileList
        {
            get { return _outlookProfileList; }
            set { SetProperty(ref _outlookProfileList, value); }
        }

        public List<OutlookMailBox> OutlookMailBoxes
        {
            get { return _outlookMailBoxes; }
            set { SetProperty(ref _outlookMailBoxes, value); }
        }
        
        public List<SyncRangeTypeEnum> SyncRangeTypes
        {
            get { return _syncRangeTypes; }
            set { SetProperty(ref _syncRangeTypes, value); }
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

        public List<SyncDirectionEnum> CalendarSyncModes
        {
            get { return _calendarSyncModes; }
            set { SetProperty(ref _calendarSyncModes, value); }
        }

        public bool AllowMasterCalendarSelect
        {
            get { return _allowMasterCalendarSelect; }
            set { SetProperty(ref _allowMasterCalendarSelect, value); }
        }
        
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }
        
        public List<Category> Categories
        {
            get { return _categories; }
            set { SetProperty(ref _categories, value); }
        }
        
        public ObservableCollection<CalendarSyncProfile> CalendarSyncProfiles
        {
            get { return _calendarSyncProfiles; }
            set { SetProperty(ref _calendarSyncProfiles, value); }
        }

        public CalendarSyncProfile SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                SetProperty(ref _selectedProfile, value);
                LoadSyncProfile();
            }
        }

        public ObservableCollection<GoogleAccount> GoogleAccounts
        {
            get { return _googleAccounts; }
            set { SetProperty(ref _googleAccounts, value); }
        }

        public DelegateCommand AddGoogleAccountCommand
        {
            get { return _addGoogleAccountCommand; }
            set { SetProperty(ref _addGoogleAccountCommand, value); }
        }

        public DelegateCommand DisconnectAccountCommand
        {
            get { return _disconnectAccountCommand; }
            set { SetProperty(ref _disconnectAccountCommand, value); }
        }

        #endregion

        #region Commands

        public DelegateCommand GetOutlookProfileListCommand
        {
            get
            {
                return _getOutlookProfileLIstCommand ??
                       (_getOutlookProfileLIstCommand = new DelegateCommand(GetOutlookProfileList));
            }
        }

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

        internal void Initialize()
        {
            SyncRangeTypes = new List<SyncRangeTypeEnum>
            {
                SyncRangeTypeEnum.SyncEntireCalendar,
                SyncRangeTypeEnum.SyncFixedDateRange,
                SyncRangeTypeEnum.SyncRangeInDays
            };
            CalendarSyncModes = new List<SyncDirectionEnum>
            {
                SyncDirectionEnum.OutlookGoogleOneWay,
                SyncDirectionEnum.OutlookGoogleOneWayToSource,
                SyncDirectionEnum.OutlookGoogleTwoWay
            };
            SyncFrequencies = new List<string>
            {
                "Interval",
                "Daily",
                "Weekly"
            };

            SyncFrequency = "Interval";
            Categories = CategoryHelper.GetCategories();
            
        }

        private async void GetOutlookMailBoxes()
        {
            if (SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultMailBoxCalendar) || IsLoading)
                return;
            IsLoading = true;
            await GetOutlookMailBoxesInternal();
            IsLoading = false;
        }

        private async Task GetOutlookMailBoxesInternal()
        {
            try
            {
                var mailBoxes = await Task<List<OutlookMailBox>>.Factory.StartNew(GetOutlookMailBox);
                if (mailBoxes == null)
                {
                    MessageService.ShowMessageAsync("Failed to fetch outlook mailboxes. Please try again."+
                        Environment.NewLine+"If the problem persists, Please restart Outlook application.");
                    return;
                }

                if (SelectedProfile.OutlookSettings.OutlookMailBox != null && mailBoxes.Any())
                {
                    var mailbox = mailBoxes.FirstOrDefault(
                        t => t.EntryId.Equals(SelectedProfile.OutlookSettings.OutlookMailBox.EntryId)) ?? mailBoxes.First();

                    if (SelectedProfile.OutlookSettings.OutlookCalendar != null)
                    {
                        SelectedProfile.OutlookSettings.OutlookCalendar =
                            mailbox.Folders.FirstOrDefault(
                                t => t.EntryId.Equals(SelectedProfile.OutlookSettings.OutlookCalendar.EntryId)) ??
                            mailbox.Folders.First();
                    }
                    SelectedProfile.OutlookSettings.OutlookMailBox = mailbox;
                }
                OutlookMailBoxes = mailBoxes;
            }
            catch (Exception aggregateException)
            {
                Logger.Error(aggregateException);
            }
        }

        private List<OutlookMailBox> GetOutlookMailBox()
        {
            return OutlookCalendarService.GetAllMailBoxes(SelectedProfile.OutlookSettings.OutlookProfileName ?? string.Empty);
        }

        private async void AutoDetectEWSSettings()
        {
            IsLoading = true;

            IsLoading = false;
        }

        private async void GetOutlookProfileList()
        {
            if (SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile) || IsLoading)
                return;
            IsLoading = true;

            await GetOutlookProfileListInternal();

            IsLoading = false;
        }

        private async Task GetOutlookProfileListInternal()
        {
            OutlookProfileList = await OutlookCalendarService.GetOutLookProfieListAsync();
            if (OutlookProfileList == null)
            {
                MessageService.ShowMessageAsync("Failed to fetch outlook profiles. Please try again.");
            }
        }


        internal async void GetGoogleCalendar()
        {
            if (IsLoading)
                return;
            //TODO : Move this method to main setitngs view
            IsLoading = true;
            try
            {
                if (SelectedProfile.GoogleSettings.GoogleAccount == null)
                {
                    MessageService.ShowMessageAsync("Please select a Google account to get calendars");
                    return;
                }
                Logger.Info("Loading Google calendars...");
                await GetGoogleCalendarInternal();
                Logger.Info("Google calendars loaded...");
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                MessageService.ShowMessageAsync(flattenException.Message);
                Logger.Error(flattenException);
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync(exception.Message);
                Logger.Error(exception);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GetGoogleCalendarInternal()
        {
            try
            {
                if (SelectedProfile.GoogleSettings.GoogleAccount == null ||
                    SelectedProfile.GoogleSettings.GoogleAccount.Name == null)
                {
                    return;
                }
                var calendars =  await GoogleCalendarService.GetAvailableCalendars(
                    SelectedProfile.GoogleSettings.GoogleAccount.Name);

                if (calendars.Any())
                {
                    if (SelectedProfile.GoogleSettings.GoogleCalendar != null)
                    {
                        SelectedProfile.GoogleSettings.GoogleCalendar = calendars.FirstOrDefault(t =>
                            t.Id.Equals(SelectedProfile.GoogleSettings.GoogleCalendar.Id));
                    }

                    if (SelectedProfile.GoogleSettings.GoogleCalendar == null)
                    {
                        SelectedProfile.GoogleSettings.GoogleCalendar = calendars.First();
                    }
                }
                SelectedProfile.GoogleSettings.GoogleCalendars = calendars;
            }
            catch (Exception exception)
            {
                MessageService.ShowMessageAsync("Unable to get Google calendars.");
                Logger.Error(exception);
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
            if (SelectedProfile.GoogleSettings.GoogleAccount == null ||
                SelectedProfile.GoogleSettings.GoogleCalendar == null)
            {
                MessageService.ShowMessageAsync("Please select a Google calendar to wipe");
                return;
            }

            var task =
                await
                    MessageService.ShowConfirmMessage(
                        "Are you sure you want to reset events from 10 year past and 10 year future?");
            if (task != MessageDialogResult.Affirmative)
            {
                return;
            }

            var calendarSpecificData = new Dictionary<string, object>
            {
                {"CalendarId", SelectedProfile.GoogleSettings.GoogleCalendar.Id},
                {"AccountName", SelectedProfile.GoogleSettings.GoogleAccount.Name}
            };
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
            if ((SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.AlternateMailBoxCalendar) &&
                 (SelectedProfile.OutlookSettings.OutlookMailBox == null || SelectedProfile.OutlookSettings.OutlookCalendar == null)) ||
                (SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.AlternateProfile) &&
                 string.IsNullOrEmpty(SelectedProfile.OutlookSettings.OutlookProfileName)))
            {
                MessageService.ShowMessageAsync("Please select a Outlook calendar to reset.");
                return;
            }

            var task =
                await
                    MessageService.ShowConfirmMessage(
                        "Are you sure you want to reset events from 10 year past and 10 year future?");
            if (task != MessageDialogResult.Affirmative)
            {
                return;
            }

            var calendarSpecificData = new Dictionary<string, object>
            {
                {
                    "ProfileName", SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.AlternateProfile)
                        ? SelectedProfile.OutlookSettings.OutlookProfileName
                        : null
                },
                {
                    "OutlookCalendar", SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.AlternateMailBoxCalendar)
                        ? SelectedProfile.OutlookSettings.OutlookCalendar
                        : null
                },
                {"AddAsAppointments", SelectedProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AddAsAppointments)}
            };

            var result = await OutlookCalendarService.ResetCalendar(calendarSpecificData);
            if (!result)
            {
                MessageService.ShowMessageAsync("Reset calendar failed.");
            }
        }

        /// <summary>
        /// </summary>
        private void OnSyncFrequencyChanged()
        {
            if (SelectedProfile != null && SelectedProfile.SyncFrequency != null &&
                SyncFrequency == SelectedProfile.SyncFrequency.Name)
            {
                switch (SyncFrequency)
                {
                    case "Interval":
                        SyncFrequencyViewModel
                            = new IntervalSyncViewModel(SelectedProfile.SyncFrequency as IntervalSyncFrequency);
                        break;
                    case "Daily":
                        SyncFrequencyViewModel
                            = new DailySyncViewModel(SelectedProfile.SyncFrequency as DailySyncFrequency);
                        break;
                    case "Weekly":
                        SyncFrequencyViewModel
                            = new WeeklySyncViewModel(SelectedProfile .SyncFrequency as WeeklySyncFrequency);
                        break;
                }
            }
            else
            {
                switch (SyncFrequency)
                {
                    case "Interval":
                        SyncFrequencyViewModel = new IntervalSyncViewModel();
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
            if (SelectedProfile == null || SelectedProfile.IsLoaded)
                return;

            IsLoading = true;
            
            if (!SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile))
            {
                await GetOutlookProfileListInternal();
            }

            if (!SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultMailBoxCalendar))
            {
                await GetOutlookMailBoxesInternal();
            }

            if (SelectedProfile.GoogleSettings.GoogleAccount != null)
            {
                SelectedProfile.GoogleSettings.GoogleAccount = GoogleAccounts.FirstOrDefault(t =>
                    t.Name.Equals(SelectedProfile.GoogleSettings.GoogleAccount.Name));
                
                if (SelectedProfile.GoogleSettings.GoogleCalendar != null)
                {
                    await GetGoogleCalendarInternal();
                }
            }

            SyncFrequency = SelectedProfile.SyncFrequency.Name;

            if (SelectedProfile.EventCategory != null)
            {
                SelectedProfile.EventCategory =
                    Categories.First(t => t.CategoryName.Equals(SelectedProfile.EventCategory.CategoryName));
            }

            SelectedProfile.IsLoaded = true;
            IsLoading = false;
        }


        #endregion
    }
}