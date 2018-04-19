using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Waf.Applications;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    /// <summary>
    /// </summary>
    public class Settings : Model
    {
        [JsonProperty("version")]
        private string _settingsVersion;        
        [JsonProperty("appSettings")]
        private AppSettings _appSettings;
        [JsonProperty("calendarProfiles")]
        private ObservableCollection<CalendarSyncProfile> _calendarSyncProfiles;
        [JsonProperty("contactProfiles")]
        private ObservableCollection<ContactSyncProfile> _contactSyncProfiles;
        [JsonProperty("allowManualAuthentication")]
        private bool _allowManualAuthentication;
        [JsonProperty("googleAccounts")]
        private ObservableCollection<GoogleAccount> _googleAccounts;

        private bool _isFirstSave;
        [JsonProperty("logSettings")]
        private LogSettings _logSettings;
        [JsonProperty("taskProfiles")]
        private ObservableCollection<TaskSyncProfile> _taskSyncProfiles;
        

        /// <summary>
        /// </summary>
        public string SettingsVersion { get => _settingsVersion; set => _settingsVersion = value; }

        /// <summary>
        /// </summary>
        public bool AllowManualAuthentication { get => _allowManualAuthentication; set => _allowManualAuthentication = value; }

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
        /// </summary>
        public ObservableCollection<ContactSyncProfile> ContactSyncProfiles
        {
            get { return _contactSyncProfiles; }
            set { SetProperty(ref _contactSyncProfiles, value); }
        }

        /// <summary>
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
                AppSettings = AppSettings.GetDefault(),
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
                GoogleAccounts = new ObservableCollection<GoogleAccount>()
            };
            return settings;
        }
    }
}