using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class AppSettings : Model
    {
        private bool _isManualSynchronization;

        #region Properties

        public bool IsFirstSave { get; set; }

        public bool MinimizeToSystemTray { get; set; }

        public bool HideSystemTrayTooltip { get; set; }

        public bool CheckForUpdates { get; set; }

        public bool IsManualSynchronization
        {
            get { return _isManualSynchronization; }
            set { SetProperty(ref _isManualSynchronization, value); }
        }

        public bool PeriodicSyncOn { get; set; }

        public bool RunApplicationAtSystemStartup { get; set; }

        public ProxySetting ProxySettings { get; set; }

        #endregion
    }
}