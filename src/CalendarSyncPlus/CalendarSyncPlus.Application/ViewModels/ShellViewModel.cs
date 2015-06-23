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
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Interfaces;
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
            if (Closing != null)
            {
                Closing(this, e);
            }
        }

        #endregion

        #region Fields

        private readonly StringBuilder _statusBuilder;
        private DelegateCommand _downloadCommand;
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
        private DelegateCommand _launchAbout;
        private DelegateCommand _launchHelp;
        private DelegateCommand _launchSettings;
        private Settings _settings;
        private DelegateCommand _startSyncCommand;
        private DelegateCommand _syncNowCommand;
        private List<CalendarSyncProfile> _scheduledSyncProfiles;
        private ChildViewContentType _childContentViewType;
        private bool _showChildView;

        #endregion

        #region Properties

        public IGuiInteractionService GuiInteractionService { get; set; }
        public ISyncService SyncStartService { get; private set; }
        public ILog Logger { get; private set; }
        public ApplicationLogger ApplicationLogger { get; set; }
        public SystemTrayNotifierViewModel SystemTrayNotifierViewModel { get; private set; }
        public ChildContentViewFactory ChildContentViewFactory { get; set; }
        public IMessageService MessageService { get; set; }
        public IApplicationUpdateService ApplicationUpdateService { get; set; }
        public IShellService ShellService { get; set; }

        public bool IsSettingsVisible
        {
            get { return _isSettingsVisible; }
            set { SetProperty(ref _isSettingsVisible, value); }
        }

        public DelegateCommand LaunchSettings
        {
            get { return _launchSettings ?? (_launchSettings = new DelegateCommand(LaunchSettingsHandler)); }
        }

        public bool IsAboutVisible
        {
            get { return _isAboutVisible; }
            set { SetProperty(ref _isAboutVisible, value); }
        }

        public DelegateCommand LaunchAbout
        {
            get { return _launchAbout ?? (_launchAbout = new DelegateCommand(LaunchAboutHandler)); }
        }

        public bool IsHelpVisible
        {
            get { return _isHelpVisible; }
            set { SetProperty(ref _isHelpVisible, value); }
        }

        public DelegateCommand LaunchHelp
        {
            get { return _launchHelp ?? (_launchHelp = new DelegateCommand(LaunchHelpHandler)); }
        }

        public DelegateCommand StartSyncCommand
        {
            get { return _startSyncCommand ?? (_startSyncCommand = new DelegateCommand(PeriodicSyncCommandHandler)); }
        }

        public DelegateCommand SyncNowCommand
        {
            get { return _syncNowCommand ?? (_syncNowCommand = new DelegateCommand(SyncNowHandler)); }
        }

        public DelegateCommand DeleteLogFileCommand
        {
            get { return _deleteLogFileCommand ?? (_deleteLogFileCommand = new DelegateCommand(DeleteLogFile)); }
        }

        public string SyncLog
        {
            get { return _statusBuilder.ToString(); }
        }

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
                if (_settings != null)
                {
                    ScheduledSyncProfiles = Settings.SyncProfiles.Where(t => t.IsSyncEnabled).ToList();
                    if (!IsPeriodicSyncStarted)
                    {
                        if ((Settings.AppSettings.IsManualSynchronization && Settings.AppSettings.PeriodicSyncOn) ||
                            !Settings.AppSettings.IsManualSynchronization)
                        {
                            StartPeriodicSync();
                        }
                    }
                }
            }
        }


        public DelegateCommand DownloadCommand
        {
            get
            {
                return _downloadCommand ??
                       (_downloadCommand = new DelegateCommand(Download));
            }
        }

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

        public List<CalendarSyncProfile> ScheduledSyncProfiles
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

        public DelegateCommand ShowWhatsNewCommand
        {
            get
            {
                return _showWhatsNewCommand ??
                       (_showWhatsNewCommand = new DelegateCommand(ShowWhatsNew));
            }
        }

        public DelegateCommand ClearLogCommand
        {
            get
            {
                return _clearLogCommand ??
                       (_clearLogCommand = new DelegateCommand(ClearLog));
            }
        }

        private void ClearLog()
        {
            _statusBuilder.Clear();
            RaisePropertyChanged("SyncLog");
        }

        private void ShowWhatsNew()
        {
            var contentView = ChildContentViewFactory.GetChildContentViewModel(ChildViewContentType.WhatsNew);
            ViewCore.ShowChildWindow(contentView.View);
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


        private void LaunchSettingsHandler()
        {
            IsAboutVisible = false;
            IsHelpVisible = false;
            IsSettingsVisible = true;
        }

        private void LaunchAboutHandler()
        {
            IsSettingsVisible = false;
            IsHelpVisible = false;
            IsAboutVisible = true;
        }

        private void LaunchHelpHandler()
        {
            IsSettingsVisible = false;
            IsAboutVisible = false;
            IsHelpVisible = true;
        }

        private void DeleteLogFile()
        {
            try
            {
                MessageService.ShowMessageAsync("Application log file deleted.");
            }
            catch (Exception ex)
            {
                MessageService.ShowMessageAsync("Error occurred in deleting application log file.");
                Logger.Error(ex);
            }
        }

        private async void PeriodicSyncCommandHandler()
        {
            if (IsSettingsLoading)
            {
                MessageService.ShowMessageAsync("Unable to do the operation as settings are loading.");
                return;
            }

            if (IsSyncInProgress)
            {
                MessageService.ShowMessageAsync("Unable to do the operation as sync is in progress.");
                return;
            }
            await StartPeriodicSync();
            Settings.AppSettings.PeriodicSyncOn = IsPeriodicSyncStarted;
            if (IsPeriodicSyncStarted)
            {
                SyncNowHandler();
            }
        }

        private async Task StartPeriodicSync()
        {
            if (IsPeriodicSyncStarted)
            {
                SyncStartService.Stop(OnTimerElapsed);
                IsPeriodicSyncStarted = false;

                UpdateStatus(string.Format("Periodic Sync Stopped : {0}", DateTime.Now));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
            }
            else
            {
                var result = await SyncStartService.Start(OnTimerElapsed);
                if (result)
                {
                    foreach (var syncProfile in Settings.SyncProfiles)
                    {
                        if (syncProfile.IsSyncEnabled && syncProfile.SyncSettings.SyncFrequency != null)
                        {
                            syncProfile.NextSync = syncProfile.SyncSettings.SyncFrequency.GetNextSyncTime(DateTime.Now);
                        }
                    }

                    IsPeriodicSyncStarted = true;
                    UpdateStatus(string.Format("Periodic Sync Started : {0}", DateTime.Now));
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
                                string.Format("New Update {0} Available!", version), 5000);
                        }
                    }
                    _lastCheckDateTime = DateTime.Now;
                }
            });
        }

        private void Download(object o)
        {
            try
            {
                Process.Start(new ProcessStartInfo(ApplicationUpdateService.GetDownloadUri().AbsoluteUri));
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
                _statusBuilder.AppendLine(text);
                RaisePropertyChanged("SyncLog");
            });
            Logger.Info(text);
        }

        private void ShowNotification(bool showHide, string popupText = "Syncing...")
        {
            if (!Settings.AppSettings.HideSystemTrayTooltip)
            {
                try
                {
                    if (showHide)
                    {
                        SystemTrayNotifierViewModel.ShowBalloon(popupText);
                    }
                    else
                    {
                        SystemTrayNotifierViewModel.HideBalloon();
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception);
                }
            }
        }

        private void UpdateNotification(string popupText)
        {
            if (!Settings.AppSettings.HideSystemTrayTooltip)
            {
                try
                {
                    SystemTrayNotifierViewModel.UpdateBalloonText(popupText);
                }
                catch (Exception exception)
                {
                    Logger.Error("Updating status in balloon", exception);
                }
            }
        }

        private void BeginInvokeOnCurrentDispatcher(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }

        private static readonly object lockerObject = new object();
        private DelegateCommand _showWhatsNewCommand;
        private DelegateCommand _clearLogCommand;
        private DelegateCommand _deleteLogFileCommand;
        private SyncSummary _syncSummary;

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            BeginInvokeOnCurrentDispatcher(() =>
            {
                if (Settings != null)
                {
                    SyncPeriodicHandler(e.SignalTime);
                }
            });
        }

        private void SyncNowHandler()
        {
            try
            {
                if (IsSyncInProgress)
                {
                    MessageService.ShowMessageAsync("Unable to do the operation as sync is in progress.");
                    return;
                }

                foreach (var syncProfile in ScheduledSyncProfiles)
                {
                    var profile = syncProfile;
                    Task.Factory.StartNew(() => StartSyncTask(profile));
                    Task.Delay(1000);
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
        }


        /// <summary>
        /// </summary>
        /// <param name="signalTime"></param>
        private void SyncPeriodicHandler(DateTime signalTime)
        {
            try
            {
                foreach (var syncProfile in ScheduledSyncProfiles)
                {
                    var nextSyncTime = syncProfile.NextSync.GetValueOrDefault();
                    if (
                        nextSyncTime.ToString(Constants.CompareStringFormat)
                            .Equals(signalTime.ToString(Constants.CompareStringFormat)))
                    {
                        var profile = syncProfile;
                        Task.Factory.StartNew(() => StartSyncTask(profile), TaskCreationOptions.None);
                    }
                    else if (nextSyncTime.CompareTo(signalTime) < 0)
                    {
                        if (!IsSyncInProgress && (signalTime - nextSyncTime).TotalMinutes > 1)
                        {
                            var profile = syncProfile;
                            Task.Factory.StartNew(() => StartSyncTask(profile), TaskCreationOptions.None);
                        }
                    }
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
        }

        private void StartSyncTask(CalendarSyncProfile syncProfile)
        {
            lock (lockerObject)
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
                var result = SyncStartService.SyncNow(syncProfile, SyncCallback);
                OnSyncCompleted(syncProfile, result);
            }
        }


        private async Task<bool> SyncCallback(SyncEventArgs e)
        {
            var task = await MessageService.ShowConfirmMessage(e.Message);
            if (task != MessageDialogResult.Affirmative)
            {
                return false;
            }
            return true;
        }

        private void OnSyncCompleted(CalendarSyncProfile syncProfile, string result)
        {
            SyncSummary.TotalSyncs++;
            if (string.IsNullOrEmpty(result))
            {
                SyncSummary.SuccessSyncs++;
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.SyncSuccess, DateTime.Now));
            }
            else
            {
                SyncSummary.FailedSyncs++;
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.SyncFailed, result));
            }
            int totalSeconds = (int)DateTime.Now.Subtract(syncProfile.LastSync.GetValueOrDefault()).TotalSeconds;
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
            UpdateStatus(string.Format("Time Elapsed : {0} s", totalSeconds));
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
            SyncSummary.TotalSyncSeconds += totalSeconds;
            ShowNotification(false);

            syncProfile.NextSync = syncProfile.SyncSettings.SyncFrequency.GetNextSyncTime(
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
                ShowOnLaunch();
            }
        }

        private void ShowOnLaunch()
        {
            ShowWhatsNewOnStartup();
        }

        private void ShowWhatsNewOnStartup()
        {
            var calendarSyncPlusKey = @"Software\Ankesh Dave & Akanksha Gaur\CalendarSyncPlus";

            var key = Registry.CurrentUser.OpenSubKey(calendarSyncPlusKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
            try
            {
                if (key != null)
                {
                    var value = (int)key.GetValue("FirstLaunch", 0);
                    if (value == 1)
                    {
                        ShowWhatsNew();
                        key.SetValue("FirstLaunch", 0, RegistryValueKind.DWord);
                    }
                }
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
            {
                UpdateStatus(message);
            }
        }


        public void CheckForUpdates()
        {
            if (Settings.AppSettings.CheckForUpdates)
            {
                if (_lastCheckDateTime == null ||
                    DateTime.Now.Subtract(_lastCheckDateTime.GetValueOrDefault()).TotalHours > 6)
                {
                    Task<string>.Factory.StartNew(
                        () =>
                            ApplicationUpdateService.GetLatestReleaseFromServer(
                                Settings.AppSettings.CheckForAlphaReleases))
                        .ContinueWith(UpdateContinuationAction);
                }
            }
        }

        public void Shutdown()
        {
            if (IsPeriodicSyncStarted)
            {
                SyncStartService.Stop(OnTimerElapsed);
            }
        }

        #endregion
    }
}