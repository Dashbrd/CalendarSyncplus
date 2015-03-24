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
//  *      Modified On:    05-02-2015 1:34 PM
//  *      FileName:       ShellViewModel.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Waf.Applications;
using System.Windows.Documents;
using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Office.Interop.Outlook;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.Utilities;
using OutlookGoogleSyncRefresh.Application.Views;
using OutlookGoogleSyncRefresh.Common;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Helpers;
using OutlookGoogleSyncRefresh.Domain.Models;
using Action = System.Action;
using Exception = System.Exception;

#endregion

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        #region Fields

        private readonly StringBuilder _statusBuilder;
        private DelegateCommand _authenticateGoogleAccount;
        private DelegateCommand _downloadCommand;
        private DelegateCommand _exitCommand;
        private bool _isAboutVisible;
        private bool _isHelpVisible;
        private bool _isLatestVersionAvailable;
        private bool _isPeriodicSyncStarted;
        private bool _isSettingsVisible;
        private bool _isSyncInProgress;
        private DateTime? _lastCheckDateTime;
        private DateTime? _lastSyncTime;
        private string _latestVersion;
        private DelegateCommand _launchAbout;
        private DelegateCommand _launchHelp;
        private DelegateCommand _launchSettings;
        private DateTime? _nextSyncTime;
        private Settings _settings;
        private DelegateCommand _startSyncCommand;
        private DelegateCommand _syncNowCommand;
        private bool _isSettingsLoading;

        #endregion

        #region Events

        public event CancelEventHandler Closing;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public ShellViewModel(IShellView view, IShellService shellService,
            ISyncService syncStartService,
            IAccountAuthenticationService accountAuthenticationService,
            IGuiInteractionService guiInteractionService,
            Settings settings,
            IMessageService messageService,
            ApplicationLogger applicationLogger, IApplicationUpdateService applicationUpdateService,
            SystemTrayNotifierViewModel systemTrayNotifierViewModel)
            : base(view)
        {
            MessageService = messageService;
            ApplicationUpdateService = applicationUpdateService;
            ShellService = shellService;
            SyncStartService = syncStartService;
            AccountAuthenticationService = accountAuthenticationService;
            GuiInteractionService = guiInteractionService;
            _settings = settings;
            ApplicationLogger = applicationLogger;
            SystemTrayNotifierViewModel = systemTrayNotifierViewModel;
            _statusBuilder = new StringBuilder();
            view.Closing += ViewClosing;
            view.Closed += ViewClosed;
            //If no settings
            if (!Settings.ValidateSettings())
            {
                IsSettingsVisible = true;
                LastSyncTime = null;
                NextSyncTime = null;
            }
            else
            {
                LastSyncTime = Settings.LastSuccessfulSync;
                NextSyncTime = null;
                if (Settings.AppSettings.RememberPeriodicSyncOn && Settings.AppSettings.PeriodicSyncOn)
                {
                    StartPeriodicSync();
                }
            }
        }

        #endregion

        #region Properties

        public IAccountAuthenticationService AccountAuthenticationService { get; private set; }
        public IGuiInteractionService GuiInteractionService { get; set; }
        public ISyncService SyncStartService { get; private set; }
        public ApplicationLogger ApplicationLogger { get; private set; }
        public SystemTrayNotifierViewModel SystemTrayNotifierViewModel { get; private set; }
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


        public DelegateCommand AuthenticateGoogleAccount
        {
            get
            {
                return _authenticateGoogleAccount ??
                       (_authenticateGoogleAccount = new DelegateCommand(AuthenticateGoogle));
            }
        }

        public Settings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

        public DateTime? NextSyncTime
        {
            get { return _nextSyncTime; }
            set { SetProperty(ref _nextSyncTime, value); }
        }

        public DateTime? LastSyncTime
        {
            get { return _lastSyncTime; }
            set { SetProperty(ref _lastSyncTime, value); }
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

        private void AuthenticateGoogle()
        {
            AccountAuthenticationService.AuthenticateCalenderOauth();
        }

        private void LaunchSettingsHandler()
        {
            IsSettingsVisible = true;
        }

        private void LaunchAboutHandler()
        {
            IsAboutVisible = true;
        }

        private void LaunchHelpHandler()
        {
            IsHelpVisible = true;
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

                UpdateStatus(string.Format("Period Sync Stopped : {0}", DateTime.Now));
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
            }
            else
            {
                bool result = await SyncStartService.Start(OnTimerElapsed);
                if (result)
                {
                    if (_settings.SyncFrequency != null)
                    {
                        NextSyncTime = _settings.SyncFrequency.GetNextSyncTime(DateTime.Now);
                    }
                    IsPeriodicSyncStarted = true;
                    UpdateStatus(string.Format("Period Sync Started : {0}", DateTime.Now));
                    UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
                }
            }
        }

        private void UpdateContinuationAction(Task<string> task)
        {
            if (task.Result == null)
            {
                if (ApplicationUpdateService.IsNewVersionAvailable())
                {
                    LatestVersion = ApplicationUpdateService.GetNewAvailableVersion();
                    IsLatestVersionAvailable = true;
                    SystemTrayNotifierViewModel.ShowBalloon("New Update Available!", 5000);
                }
                _lastCheckDateTime = DateTime.Now;
            }
        }

        private void Download(object o)
        {
            Process.Start(new ProcessStartInfo(ApplicationUpdateService.GetDownloadUri().AbsoluteUri));
        }

        private void UpdateStatus(string text)
        {
            BeginInvokeOnCurrentDispatcher(() =>
            {
                _statusBuilder.AppendLine(text);
                ApplicationLogger.LogInfo(text);
                RaisePropertyChanged("SyncLog");
            });
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
                    ApplicationLogger.LogError(exception.Message);
                }
            }

        }

        private void BeginInvokeOnCurrentDispatcher(Action action)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(action);
        }

        private T InvokeOnCurrentDispatcher<T>(Func<T> action)
        {
            return DispatcherHelper.CheckInvokeOnUI(action);
        }

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
            }
        }

        public void Close()
        {
            ViewCore.Close();
        }


        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            BeginInvokeOnCurrentDispatcher(() =>
            {
                if (Settings != null)
                {
                    if (Settings.SyncFrequency.ValidateTimer(DateTime.Now))
                    {
                        SyncNowHandler();
                    }
                }
            });
        }


        private TaskFactory _taskFactory = null;

        void SyncNowHandler()
        {
            try
            {
                if (_taskFactory == null)
                {
                    var taskScheduler = new LimitedConcurrencyLevelTaskScheduler(1);
                    _taskFactory = new TaskFactory(taskScheduler);
                }

                foreach (var syncProfile in Settings.SyncProfiles)
                {
                    SyncProfile profile = syncProfile;
                    _taskFactory.StartNew(() => StartSyncTask(profile));
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
        }

        private void StartSyncTask(SyncProfile syncProfile)
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
            IsSyncInProgress = true;
            IsSettingsVisible = false;
            LastSyncTime = DateTime.Now;
            ShowNotification(true);
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.SyncStarted, LastSyncTime));
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
            var result = SyncStartService.SyncNow(syncProfile, SyncCallback);
            OnSyncCompleted(result);

        }


        private async Task<bool> SyncCallback(SyncEventArgs e)
        {
            return await InvokeOnCurrentDispatcher(async () =>
            {
                var task = await MessageService.ShowConfirmMessage(e.Message);
                if (task != MessageDialogResult.Affirmative)
                {
                    return false;
                }
                return true;
            });
        }

        private void OnSyncCompleted(string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.SyncSuccess, DateTime.Now));
            }
            else
            {
                UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.SyncFailed, result));
            }
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.Line));
            UpdateStatus(string.Format("Time Elapsed : {0} s", (int)DateTime.Now.Subtract(LastSyncTime.GetValueOrDefault()).TotalSeconds));
            UpdateStatus(StatusHelper.GetMessage(SyncStateEnum.LogSeparator));
            ShowNotification(false);
            if (Settings.SyncFrequency != null)
            {
                NextSyncTime = Settings.SyncFrequency.GetNextSyncTime(LastSyncTime.GetValueOrDefault());
            }
            IsSyncInProgress = false;
            CheckForUpdates();
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
            BeginInvokeOnCurrentDispatcher(() =>
            {
                if (Settings.AppSettings.CheckForUpdates)
                {
                    if (!IsLatestVersionAvailable && _lastCheckDateTime == null &&
                        DateTime.Now.Subtract(_lastCheckDateTime.GetValueOrDefault()).TotalHours > 24)
                    {
                        Task<string>.Factory.StartNew(() => ApplicationUpdateService.GetLatestReleaseFromServer())
                            .ContinueWith(UpdateContinuationAction);
                    }
                }
            });
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