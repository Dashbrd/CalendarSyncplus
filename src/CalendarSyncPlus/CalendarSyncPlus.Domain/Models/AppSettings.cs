namespace CalendarSyncPlus.Domain.Models
{
    public class AppSettings
    {
        #region Properties

        public bool IsFirstSave { get; set; }

        public bool MinimizeToSystemTray { get; set; }

        public bool HideSystemTrayTooltip { get; set; }

        public bool CheckForUpdates { get; set; }

        public bool RememberPeriodicSyncOn { get; set; }

        public bool PeriodicSyncOn { get; set; }

        public bool RunApplicationAtSystemStartup { get; set; }

        public ProxySetting ProxySettings { get; set; }

        #endregion
    }
}