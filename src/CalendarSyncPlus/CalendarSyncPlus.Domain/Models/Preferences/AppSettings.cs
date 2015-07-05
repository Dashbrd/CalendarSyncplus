using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class AppSettings : Model
    {
        private bool _isManualSynchronization;
        private bool _minimizeToSystemTray;
        private bool _hideSystemTrayTooltip;
        private bool _checkForUpdates;
        private bool _checkForAlphaReleases;
        private bool _startMinimized;
        private bool _periodicSyncOn;
        private bool _runApplicationAtSystemStartup;
        private ProxySetting _proxySettings;

        #region Properties

        public bool MinimizeToSystemTray
        {
            get { return _minimizeToSystemTray; }
            set { SetProperty(ref _minimizeToSystemTray, value); }
        }

        public bool HideSystemTrayTooltip
        {
            get { return _hideSystemTrayTooltip; }
            set { SetProperty(ref _hideSystemTrayTooltip, value); }
        }

        public bool CheckForUpdates
        {
            get { return _checkForUpdates; }
            set { SetProperty(ref _checkForUpdates, value); }
        }

        public bool CheckForAlphaReleases
        {
            get { return _checkForAlphaReleases; }
            set { SetProperty(ref _checkForAlphaReleases, value); }
        }

        public bool IsManualSynchronization
        {
            get { return _isManualSynchronization; }
            set { SetProperty(ref _isManualSynchronization, value); }
        }

        public bool StartMinimized
        {
            get { return _startMinimized; }
            set { SetProperty(ref _startMinimized, value); }
        }

        public bool PeriodicSyncOn
        {
            get { return _periodicSyncOn; }
            set { SetProperty(ref _periodicSyncOn, value); }
        }

        public bool RunApplicationAtSystemStartup
        {
            get { return _runApplicationAtSystemStartup; }
            set { SetProperty(ref _runApplicationAtSystemStartup, value); }
        }

        public ProxySetting ProxySettings
        {
            get { return _proxySettings; }
            set { SetProperty(ref _proxySettings, value); }
        }

        #endregion
    }
}