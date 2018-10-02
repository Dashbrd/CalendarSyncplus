using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Waf.Applications;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [DataContract]
    /// <summary>
    /// </summary>
    public class Settings : Model
    {
        private string _settingsVersion;        
        private AppSettings _appSettings;
        private ObservableCollection<CalendarSyncProfile> _calendarSyncProfiles;
        private ObservableCollection<ContactSyncProfile> _contactSyncProfiles;
        private bool _allowManualAuthentication;
        private ObservableCollection<GoogleAccount> _googleAccounts;
        private bool _isFirstSave;
        private LogSettings _logSettings;
        private ObservableCollection<TaskSyncProfile> _taskSyncProfiles;

        [DataMember]
        /// <summary>
        /// </summary>
        public string SettingsVersion { get => _settingsVersion; set => _settingsVersion = value; }
        [DataMember]
        /// <summary>
        /// </summary>
        public bool AllowManualAuthentication { get => _allowManualAuthentication; set => _allowManualAuthentication = value; }
        // do not serialize this
        /// <summary>
        /// </summary>
        public bool IsFirstSave
        {
            get { return _isFirstSave; }
            set { SetProperty(ref _isFirstSave, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public ObservableCollection<GoogleAccount> GoogleAccounts
        {
            get { return _googleAccounts; }
            set { SetProperty(ref _googleAccounts, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public ObservableCollection<CalendarSyncProfile> CalendarSyncProfiles
        {
            get { return _calendarSyncProfiles; }
            set { SetProperty(ref _calendarSyncProfiles, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public ObservableCollection<ContactSyncProfile> ContactSyncProfiles
        {
            get { return _contactSyncProfiles; }
            set { SetProperty(ref _contactSyncProfiles, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public ObservableCollection<TaskSyncProfile> TaskSyncProfiles
        {
            get { return _taskSyncProfiles; }
            set { SetProperty(ref _taskSyncProfiles, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set { SetProperty(ref _appSettings, value); }
        }
        [DataMember]
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