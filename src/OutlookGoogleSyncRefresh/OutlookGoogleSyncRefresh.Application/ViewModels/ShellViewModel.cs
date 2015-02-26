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
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows.Input;
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
    public class ShellViewModel : Utilities.ViewModel<IShellView>
    {
        #region Fields

        private readonly IAccountAuthenticationService _accountAuthenticationService;
        private readonly IShellService _shellService;
        private readonly ISyncService _syncStartService;
        private Settings _settings;
        private readonly ApplicationLogger _applicationLogger;

        private DelegateCommand _authenticateGoogleAccount;
        private Appointment _currentAppointment;
        private DelegateCommand _exitCommand;
        private ObservableCollection<Appointment> _googleAppointments;
        private int _googleEntriesCount;
        private bool _isSettingsVisible;
        private bool _isHelpVisible;
        private bool _isAboutVisible;
        private bool _isPeriodicSyncStarted;
        private bool _isSyncInProgress;
        private DelegateCommand _launchSettings;
        private DelegateCommand _launchAbout;
        private DelegateCommand _launchHelp;
        private ObservableCollection<Appointment> _outlookAppointments;
        private int _outlookEntriesCount;
        private DelegateCommand _startSyncCommand;
        private string _syncLog;
        private DelegateCommand _syncNowCommand;
        private StringBuilder _statusBuilder;
        private DateTime? _lastSyncTime;
        private DateTime? _nextSyncTime;

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
            ApplicationLogger applicationLogger)
            : base(view)
        {
            MessageService = messageService;
            _shellService = shellService;
            _syncStartService = syncStartService;
            _accountAuthenticationService = accountAuthenticationService;
            _settings = settings;
            _applicationLogger = applicationLogger;
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
            }
        }

        #endregion

        #region Properties
        public IMessageService MessageService { get; set; }

        public ObservableCollection<Appointment> OutlookAppointments
        {
            get { return _outlookAppointments; }
            set
            {
                SetProperty(ref _outlookAppointments, value);
            }
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
            set
            {
                SetProperty(ref _isSyncInProgress, value);
            }
        }

        public IShellService ShellService
        {
            get { return _shellService; }
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
            set
            {
                SetProperty(ref _googleAppointments, value);
            }
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
            _accountAuthenticationService.AuthenticateCalenderOauth();
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
            if (IsPeriodicSyncStarted)
            {
                _syncStartService.Stop();
                IsPeriodicSyncStarted = false;
                _applicationLogger.LogInfo("Periodic Sync Stopped");
            }
            else
            {
                var result = await _syncStartService.Start();
                if (result)
                {
                    if (_settings.SyncFrequency != null)
                    {
                        NextSyncTime = _settings.SyncFrequency.GetNextSyncTime();
                    }
                    _applicationLogger.LogInfo("Periodic Sync Started");
                    IsPeriodicSyncStarted = true;
                    SyncNow();
                }
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

        public async void SyncNow()
        {
            await _syncStartService.SyncNowAsync(Settings);
        }

        void UpdateStatus(string text)
        {
            _statusBuilder.AppendLine(text);
            Dispatcher.CurrentDispatcher.BeginInvoke(((Action)(() => RaisePropertyChanged("SyncLog"))));
        }

        public void ErrorMessageChanged(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                UpdateStatus(message);
            }
        }
    }

        #endregion

}