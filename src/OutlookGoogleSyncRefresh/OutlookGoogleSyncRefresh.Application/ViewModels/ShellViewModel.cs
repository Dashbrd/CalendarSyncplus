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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows.Threading;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.Views;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        #region Fields

        private readonly StringBuilder _statusBuilder;
        private DelegateCommand _authenticateGoogleAccount;
        private Appointment _currentAppointment;
        private DelegateCommand _downloadCommand;
        private DelegateCommand _exitCommand;
        private ObservableCollection<Appointment> _googleAppointments;
        private int _googleEntriesCount;
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
        private ObservableCollection<Appointment> _outlookAppointments;
        private int _outlookEntriesCount;
        private Settings _settings;
        private DelegateCommand _startSyncCommand;
        private DelegateCommand _syncNowCommand;

        #endregion

        #region Events

        public event CancelEventHandler Closing;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public ShellViewModel(IShellView view, IShellService shellService,
            ISyncService syncStartService,
            IAccountAuthenticationService accountAuthenticationService,
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
            _settings = settings;
            ApplicationLogger = applicationLogger;
            SystemTrayNotifierViewModel = systemTrayNotifierViewModel;
            _statusBuilder = new StringBuilder();
            view.Closing += ViewClosing;
            view.Closed += ViewClosed;
            //If no settings
            if (Settings == null)
            {
                IsSettingsVisible = true;
                LastSyncTime = null;
                NextSyncTime = null;
            }
            else
            {
                LastSyncTime = Settings.LastSuccessfulSync;
                NextSyncTime = null;
                if (Settings.RememberPeriodicSyncOn && Settings.PeriodicSyncOn)
                {
                    StartPeriodicSync();
                }
            }
        }

        #endregion

        #region Properties

        public IAccountAuthenticationService AccountAuthenticationService { get; private set; }
        public ISyncService SyncStartService { get; private set; }
        public ApplicationLogger ApplicationLogger { get; private set; }
        public SystemTrayNotifierViewModel SystemTrayNotifierViewModel { get; private set; }
        public IMessageService MessageService { get; set; }
        public IApplicationUpdateService ApplicationUpdateService { get; set; }
        public IShellService ShellService { get; set; }

        public ObservableCollection<Appointment> OutlookAppointments
        {
            get { return _outlookAppointments; }
            set { SetProperty(ref _outlookAppointments, value); }
        }

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
            get { return _startSyncCommand ?? (_startSyncCommand = new DelegateCommand(StartSync)); }
        }

        public DelegateCommand SyncNowCommand
        {
            get { return _syncNowCommand ?? (_syncNowCommand = new DelegateCommand(SyncNow)); }
        }

        public int OutlookEntriesCount
        {
            get { return _outlookEntriesCount; }
            set { SetProperty(ref _outlookEntriesCount, value); }
        }

        public int GoogleEntriesCount
        {
            get { return _googleEntriesCount; }
            set { SetProperty(ref _googleEntriesCount, value); }
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

        public ObservableCollection<Appointment> GoogleAppointments
        {
            get { return _googleAppointments; }
            set { SetProperty(ref _googleAppointments, value); }
        }

        public Appointment CurrentAppointment
        {
            get { return _currentAppointment; }
            set { SetProperty(ref _currentAppointment, value); }
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

        private async void StartSync()
        {
            await StartPeriodicSync();
            Settings.PeriodicSyncOn = IsPeriodicSyncStarted;
            if (IsPeriodicSyncStarted)
            {
                SyncNow();
            }
        }

        private async Task StartPeriodicSync()
        {
            if (IsPeriodicSyncStarted)
            {
                SyncStartService.Stop();
                IsPeriodicSyncStarted = false;
                ApplicationLogger.LogInfo("Periodic Sync Stopped");
            }
            else
            {
                bool result = await SyncStartService.Start(SyncTimerCallBack);
                if (result)
                {
                    if (_settings.SyncFrequency != null)
                    {
                        NextSyncTime = _settings.SyncFrequency.GetNextSyncTime();
                    }
                    ApplicationLogger.LogInfo("Periodic Sync Started");
                    IsPeriodicSyncStarted = true;
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
            InvokeOnCurrentDispatcher(() =>
            {
                _statusBuilder.AppendLine(text);
                RaisePropertyChanged("SyncLog");
            });
        }

        private void ShowNotification(bool showHide, string popupText = "Syncing...")
        {
            
                if (!Settings.HideSystemTrayTooltip)
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
                        //Ignore in this release
                        //ApplicationLogger.LogError(exception.Message);
                    }
                }
            
        }

        private void InvokeOnCurrentDispatcher(Action action)
        {
            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                Dispatcher.CurrentDispatcher.Invoke(action, DispatcherPriority.Normal);
            }
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

        public void Show()
        {
            ViewCore.Show();
        }

        public void Close()
        {
            ViewCore.Close();
        }

        private void SyncTimerCallBack(object state)
        {
            InvokeOnCurrentDispatcher(() =>
            {
                if (Settings != null)
                {
                    if (Settings.SyncFrequency.ValidateTimer(DateTime.Now))
                    {
                        SyncNow();
                    }
                }
            });
        }


        public async void SyncNow()
        {
            if (IsSyncInProgress)
            {
                return;
            }
            ShowNotification(true);
            IsSyncInProgress = true;
            bool result = await SyncStartService.SyncNowAsync(Settings);

            IsSyncInProgress = false;

            if (result)
            {
                LastSyncTime = DateTime.Now;
            }
            ShowNotification(false);
            if (Settings.SyncFrequency != null)
            {
                NextSyncTime = Settings.SyncFrequency.GetNextSyncTime();
            }

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
            InvokeOnCurrentDispatcher(() =>
            {
                if (Settings.CheckForUpdates)
                {
                    if (_lastCheckDateTime == null ||
                        DateTime.Now.Subtract(_lastCheckDateTime.GetValueOrDefault()).Hours > 24)
                    {
                        Task<string>.Factory.StartNew(() => ApplicationUpdateService.GetLatestReleaseFromServer())
                            .ContinueWith(UpdateContinuationAction);
                    }
                }
            });
        }

        #endregion
    }
}