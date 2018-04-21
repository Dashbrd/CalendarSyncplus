using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [DataContract]   
    public class AppSettings : Model
    {
        private bool _checkForAlphaReleases;
        private bool _checkForUpdates;
        private bool _hideSystemTrayTooltip;
        private bool _isManualSynchronization;
        private bool _minimizeToSystemTray;
        private bool _periodicSyncOn;
        private ProxySetting _proxySettings;
        private bool _runApplicationAtSystemStartup;
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
        [DataMember]
        public bool MinimizeToSystemTray
        {
            get { return _minimizeToSystemTray; }
            set { SetProperty(ref _minimizeToSystemTray, value); }
        }
        [DataMember]
        public bool HideSystemTrayTooltip
        {
            get { return _hideSystemTrayTooltip; }
            set { SetProperty(ref _hideSystemTrayTooltip, value); }
        }
        [DataMember]
        public bool CheckForUpdates
        {
            get { return _checkForUpdates; }
            set { SetProperty(ref _checkForUpdates, value); }
        }
        [DataMember]
        public bool CheckForAlphaReleases
        {
            get { return _checkForAlphaReleases; }
            set { SetProperty(ref _checkForAlphaReleases, value); }
        }
        [DataMember]
        public bool IsManualSynchronization
        {
            get { return _isManualSynchronization; }
            set { SetProperty(ref _isManualSynchronization, value); }
        }
        [DataMember]
        public bool StartMinimized
        {
            get { return _startMinimized; }
            set { SetProperty(ref _startMinimized, value); }
        }
        [DataMember]
        public bool PeriodicSyncOn
        {
            get { return _periodicSyncOn; }
            set { SetProperty(ref _periodicSyncOn, value); }
        }
        [DataMember]
        public bool RunApplicationAtSystemStartup
        {
            get { return _runApplicationAtSystemStartup; }
            set { SetProperty(ref _runApplicationAtSystemStartup, value); }
        }
        [DataMember]
        public ProxySetting ProxySettings
        {
            get { return _proxySettings; }
            set { SetProperty(ref _proxySettings, value); }
        }

        #endregion
    }
}