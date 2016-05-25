using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.ExchangeWebServices.Task;
using CalendarSyncPlus.GoogleServices.Tasks;
using CalendarSyncPlus.OutlookServices.Task;
using CalendarSyncPlus.Services.Interfaces;
using log4net;
using MahApps.Metro.Controls.Dialogs;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class TaskViewModel : ViewModel<ITaskView>
    {
        private DelegateCommand _addGoogleAccountCommand;
        private DelegateCommand _autoDetectExchangeServer;
        private List<SyncDirectionEnum> _calendarSyncModes;
        private DelegateCommand _cleanGoogleCalendar;
        private DelegateCommand _cleanOutlookCalendarCommand;
        private DelegateCommand _disconnectAccountCommand;
        private DelegateCommand _getGoogleCalendarCommand;
        private DelegateCommand _getOutlookMailboxCommand;
        private DelegateCommand _getOutlookProfileLIstCommand;
        private ObservableCollection<GoogleAccount> _googleAccounts;
        private bool _isLoading;
        private List<OutlookMailBox> _outlookMailBoxes;
        private List<string> _outlookProfileList;

        private string _selectedFrequency;
        private TaskSyncProfile _selectedProfile;
        private List<string> _syncFrequencyTypes;


        private ObservableCollection<TaskSyncProfile> _taskSyncProfiles;

        [ImportingConstructor]
        public TaskViewModel(ITaskView view, ApplicationLogger applicationLogger,
            IMessageService messageService, IOutlookTaskService outlookTaskService,
            IGoogleTaskService googleTaskService, IExchangeWebTaskService exchangeWebTaskService) : base(view)
        {
            MessageService = messageService;
            OutlookTaskService = outlookTaskService;
            GoogleTaskService = googleTaskService;
            ExchangeWebTaskService = exchangeWebTaskService;
            Logger = applicationLogger.GetLogger(GetType());
        }

        public IMessageService MessageService { get; set; }
        public IOutlookTaskService OutlookTaskService { get; set; }
        public IGoogleTaskService GoogleTaskService { get; set; }
        public IExchangeWebTaskService ExchangeWebTaskService { get; set; }

        /// <summary>
        /// </summary>
        public void Initialize()
        {
            CalendarSyncModes = new List<SyncDirectionEnum>
            {
                SyncDirectionEnum.OutlookGoogleOneWay,
                SyncDirectionEnum.OutlookGoogleOneWayToSource
                //SyncDirectionEnum.OutlookGoogleTwoWay
            };
            SyncFrequencyTypes = new List<string>
            {
                "Interval",
                "Daily",
                "Weekly"
            };

            SelectedFrequency = "Interval";
        }

        private async void GetOutlookMailBoxes()
        {
            if (SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultMailBoxCalendar) ||
                IsLoading)
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
                    MessageService.ShowMessageAsync("Failed to fetch outlook mailboxes. Please try again." +
                                                    Environment.NewLine +
                                                    "If the problem persists, Please restart Outlook application.");
                    return;
                }

                if (SelectedProfile.OutlookSettings.OutlookMailBox != null && mailBoxes.Any())
                {
                    var mailbox = mailBoxes.FirstOrDefault(
                        t => t.EntryId.Equals(SelectedProfile.OutlookSettings.OutlookMailBox.EntryId)) ??
                                  mailBoxes.First();

                    if (SelectedProfile.OutlookSettings.OutlookFolder != null)
                    {
                        SelectedProfile.OutlookSettings.OutlookFolder =
                            mailbox.Folders.FirstOrDefault(
                                t => t.EntryId.Equals(SelectedProfile.OutlookSettings.OutlookFolder.EntryId)) ??
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
            return OutlookTaskService.GetAllMailBoxes(SelectedProfile.OutlookSettings.OutlookProfileName ?? string.Empty);
        }

        private void AutoDetectEWSSettings()
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
            OutlookProfileList = await OutlookTaskService.GetOutLookProfieListAsync();
            if (OutlookProfileList == null)
            {
                MessageService.ShowMessageAsync("Failed to fetch outlook profiles. Please try again.");
            }
        }


        internal async void GetGoogleTaskLists()
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
                Logger.Info("Loading Google task lists...");
                await GetGoogleTaskListsInternal();
                Logger.Info("Google task lists loaded.");
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

        private async Task GetGoogleTaskListsInternal()
        {
            try
            {
                if (SelectedProfile.GoogleSettings.GoogleAccount == null ||
                    SelectedProfile.GoogleSettings.GoogleAccount.Name == null)
                {
                    return;
                }
                var calendars = await GoogleTaskService.GetAvailableTaskList(
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

        private async void CleanGoogleCalendarHandler()
        {
            IsLoading = true;
            await CleanGoogleCalendarInternal();
            IsLoading = false;
        }

        private async Task CleanGoogleCalendarInternal()
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
                {"TaskListId", SelectedProfile.GoogleSettings.GoogleCalendar.Id},
                {"AccountName", SelectedProfile.GoogleSettings.GoogleAccount.Name}
            };
            var result = await GoogleTaskService.ClearCalendar(calendarSpecificData);
            if (!result)
            {
                MessageService.ShowMessageAsync("Reset calendar failed.");
            }
        }

        private async void CleanOutlookCalendarHandler()
        {
            IsLoading = true;
            await CleanOutlookCalendarInternal();
            IsLoading = false;
        }

        private async Task CleanOutlookCalendarInternal()
        {
            if ((SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.AlternateMailBoxCalendar) &&
                 (SelectedProfile.OutlookSettings.OutlookMailBox == null ||
                  SelectedProfile.OutlookSettings.OutlookFolder == null)) ||
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
                    "ProfileName",
                    SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.AlternateProfile)
                        ? SelectedProfile.OutlookSettings.OutlookProfileName
                        : null
                },
                {
                    "OutlookTaskList",
                    SelectedProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.AlternateMailBoxCalendar)
                        ? SelectedProfile.OutlookSettings.OutlookFolder
                        : null
                }
            };

            var result = await OutlookTaskService.ClearCalendar(calendarSpecificData);
            if (!result)
            {
                MessageService.ShowMessageAsync("Reset calendar failed.");
            }
        }

        /// <summary>
        /// </summary>
        private void OnSyncFrequencyChanged()
        {
            if (SelectedProfile == null)
            {
                return;
            }

            if (SelectedProfile.SyncFrequency == null ||
                !SelectedFrequency.Equals(SelectedProfile.SyncFrequency.Name))
            {
                switch (SelectedFrequency)
                {
                    case "Interval":
                        SelectedProfile.SyncFrequency = new IntervalSyncFrequency();
                        break;
                    case "Daily":
                        SelectedProfile.SyncFrequency = new DailySyncFrequency();
                        break;
                    case "Weekly":
                        SelectedProfile.SyncFrequency = new WeeklySyncFrequency();
                        break;
                }
            }
        }

        public async void LoadSyncProfile()
        {
            if (SelectedProfile == null)
                return;

            IsLoading = true;

            if (SelectedProfile.SyncFrequency != null)
            {
                SelectedFrequency = SelectedProfile.SyncFrequency.Name;
            }

            if (!SelectedProfile.IsLoaded)
            {
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
                        await GetGoogleTaskListsInternal();
                    }
                }

                SelectedProfile.IsLoaded = true;
            }

            IsLoading = false;
        }

        #region Commands

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
                return _getGoogleCalendarCommand ?? (_getGoogleCalendarCommand = new DelegateCommand(GetGoogleTaskLists));
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

        public DelegateCommand CleanOutlookCalendarCommand
        {
            get
            {
                return _cleanOutlookCalendarCommand = _cleanOutlookCalendarCommand ??
                                                      new DelegateCommand(CleanOutlookCalendarHandler);
            }
        }

        public DelegateCommand CleanGoogleCalendarCommand
        {
            get
            {
                return _cleanGoogleCalendar = _cleanGoogleCalendar ?? new DelegateCommand(CleanGoogleCalendarHandler);
            }
        }

        #endregion

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

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        public ILog Logger { get; set; }

        public ObservableCollection<GoogleAccount> GoogleAccounts
        {
            get { return _googleAccounts; }
            set { SetProperty(ref _googleAccounts, value); }
        }

        public ObservableCollection<TaskSyncProfile> TaskSyncProfiles
        {
            get { return _taskSyncProfiles; }
            set { SetProperty(ref _taskSyncProfiles, value); }
        }


        public TaskSyncProfile SelectedProfile
        {
            get { return _selectedProfile; }
            set { SetProperty(ref _selectedProfile, value); }
        }

        /// <summary>
        /// </summary>
        public string SelectedFrequency
        {
            get { return _selectedFrequency; }
            set
            {
                SetProperty(ref _selectedFrequency, value);
                OnSyncFrequencyChanged();
            }
        }

        /// <summary>
        /// </summary>
        public List<string> SyncFrequencyTypes
        {
            get { return _syncFrequencyTypes; }
            set { SetProperty(ref _syncFrequencyTypes, value); }
        }

        /// <summary>
        /// </summary>
        public List<SyncDirectionEnum> CalendarSyncModes
        {
            get { return _calendarSyncModes; }
            set { SetProperty(ref _calendarSyncModes, value); }
        }

        #endregion
    }
}