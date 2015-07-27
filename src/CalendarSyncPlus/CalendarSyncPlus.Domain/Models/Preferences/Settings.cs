using System;
using System.Collections.ObjectModel;
using System.Waf.Applications;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Settings : Model
    {
        [NonSerialized]
        private bool _isFirstSave;

        private ObservableCollection<GoogleAccount> _googleAccounts;
        private ObservableCollection<CalendarSyncProfile> _calendarSyncProfiles;
        private ObservableCollection<ContactSyncProfile> _contactSyncProfiles;
        private ObservableCollection<TaskSyncProfile> _taskSyncProfiles;
        private AppSettings _appSettings;
        private LogSettings _logSettings;

        /// <summary>
        /// 
        /// </summary>
        public string SettingsVersion { get; set; }

        /// <summary>
        /// </summary>
        public bool AllowManualAuthentication { get; set; }

        /// <summary>
        /// </summary>
        public bool IsFirstSave
        {
            get { return _isFirstSave; }
            set { SetProperty(ref _isFirstSave, value); }
        }

        /// <summary>
        /// </summary>
        public ObservableCollection<GoogleAccount> GoogleAccounts
        {
            get { return _googleAccounts; }
            set { SetProperty(ref _googleAccounts, value); }
        }

        /// <summary>
        /// </summary>
        public ObservableCollection<CalendarSyncProfile> CalendarSyncProfiles
        {
            get { return _calendarSyncProfiles; }
            set { SetProperty(ref _calendarSyncProfiles, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<ContactSyncProfile> ContactSyncProfiles
        {
            get { return _contactSyncProfiles; }
            set { SetProperty(ref _contactSyncProfiles, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<TaskSyncProfile> TaskSyncProfiles
        {
            get { return _taskSyncProfiles; }
            set { SetProperty(ref _taskSyncProfiles, value); }
        }

        /// <summary>
        /// </summary>
        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set { SetProperty(ref _appSettings, value); }
        }

        /// <summary>
        /// </summary>
        public LogSettings LogSettings
        {
            get { return _logSettings; }
            set { SetProperty(ref _logSettings, value); }
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public static Settings GetDefaultSettings()
        {
            var settings = new Settings
            {
                IsFirstSave = true,
                SettingsVersion = ApplicationInfo.Version,
                AppSettings = new AppSettings
                {
                    MinimizeToSystemTray = true,
                    CheckForUpdates = true,
                    PeriodicSyncOn = true,
                    RunApplicationAtSystemStartup = true,
                    ProxySettings = new ProxySetting
                    {
                        ProxyType = ProxyType.Auto
                    }
                },
                CalendarSyncProfiles = new ObservableCollection<CalendarSyncProfile>
                {
                    CalendarSyncProfile.GetDefaultSyncProfile()
                },
                TaskSyncProfiles = new ObservableCollection<TaskSyncProfile>
                {
                    TaskSyncProfile.GetDefaultSyncProfile()
                },
                ContactSyncProfiles = new ObservableCollection<ContactSyncProfile>
                {
                    ContactSyncProfile.GetDefaultSyncProfile()
                },
                GoogleAccounts =  new ObservableCollection<GoogleAccount>()
            };
            return settings;
        }
       
    }
}