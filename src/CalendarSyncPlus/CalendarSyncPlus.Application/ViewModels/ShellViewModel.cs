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
//  *      Created On:     02-02-2015 11:15 AM
//  *      Modified On:    05-02-2015 1:34 PM
//  *      FileName:       ShellViewModel.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Sync.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

#endregion

namespace CalendarSyncPlus.Application.ViewModels
{
    /// <summary>
    /// </summary>
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        #region Constructors

        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="shellService"></param>
        /// <param name="syncStartService"></param>
        /// <param name="guiInteractionService"></param>
        /// <param name="settings"></param>
        /// <param name="messageService"></param>
        /// <param name="applicationLogger"></param>
        /// <param name="applicationUpdateService"></param>
        /// <param name="systemTrayNotifierViewModel"></param>
        /// <param name="childContentViewFactory"></param>
        [ImportingConstructor]
        public ShellViewModel(IShellView view, IShellService shellService,
            ISyncService syncStartService,
            IGuiInteractionService guiInteractionService,
            Settings settings,
            SyncSummary syncSummary,
            IMessageService messageService,
            ApplicationLogger applicationLogger, IApplicationUpdateService applicationUpdateService,
            SystemTrayNotifierViewModel systemTrayNotifierViewModel, ChildContentViewFactory childContentViewFactory)
            : base(view)
        {
            _statusBuilder = new StringBuilder();
            MessageService = messageService;
            ApplicationLogger = applicationLogger;
            Logger = applicationLogger.GetLogger(GetType());
            ApplicationUpdateService = applicationUpdateService;
            ShellService = shellService;
            SyncStartService = syncStartService;
            GuiInteractionService = guiInteractionService;
            Settings = settings;
            SyncSummary = syncSummary;
            SystemTrayNotifierViewModel = systemTrayNotifierViewModel;
            ChildContentViewFactory = childContentViewFactory;
            view.Closing += ViewClosing;
            view.Closed += ViewClosed;
        }

        #endregion

        #region Events

        public event CancelEventHandler Closing;

        #endregion

        #region Protected Methods

        protected virtual void OnClosing(CancelEventArgs e)
        {
            Closing?.Invoke(this, e);
        }

        #endregion

        #region Fields

        private readonly StringBuilder _statusBuilder;
        private AsyncDelegateCommand _downloadCommand;
        private DelegateCommand _exitCommand;
        private bool _isAboutVisible;
        private bool _isHelpVisible;
        private bool _isLatestVersionAvailable;
        private bool _isPeriodicSyncStarted;
        private bool _isSettingsLoading;
        private bool _isSettingsVisible;
        private bool _isSyncInProgress;
        private DateTime? _lastCheckDateTime;
        private string _latestVersion;
        private AsyncDelegateCommand _launchAbout;
        private AsyncDelegateCommand _launchHelp;
        private AsyncDelegateCommand _launchSettings;
        private Settings _settings;
        private AsyncDelegateCommand _startSyncCommand;
        private AsyncDelegateCommand _syncNowCommand;
        private List<SyncProfile> _scheduledSyncProfiles;
        private ChildViewContentType _childContentViewType;
        private bool _showChildView;

        #endregion

        #region Properties

        public IGuiInteractionService GuiInteractionService { get; set; }
        public ISyncService SyncStartService { get; }
        public ILog Logger { get; }
        public ApplicationLogger ApplicationLogger { get; set; }
        public SystemTrayNotifierViewModel SystemTrayNotifierViewModel { get; }
        public ChildContentViewFactory ChildContentViewFactory { get; set; }
        public IMessageService MessageService { get; set; }
        public IApplicationUpdateService ApplicationUpdateService { get; set; }
        public IShellService ShellService { get; set; }

        public bool IsSettingsVisible
        {
            get { return _isSettingsVisible; }
            set { SetProperty(ref _isSettingsVisible, value); }
        }

        public AsyncDelegateCommand LaunchSettings => _launchSettings ?? (_launchSettings = new AsyncDelegateCommand(LaunchSettingsHandler));

        public bool IsAboutVisible
        {
            get { return _isAboutVisible; }
            set { SetProperty(ref _isAboutVisible, value); }
        }

        public AsyncDelegateCommand LaunchAbout => _launchAbout ?? (_launchAbout = new AsyncDelegateCommand(LaunchAboutHandler));

