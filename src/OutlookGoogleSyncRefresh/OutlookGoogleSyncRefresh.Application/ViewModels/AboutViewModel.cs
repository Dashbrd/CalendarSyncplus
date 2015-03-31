using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class AboutViewModel : ViewModel<IAboutView>
    {
        private readonly IApplicationUpdateService _applicationUpdateService;
        private DelegateCommand _checkForUpdatesCommand;
        private DelegateCommand _downloadCommand;
        private bool _isCheckInProgress;
        private bool _isLatestVersionAvailable;
        private string _productVersion;
        private string _updateText;
        private DelegateCommand _uriCommand;

        [ImportingConstructor]
        public AboutViewModel(IAboutView aboutView, IApplicationUpdateService applicationUpdateService)
            : base(aboutView)
        {
            _applicationUpdateService = applicationUpdateService;
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
            if (IsCheckInProgress || IsLatestVersionAvailable)
            {
                return;
            }

            IsCheckInProgress = true;
            IsLatestVersionAvailable = false;
            UpdateText = string.Empty;
            TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task<string>.Factory.StartNew(() => _applicationUpdateService.GetLatestReleaseFromServer())
                .ContinueWith(CheckForUpdatesComplete, scheduler);
        }

        private void CheckForUpdatesComplete(Task<string> task)
        {
            IsCheckInProgress = false;
            if (task.Result == null)
            {
                if (_applicationUpdateService.IsNewVersionAvailable())
                {
                    UpdateText = string.Format("New version available for download : {0}",
                        _applicationUpdateService.GetNewAvailableVersion());
                    IsLatestVersionAvailable = true;
                }
                else
                {
                    UpdateText = "Your application is up to date.";
                }
            }
            else
            {
                UpdateText = task.Result;
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