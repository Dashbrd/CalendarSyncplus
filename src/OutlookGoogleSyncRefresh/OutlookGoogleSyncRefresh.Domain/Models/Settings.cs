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
            AppSettings = new AppSettings();
        }

        public ObservableCollection<SyncProfile> SyncProfiles { get; set; }

        public AppSettings AppSettings { get; set; }
    }
}
