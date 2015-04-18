using System.Collections.ObjectModel;

namespace CalendarSyncPlus.Domain.Models
{
    public class Settings
    {
        private AppSettings _appSettings;
        public ObservableCollection<CalendarSyncProfile> SyncProfiles { get; set; }

        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set { _appSettings = value; }
        }

        public static Settings GetDefaultSettings()
        {
            var settings = new Settings
            {
                AppSettings = new AppSettings
                {
                    IsFirstSave = true,
                    MinimizeToSystemTray = true,
                    CheckForUpdates = true,
                    PeriodicSyncOn = true,
                    RunApplicationAtSystemStartup = true,
                    ProxySettings = new ProxySetting()
                    {
                        ProxyType = ProxyType.Auto
                    }
                },

                SyncProfiles = new ObservableCollection<CalendarSyncProfile>()
                {
                    CalendarSyncProfile.GetDefaultSyncProfile()
                }
            };
            return settings;
        }
    }
}