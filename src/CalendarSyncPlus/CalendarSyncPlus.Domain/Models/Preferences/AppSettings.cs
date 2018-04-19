using Newtonsoft.Json;
using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    
    public class AppSettings : Model
    {
        [JsonProperty("checkForAlphaReleases")]
        private bool _checkForAlphaReleases;
        [JsonProperty("checkForUpdates")]
        private bool _checkForUpdates;
        [JsonProperty("hideSystemTrayToolTip")]
        private bool _hideSystemTrayTooltip;
        [JsonProperty("manualSynchronization")]
        private bool _isManualSynchronization;
        [JsonProperty("minimizeToTray")]
        private bool _minimizeToSystemTray;
        [JsonProperty("periodicSync")]
        private bool _periodicSyncOn;
        [JsonProperty("proxy")]
        private ProxySetting _proxySettings;
        [JsonProperty("runAtStartup")]
        private bool _runApplicationAtSystemStartup;
        [JsonProperty("startMinimized")]
        private bool _startMinimized;

        public static AppSettings GetDefault()
        {
            return new AppSettings
            {
                MinimizeToSystemTray = true,
                CheckForUpdates = true,
                PeriodicSyncOn = true,
                RunApplicationAtSystemStartup = true,
                ProxySettings = new ProxySetting
                {
                    ProxyType = ProxyType.Auto
                }
            };
        }

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