        public bool IsHelpVisible
        {
            get { return _isHelpVisible; }
            set { SetProperty(ref _isHelpVisible, value); }
        }

        public AsyncDelegateCommand LaunchHelp => _launchHelp ?? (_launchHelp = new AsyncDelegateCommand(LaunchHelpHandler));

        public AsyncDelegateCommand StartSyncCommand => _startSyncCommand ?? (_startSyncCommand = new AsyncDelegateCommand(PeriodicSyncCommandHandler));

        public AsyncDelegateCommand SyncNowCommand => _syncNowCommand ?? (_syncNowCommand = new AsyncDelegateCommand(SyncNowHandler));

        public AsyncDelegateCommand DeleteLogFileCommand => _deleteLogFileCommand ?? (_deleteLogFileCommand = new AsyncDelegateCommand(DeleteLogFile));

        public string SyncLog => _statusBuilder.ToString();

        public DelegateCommand ExitCommand
        {
            get { return _exitCommand; }
            set { SetProperty(ref _exitCommand, value); }
        }

        public bool IsPeriodicSyncStarted
        {
            get { return _isPeriodicSyncStarted; }
            set { SetProperty(ref _isPeriodicSyncStarted, value); }
        }

        public bool IsSettingsLoading
        {
            get { return _isSettingsLoading; }
            set { SetProperty(ref _isSettingsLoading, value); }
        }

        public bool IsSyncInProgress
        {
            get { return _isSyncInProgress; }
            set { SetProperty(ref _isSyncInProgress, value); }
        }

        public SyncSummary SyncSummary
        {
            get { return _syncSummary; }
            set { SetProperty(ref _syncSummary, value); }
        }

        public Settings Settings
        {
            get { return _settings; }
            set
            {
                SetProperty(ref _settings, value);
                if (_settings == null) return;
                ScheduledSyncProfiles = GetScheduledProfiles();
                if (IsPeriodicSyncStarted) return;
                if (Settings.AppSettings.IsManualSynchronization && Settings.AppSettings.PeriodicSyncOn ||
                    !Settings.AppSettings.IsManualSynchronization)
                    Task.Run(StartPeriodicSync);
            }
        }

        public AsyncDelegateCommand DownloadCommand => _downloadCommand ?? (_downloadCommand = new AsyncDelegateCommand(Download))
        ;

        public bool IsLatestVersionAvailable
        {
            get { return _isLatestVersionAvailable; }
            set { SetProperty(ref _isLatestVersionAvailable, value); }
        }

        public string LatestVersion
        {
            get { return _latestVersion; }
            set { SetProperty(ref _latestVersion, value); }
        }

        public List<SyncProfile> ScheduledSyncProfiles
        {
            get { return _scheduledSyncProfiles; }
            set { SetProperty(ref _scheduledSyncProfiles, value); }
        }

        public ChildViewContentType ChildContentViewType
        {
            get { return _childContentViewType; }
            set { SetProperty(ref _childContentViewType, value); }
        }

        public bool ShowChildView
        {
            get { return _showChildView; }
            set { SetProperty(ref _showChildView, value); }
        }

        public AsyncDelegateCommand ShowWhatsNewCommand => _showWhatsNewCommand ??
                                                      (_showWhatsNewCommand = new AsyncDelegateCommand(ShowWhatsNew));

        public AsyncDelegateCommand ClearLogCommand => _clearLogCommand ??
                                                  (_clearLogCommand = new AsyncDelegateCommand(ClearLog));

        private async Task ClearLog()
        {
            await Task.Run(() =>
            {
                _statusBuilder.Clear();
                RaisePropertyChanged(propertyName: "SyncLog");
            });
        }

