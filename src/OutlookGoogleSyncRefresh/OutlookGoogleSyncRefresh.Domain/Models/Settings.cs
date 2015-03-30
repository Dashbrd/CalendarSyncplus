using System.Collections.ObjectModel;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class Settings
    {
        public Settings()
        {
            SyncProfiles = new ObservableCollection<CalendarSyncProfile>();
            AppSettings = new AppSettings();
        }

        public ObservableCollection<CalendarSyncProfile> SyncProfiles { get; set; }

        public AppSettings AppSettings { get; set; }


        public static Settings GetDefaultSettings()
        {
            var settings = new Settings();
            settings.AppSettings = new AppSettings
            {
                IsFirstSave = true,
                MinimizeToSystemTray = true,
                CheckForUpdates = true,
                RememberPeriodicSyncOn = true,
                RunApplicationAtSystemStartup = true
            };
            settings.SyncProfiles.Add(CalendarSyncProfile.GetDefaultSyncProfile());
            return settings;
        }
    }
}