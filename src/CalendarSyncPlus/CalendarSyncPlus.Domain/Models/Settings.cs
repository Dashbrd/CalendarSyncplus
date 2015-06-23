using System.Collections.ObjectModel;
using System.Waf.Applications;
using System.Xml.Serialization;

namespace CalendarSyncPlus.Domain.Models
{
    public class Settings
    {
        public string SettingsVersion { get; set; }

        /// <summary>
        /// </summary>
        public bool AllowManualAuthentication { get; set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<GoogleAccount> GoogleAccounts { get; set; }

        /// <summary>
        /// </summary>
        public ObservableCollection<CalendarSyncProfile> SyncProfiles { get; set; }

        /// <summary>
        /// </summary>
        public AppSettings AppSettings { get; set; }

        /// <summary>
        /// </summary>
        [XmlIgnore]
        public bool IsFirstSave { get; set; }

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
                SyncProfiles = new ObservableCollection<CalendarSyncProfile>
                {
                    CalendarSyncProfile.GetDefaultSyncProfile()
                },
                GoogleAccounts = new ObservableCollection<GoogleAccount>()
            };
            return settings;
        }
    }
}