        private async Task ShowWhatsNew()
        {
            await Task.Run(() =>
            {
                Common.DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    var contentView = ChildContentViewFactory.GetChildContentViewModel(ChildViewContentType.WhatsNew);
                    ViewCore.ShowChildWindow(contentView.View);
                });
            });
        }

        #endregion

        #region Private Methods

        private void ViewClosed(object sender, EventArgs e)
        {
            //Save Some Settings if required.
        }

        private void ViewClosing(object sender, CancelEventArgs e)
        {
            OnClosing(e);
        }


        private async Task LaunchSettingsHandler()
        {
            await Task.Run(() =>
            {
                IsAboutVisible = false;
                IsHelpVisible = false;
                IsSettingsVisible = true;
            });
        }

        private async Task LaunchAboutHandler()
        {
            await Task.Run(() =>
            {
                IsSettingsVisible = false;
                IsHelpVisible = false;
                IsAboutVisible = true;
            });
        }

        private async  Task LaunchHelpHandler()
        {
            await Task.Run(() =>
            {
                IsSettingsVisible = false;
                IsAboutVisible = false;
                IsHelpVisible = true;
            });
        }

        private List<SyncProfile> GetScheduledProfiles()
        {
            var profiles = Settings.CalendarSyncProfiles.Where(syncProfile => syncProfile.IsSyncEnabled).Cast<SyncProfile>().ToList();
            profiles.AddRange(Settings.TaskSyncProfiles.Where(syncProfile => syncProfile.IsSyncEnabled));
            return profiles;
        }

        private async Task DeleteLogFile()
        {
            try
            {
                MessageService.ShowMessage("Application log file deleted.");
            }
            catch (Exception ex)
            {
                MessageService.ShowMessage("Error occurred in deleting application log file.");
                Logger.Error(ex);
            }
        }

        private async Task PeriodicSyncCommandHandler()
        {
            if (IsSettingsLoading)
            {
                MessageService.ShowMessage("Unable to do the operation as settings are loading.");
                return;
            }

            if (IsSyncInProgress)
            {
                MessageService.ShowMessage("Unable to do the operation as sync is in progress.");
                return;
            }
            await StartPeriodicSync();
            Settings.AppSettings.PeriodicSyncOn = IsPeriodicSyncStarted;
            if (IsPeriodicSyncStarted)
            {
                await SyncNowHandler();
            }
        }

        private async Task StartPeriodicSync()
        {
            if (IsPeriodicSyncStarted)
            {
                SyncStartService.Stop(OnTimerElapsed);
                IsPeriodicSyncStarted = false;

                UpdateStatus($"Periodic Sync Stopped : {DateTime.Now}");
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
            }
            else
            {
                var result = await SyncStartService.Start(OnTimerElapsed);
                if (result)
                {
                    foreach (var syncProfile in ScheduledSyncProfiles)
                        if (syncProfile.IsSyncEnabled && syncProfile.SyncFrequency != null)
                            syncProfile.NextSync = syncProfile.SyncFrequency.GetNextSyncTime(DateTime.Now);

                    IsPeriodicSyncStarted = true;
                    UpdateStatus($"Periodic Sync Started : {DateTime.Now}");
                    UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
                }
            }
        }

        private void UpdateContinuationAction(Task<string> task)
        {
            BeginInvokeOnCurrentDispatcher(() =>
            {
                if (task.Result == null)
                {
                    if (ApplicationUpdateService.IsNewVersionAvailable())
                    {
                        var version = ApplicationUpdateService.GetNewAvailableVersion();
                        if (string.IsNullOrEmpty(LatestVersion) || !version.Equals(LatestVersion))
                        {
                            IsLatestVersionAvailable = true;
                            LatestVersion = version;
                            SystemTrayNotifierViewModel.ShowBalloon(
                                $"New Update {version} Available!", 5000);
                        }
                    }
                    _lastCheckDateTime = DateTime.Now;
                }
            });
        }

        private async Task Download(object o)
        {
            try
            {
                await Task.Run(() =>
                {
                    Process.Start(new ProcessStartInfo(ApplicationUpdateService.GetDownloadUri().AbsoluteUri));
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }

        private void UpdateStatus(string text)
        {
            BeginInvokeOnCurrentDispatcher(() =>
            {
                if (IsSyncInProgress && !text.Equals(StatusHelper.LineConstant))
                {
                    UpdateNotification(text);
                }

                if (_statusBuilder.Length >= 1000)
                {
                    _statusBuilder.Clear();
                    _statusBuilder.AppendLine("Flushed old logs");
                    _statusBuilder.AppendLine(StatusHelper.LineConstant);
                }
                _statusBuilder.AppendLine(text);
                
                RaisePropertyChanged("SyncLog");
            });
            Logger.Info(text);
        }

        private void ShowNotification(bool showHide, string popupText = "Syncing...")
        {
            if (!Settings.AppSettings.HideSystemTrayTooltip)
                try
                {
                    if (showHide)
                        SystemTrayNotifierViewModel.ShowBalloon(popupText);
                    else
                        SystemTrayNotifierViewModel.HideBalloon();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
        }

        private void UpdateNotification(string popupText)
        {
            if (!Settings.AppSettings.HideSystemTrayTooltip)
                try
                {
                    SystemTrayNotifierViewModel.UpdateBalloonText(popupText);
                }
                catch (Exception exception)
                {
                    Logger.Error("Updating status in balloon", exception);
                }
        }

        private void BeginInvokeOnCurrentDispatcher(Action action)
        {
            Common.DispatcherHelper.CheckBeginInvokeOnUI(action);
        }

        private static readonly object LockerObject = new object();
        private AsyncDelegateCommand _showWhatsNewCommand;
        private AsyncDelegateCommand _clearLogCommand;
        private AsyncDelegateCommand _deleteLogFileCommand;
        private SyncSummary _syncSummary;

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            BeginInvokeOnCurrentDispatcher(async () =>
            {
                if (Settings != null)
                    await SyncPeriodicHandler(e.SignalTime);
            });
        }

        private async Task SyncNowHandler()
        {
            try
            {
                if (IsSyncInProgress)
                {
                    MessageService.ShowMessage("Unable to do the operation as sync is in progress.");
                    return;
                }

                foreach (var syncProfile in Settings.CalendarSyncProfiles.Where(t => t.IsSyncEnabled))
                {
                    var profile = syncProfile;
                    await StartCalendarSyncProfile(profile);
                }

                foreach (var syncProfile in Settings.TaskSyncProfiles.Where(t => t.IsSyncEnabled))
                {
                    var profile = syncProfile;
                    await StartTaskSyncProfile(profile);
                }
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                MessageService.ShowMessage(flattenException.Message);
            }
            catch (Exception exception)
            {
                MessageService.ShowMessage(exception.Message);
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="signalTime"></param>
        private async Task SyncPeriodicHandler(DateTime signalTime)
        {
            try
            {
                foreach (var syncProfile in Settings.CalendarSyncProfiles.Where(t => t.IsSyncEnabled))
                {
                    var nextSyncTime = syncProfile.NextSync.GetValueOrDefault();
                    if (nextSyncTime.ToString(Constants.CompareStringFormat)
                        .Equals(signalTime.ToString(Constants.CompareStringFormat)))
                    {
                        var profile = syncProfile;
                        await StartCalendarSyncProfile(profile);
                    }
                    else if (nextSyncTime.CompareTo(signalTime) < 0)
                    {
                        if (IsSyncInProgress || !((signalTime - nextSyncTime).TotalMinutes > 1)) continue;
                        var profile = syncProfile;
                        await StartCalendarSyncProfile(profile);
                    }
                }

                foreach (var syncProfile in Settings.TaskSyncProfiles.Where(t => t.IsSyncEnabled))
                {
                    var nextSyncTime = syncProfile.NextSync.GetValueOrDefault();
                    if (nextSyncTime.ToString(Constants.CompareStringFormat)
                        .Equals(signalTime.ToString(Constants.CompareStringFormat)))
                    {
                        var profile = syncProfile;
                        await StartTaskSyncProfile(profile);
                    }
                    else if (nextSyncTime.CompareTo(signalTime) < 0)
                    {
                        if (IsSyncInProgress || !((signalTime - nextSyncTime).TotalMinutes > 1)) continue;
                        var profile = syncProfile;
                        await StartTaskSyncProfile(profile);
                    }
                }
            }
            catch (AggregateException exception)
            {
                var flattenException = exception.Flatten();
                MessageService.ShowMessage(flattenException.Message);
            }
            catch (Exception exception)
            {
                MessageService.ShowMessage(exception.Message);
            }
        }

        private async Task StartCalendarSyncProfile(CalendarSyncProfile profile)
        {
            await Task.Run(() => StartCalendarSyncTask(profile));
        }

        private async Task StartTaskSyncProfile(TaskSyncProfile profile)
        {
            await Task.Run(() => StartTaskSyncTask(profile));
        }

        private void StartCalendarSyncTask(CalendarSyncProfile syncProfile)
        {
            lock (LockerObject)
            {
                if (IsSettingsLoading)
                {
                    MessageService.ShowMessageAsync("Unable to do the operation as settings are loading.");
                    return;
                }

                IsSyncInProgress = true;
                IsSettingsVisible = false;
                syncProfile.LastSync = DateTime.Now;
                ShowNotification(true);
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.SyncStarted, syncProfile.LastSync));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Profile, syncProfile.Name));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
                var syncMetric = new SyncMetric
                {
                    StartTime = syncProfile.LastSync.GetValueOrDefault(),
                    ProfileName = syncProfile.Name,
                    CalendarSyncDirection = syncProfile.SyncDirection.ToString()
                };
                SyncSummary.SyncMetrics.Add(syncMetric);
                var result = SyncStartService.SyncNow(syncProfile, syncMetric, SyncCallback);
                OnSyncCompleted(syncProfile, syncMetric, result);
            }
        }

        private void StartTaskSyncTask(TaskSyncProfile syncProfile)
        {
            lock (LockerObject)
            {
                if (IsSettingsLoading)
                {
                    MessageService.ShowMessageAsync("Unable to do the operation as settings are loading.");
                    return;
                }

                IsSyncInProgress = true;
                IsSettingsVisible = false;
                syncProfile.LastSync = DateTime.Now;
                ShowNotification(true);
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.SyncStarted, syncProfile.LastSync));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Profile, syncProfile.Name));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
                var syncMetric = new SyncMetric
                {
                    StartTime = syncProfile.LastSync.GetValueOrDefault(),
                    ProfileName = syncProfile.Name,
                    CalendarSyncDirection = syncProfile.SyncDirection.ToString()
                };
                SyncSummary.SyncMetrics.Add(syncMetric);
                var result = SyncStartService.SyncNow(syncProfile, syncMetric, SyncCallback);
                OnSyncCompleted(syncProfile, syncMetric, result);
            }
        }

        private async Task<bool> SyncCallback(SyncEventArgs e)
        {
            var messageDialogResult = await MessageService.ShowConfirmMessage(e.Message);
            return messageDialogResult == MessageDialogResult.Affirmative;
        }

        private void OnSyncCompleted(SyncProfile syncProfile, SyncMetric syncMetric, string result)
        {
            UpdateStatus(string.IsNullOrEmpty(result)
                ? StatusHelper.GetMessage(SyncStateEnum.SyncSuccess, DateTime.Now)
                : StatusHelper.GetMessage(SyncStateEnum.SyncFailed, result));
            var totalSeconds = (int) DateTime.Now.Subtract(syncProfile.LastSync.GetValueOrDefault()).TotalSeconds;
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
            UpdateStatus($"Time Elapsed : {totalSeconds} s");
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
            syncMetric.ElapsedSeconds = totalSeconds;
            ShowNotification(false);

            syncProfile.NextSync = syncProfile.SyncFrequency.GetNextSyncTime(
                DateTime.Now);

            IsSyncInProgress = false;
            CheckForUpdates();
        }

        #endregion

        #region Public Methods

        public void Show(bool startMinimized)
        {
            if (startMinimized)
            {
                GuiInteractionService.HideApplication();
            }
            else
            {
                ViewCore.Show();
                Task.Run(async () => await ShowOnLaunch());
            }
        }

        private async Task ShowOnLaunch()
        {
            await ShowWhatsNewOnStartup();
        }

        private async Task ShowWhatsNewOnStartup()
        {
            var calendarSyncPlusKey = @"Software\Ankesh Dave & Akanksha Gaur\CalendarSyncPlus";

            var key = Registry.CurrentUser.OpenSubKey(calendarSyncPlusKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
            try
            {
                var value = (int?) key?.GetValue("FirstLaunch", 0);
                if (value != 1) return;
                await ShowWhatsNew();
                key.SetValue("FirstLaunch", 0, RegistryValueKind.DWord);
            }
            catch (Exception exception)
            {
                Logger.Error("First Launch Key Not found", exception);
            }
        }

        public void Close()
        {
            ViewCore.Close();
        }


        public void ErrorMessageChanged(string message)
        {
            if (!string.IsNullOrEmpty(message))
                UpdateStatus(message);
        }


        public void CheckForUpdates()
        {
            if (!Settings.AppSettings.CheckForUpdates) return;
            if (_lastCheckDateTime == null ||
                DateTime.Now.Subtract(_lastCheckDateTime.GetValueOrDefault()).TotalHours > 6)
                Task<string>.Factory.StartNew(
                        () =>
                            ApplicationUpdateService.GetLatestReleaseFromServer(
                                Settings.AppSettings.CheckForAlphaReleases))
                    .ContinueWith(UpdateContinuationAction);
        }

        public void Shutdown()
        {
            if (IsPeriodicSyncStarted)
                SyncStartService.Stop(OnTimerElapsed);
        }

        #endregion
    }
}