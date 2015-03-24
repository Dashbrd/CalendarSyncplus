using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class Settings
    {
        public Settings()
        {
            SyncProfiles = new ObservableCollection<SyncProfile>();
            SyncFrequency = new HourlySyncFrequency();
            AppSettings = new AppSettings();
            LogSettings = new LogSettings();
        }

        public ObservableCollection<SyncProfile> SyncProfiles { get; set; }

        public AppSettings AppSettings { get; set; }

        public SyncFrequency SyncFrequency { get; set; }

        public LogSettings LogSettings { get; set; }

        public DateTime LastSuccessfulSync { get; set; }

    }
}
