using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Waf.Applications;
using Microsoft.Exchange.WebServices.Data;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Views;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class AboutViewModel : Utilities.ViewModel<IAboutView>
    {
        private readonly IApplicationUpdateService _applicationUpdateService;
        private readonly Settings _settings;
        private string _productVersion;
        private DelegateCommand _uriCommand;
        private DelegateCommand _checkForUpdatesCommand;
        private bool _isLatestVersionAvailable;
        private DelegateCommand _downloadCommand;
        private string _updateText;
        private bool _isCheckInProgress;

        [ImportingConstructor]
        public AboutViewModel(IAboutView aboutView, IApplicationUpdateService applicationUpdateService, Settings settings)
            : base(aboutView)
        {
            _applicationUpdateService = applicationUpdateService;
            _settings = settings;
            ProductVersion = ApplicationInfo.Version;
        }

        public string ProductVersion
        {
            get { return _productVersion; }
            set { SetProperty(ref _productVersion, value); }
        }

        /// <summary>
        /// </summary>
        public bool IsCheckInProgress
        {
            get { return _isCheckInProgress; }
            set { SetProperty(ref _isCheckInProgress, value); }
        }

        /// <summary>
        /// </summary>
        public bool IsLatestVersionAvailable
        {
            get { return _isLatestVersionAvailable; }
            set { SetProperty(ref _isLatestVersionAvailable, value); }
        }

        public DelegateCommand UriCommand
        {
            get { return _uriCommand = _uriCommand ?? new DelegateCommand(RequestNavigation); }
        }

        public DelegateCommand CheckForUpdatesCommand
        {
            get { return _checkForUpdatesCommand = _checkForUpdatesCommand ?? new DelegateCommand(CheckForUpdates); }
        }

        public DelegateCommand DownloadCommand
        {
            get { return _downloadCommand = _downloadCommand ?? new DelegateCommand(DownloadNewVersion); }
            
        }

        public string UpdateText
        {
            get { return _updateText; }
            set { SetProperty(ref _updateText, value); }
        }

        private void DownloadNewVersion()
        {
            Process.Start(new ProcessStartInfo(_applicationUpdateService.GetDownloadUri().AbsoluteUri));
        }

        private void CheckForUpdates()
        {
            IsCheckInProgress = true;
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task<bool>.Factory.StartNew(() => _applicationUpdateService.IsNewVersionAvailable())
                .ContinueWith(CheckForUpdatesComplete, scheduler);

        }

        private void CheckForUpdatesComplete(Task<bool> task)
        {
            IsCheckInProgress = false;
            if (task.Result)
            {
                UpdateText = string.Format("New version available for download : {0}",
                    _applicationUpdateService.GetNewAvailableVersion());
                IsLatestVersionAvailable = true;
            }
            else
            {
                IsLatestVersionAvailable = false;
                UpdateText = "Your application is up to date.";
            } 
        }

        private void RequestNavigation(object parameter)
        {
            if (parameter != null)
            {
                Process.Start(new ProcessStartInfo(new Uri(parameter.ToString()).AbsoluteUri));
            }
        }
    }
}
