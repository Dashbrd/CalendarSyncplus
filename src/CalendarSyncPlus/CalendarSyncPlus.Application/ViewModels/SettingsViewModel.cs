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
//  *      Modified On:    04-02-2015 12:40 PM
//  *      FileName:       SettingsViewModel.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Authentication.Google;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.ExchangeWebServices.Calendar;
using CalendarSyncPlus.GoogleServices.Calendar;
using CalendarSyncPlus.GoogleServices.Google;
using CalendarSyncPlus.OutlookServices.Calendar;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Sync.Interfaces;
using log4net;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class SettingsViewModel : ViewModel<ISettingsView>
    {
        
        #region Constructors
        [ImportingConstructor]
        public SettingsViewModel(ISettingsView view,
            IGoogleCalendarService googleCalendarService,
            Settings settings,
            ISettingsService settingsService,
            ISettingsSerializationService serializationService, IOutlookCalendarService outlookCalendarService,
            IMessageService messageService, IExchangeWebCalendarService exchangeWebCalendarService,
            ApplicationLogger applicationLogger, IWindowsStartupService windowsStartupService,
            IAccountAuthenticationService accountAuthenticationService)
            : base(view)
        {
            _lastSavedSettings = settings;
            Settings = settings.DeepClone();
            ExchangeWebCalendarService = exchangeWebCalendarService;
            ApplicationLogger = applicationLogger;
            Logger = applicationLogger.GetLogger(GetType());
            WindowsStartupService = windowsStartupService;
            AccountAuthenticationService = accountAuthenticationService;
            GoogleCalendarService = googleCalendarService;
            SettingsService = settingsService;
            SettingsSerializationService = serializationService;
            OutlookCalendarService = outlookCalendarService;
            MessageService = messageService;
        }
        
        #endregion

        #region Fields
        private Settings _lastSavedSettings;
        private bool _isLoading;
        private DelegateCommand _saveCommand;
        private Settings _settings;
        private bool _settingsSaved;
        private bool _isValid;
        private DelegateCommand _addNewGoogleAccount;
        private DelegateCommand _disconnectGoogleCommand;
        private bool _init;
        private DelegateCommand _cancelCommand;

        #endregion

        #region Properties

        public IGoogleCalendarService GoogleCalendarService { get; set; }
        public ISettingsService SettingsService { get; set; }
        public ISettingsSerializationService SettingsSerializationService { get; set; }
        public IOutlookCalendarService OutlookCalendarService { get; set; }
        public IMessageService MessageService { get; set; }
        public IExchangeWebCalendarService ExchangeWebCalendarService { get; private set; }
        public ILog Logger { get; private set; }
        public ApplicationLogger ApplicationLogger { get; set; }
        public IWindowsStartupService WindowsStartupService { get; set; }
        public IAccountAuthenticationService AccountAuthenticationService { get; set; }

        public DelegateCommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new DelegateCommand(CancelSettingsHandler)); }
        }

        public DelegateCommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new DelegateCommand(SaveSettings)); }
        }

        public DelegateCommand DisconnectGoogleCommand
        {
            get
            {
                return _disconnectGoogleCommand ??
                       (_disconnectGoogleCommand = new DelegateCommand(DisconnectGoogleHandler));
            }
        }

        public DelegateCommand AddNewGoogleAccount
        {
            get
            {
                return _addNewGoogleAccount = _addNewGoogleAccount ?? new DelegateCommand(AddNewGoogleAccountHandler);
            }
        }

        public Settings LastSavedSettings
        {
            get { return _lastSavedSettings; }
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

        public bool SettingsSaved
        {
            get { return _settingsSaved; }
            set { SetProperty(ref _settingsSaved, value); }
        }
        
        public bool IsValid
        {
            get { return _isValid; }
            set { SetProperty(ref _isValid, value); }
        }

        public bool Init
        {
            get { return _init; }
            set { SetProperty(ref _init, value); }
        }

        #endregion
        
        #region Private Methods

        private async void SaveSettings()
        {
            IsLoading = true;
            SettingsSaved = false;

            Settings.SettingsVersion = ApplicationInfo.Version;

            ApplyProxySettings();
            if (Settings.AppSettings.RunApplicationAtSystemStartup)
            {
                WindowsStartupService.RunAtWindowsStartup();
            }
            else
            {
                WindowsStartupService.RemoveFromWindowsStartup();
            }

            try
            {
                foreach (var calendarSyncProfile in Settings.CalendarSyncProfiles)
                {
                    calendarSyncProfile.SetSourceDestTypes();   
                }

                foreach (var calendarSyncProfile in Settings.TaskSyncProfiles)
                {
                    calendarSyncProfile.SetSourceDestTypes();
                }

                foreach (var calendarSyncProfile in Settings.ContactSyncProfiles)
                {
                    calendarSyncProfile.SetSourceDestTypes();
                }

                var result = await SettingsSerializationService.SerializeSettingsAsync(Settings);
                
                if (result)
                {
                    _lastSavedSettings = Settings;
                    Settings = _lastSavedSettings.DeepClone();
                }

                await MessageService.ShowMessage(result ? "Settings Saved Successfully" : "Error Saving Settings",
                        "Settings");
                
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

        private void CancelSettingsHandler(object o)
        {
            Settings = _lastSavedSettings.DeepClone();

            foreach (var syncProfile in Settings.CalendarSyncProfiles)
            {
                syncProfile.IsLoaded = false;
            }

            foreach (var syncProfile in Settings.TaskSyncProfiles)
            {
                syncProfile.IsLoaded = false;
            }

            foreach (var syncProfile in Settings.ContactSyncProfiles)
            {
                syncProfile.IsLoaded = false;
            }

            Init = true;
            MessageService.ShowMessage("Settings cancelled.",
                        "Settings");
        }
        
        private async void DisconnectGoogleHandler(object parameter)
        {
            GoogleAccount googleAccount = parameter as GoogleAccount;
            if (googleAccount == null)
            {
                MessageService.ShowMessageAsync("No account selected");
                return;
            }
            string accountName = googleAccount.Name;

            var dialogResult =
                await
                    MessageService.ShowConfirmMessage(
                        "Disconnection of Google account cannot be reverted.\nClick Yes to continue.");
            if (dialogResult == MessageDialogResult.Negative)
            {
                return;
            }

            var result = AccountAuthenticationService.DisconnectGoogle(googleAccount.Name);
            if (result)
            {

                foreach (var profile in Settings.CalendarSyncProfiles)
                {
                    if (profile.GoogleSettings.GoogleAccount != null &&
                        profile.GoogleSettings.GoogleAccount.Name.Equals(googleAccount.Name))
                    {
                        profile.GoogleSettings.GoogleAccount = null;
                    }
                    profile.IsLoaded = false;
                }

                //Remove google account
                 googleAccount = Settings.GoogleAccounts.FirstOrDefault(account =>
                    account.Name == accountName);
                
                if (googleAccount != null)
                {
                    Settings.GoogleAccounts.Remove(googleAccount);
                }

                googleAccount = _lastSavedSettings.GoogleAccounts.FirstOrDefault(account =>
                    account.Name == accountName);

                if (googleAccount != null)
                {
                    _lastSavedSettings.GoogleAccounts.Remove(googleAccount);
                }

                await MessageService.ShowMessage("Google account successfully disconnected");

                await SettingsSerializationService.SerializeSettingsAsync(_lastSavedSettings);
            }
            else
            {
                MessageService.ShowMessageAsync("Account wasn't authenticated earlier or disconnection failed.");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal void ApplyProxySettings()
        {
            IWebProxy proxy;
            try
            {
                var proxySettings = Settings.AppSettings.ProxySettings;
                switch (proxySettings.ProxyType)
                {
                    case ProxyType.NoProxy:
                        WebRequest.DefaultWebProxy = null;
                        break;
                    case ProxyType.ProxyWithAuth:
                        proxy =
                            new WebProxy(
                                new Uri(string.Format("{0}:{1}", proxySettings.ProxyAddress, proxySettings.Port)),
                                proxySettings.BypassOnLocal)
                            {
                                UseDefaultCredentials = proxySettings.UseDefaultCredentials
                            };

                        if (!proxySettings.UseDefaultCredentials)
                        {
                            proxy.Credentials = string.IsNullOrEmpty(proxySettings.Domain)
                                ? new NetworkCredential(proxySettings.UserName, proxySettings.Password)
                                : new NetworkCredential(proxySettings.UserName, proxySettings.Password,
                                    proxySettings.Domain);
                        }
                        WebRequest.DefaultWebProxy = proxy;
                        break;
                    default:
                        proxy = WebRequest.GetSystemWebProxy();
                        WebRequest.DefaultWebProxy = proxy;
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                MessageService.ShowMessageAsync("Invalid Proxy Settings. Proxy settings cannot be applied");
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        private async void AddNewGoogleAccountHandler()
        {
            //Accept Email Id
            var accountName = await MessageService.ShowInput("Enter your Google Email", "Adding Google Account");

            if (string.IsNullOrEmpty(accountName))
            {
                return;
            }

            if (Settings.GoogleAccounts != null &&
                Settings.GoogleAccounts.Any(t => t.Name.Equals(accountName, StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageService.ShowMessageAsync("An account with the same email already exists. Please try again.");
                return;
            }

            //Create cancellation token to support cancellation
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            if (Settings.AllowManualAuthentication)
            {
                var authResult =
                    await
                        AccountAuthenticationService.ManualAccountAuthetication(accountName, tokenSource.Token,
                            GetGoogleAuthCode);

                if (!authResult)
                {
                    MessageService.ShowMessageAsync("Account Not Added, Authorization Interrupted, Try Again");
                }
                else
                {
                    AddGoogleAccountDetailsToApplication(accountName);
                }
            }
            else
            {
                // Start progress controller
                var progressDialogController =
                    await
                        MessageService.ShowProgress("Authenticate and Authorize in the browser window",
                            "Add Google Account");
                //Delay for Preparedness
                await Task.Delay(5000);

                progressDialogController.SetIndeterminate();
                progressDialogController.SetCancelable(true);

                var authorizeGoogleAccountTask = AccountAuthenticationService.AuthorizeGoogleAccount(accountName,
                    tokenSource.Token);

                //Wait for 120 seconds
                var timeInSeconds = 120;
                while (timeInSeconds > 0)
                {
                    progressDialogController.SetMessage(
                        String.Format("Authenticate and Authorize in the browser window in {0} secs",
                            timeInSeconds));

                    //cancel task if cancellation is requested
                    if (progressDialogController.IsCanceled)
                    {
                        tokenSource.Cancel();
                        break;
                    }

                    //break loop if task changes its status
                    if (authorizeGoogleAccountTask.IsCanceled || authorizeGoogleAccountTask.IsFaulted ||
                        authorizeGoogleAccountTask.IsCompleted)
                    {
                        break;
                    }
                    timeInSeconds--;
                    await Task.Delay(1000, tokenSource.Token);
                }

                if (timeInSeconds < 0)
                {
                    tokenSource.Cancel();
                }

                await progressDialogController.CloseAsync();

                if (authorizeGoogleAccountTask.IsCanceled || authorizeGoogleAccountTask.IsFaulted ||
                    tokenSource.Token.IsCancellationRequested ||
                    progressDialogController.IsCanceled)
                {
                    MessageService.ShowMessageAsync("Account Not Added, Authorization Interrupted, Try Again");
                }
                else
                {
                    AddGoogleAccountDetailsToApplication(accountName);
                }
            }
        }

        private async void AddGoogleAccountDetailsToApplication(string accountName)
        {
            var account = new GoogleAccount { Name = accountName };
            Settings.GoogleAccounts.Add(account.DeepClone());
            _lastSavedSettings.GoogleAccounts.Add(account);

            await SettingsSerializationService.SerializeSettingsAsync(_lastSavedSettings);
        }

        private async Task<string> GetGoogleAuthCode()
        {
            return await MessageService.ShowInput("Enter Auth Code after authorization in browser window",
                        "Manual Authentication");
        }

        public void Load()
        {
            Init = true;
        }
        #endregion
    }
}