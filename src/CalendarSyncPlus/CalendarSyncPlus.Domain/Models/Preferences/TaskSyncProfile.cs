using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    /// <summary>
    /// Task Sync Profile
    /// </summary>
    [Serializable]
    public class TaskSyncProfile : SyncProfile
    {
        private TaskSyncSettings _syncSettings;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaskSyncProfile()
        {
            Name = "Default Task Profile";
            GoogleSettings = new GoogleSettings();
            ExchangeServerSettings = new ExchangeServerSettings();
            OutlookSettings = new OutlookSettings();
            IsSyncEnabled = true;
            IsDefault = true;
        }

        public TaskSyncSettings SyncSettings
        {
            get { return _syncSettings; }
            set { SetProperty(ref _syncSettings, value); }
        }

        public static TaskSyncProfile GetDefaultSyncProfile()
        {
            var syncProfile = new TaskSyncProfile()
            {
                SyncSettings = TaskSyncSettings.GetDefault(),
                OutlookSettings =
                {
                    OutlookOptions = OutlookOptionsEnum.OutlookDesktop |
                                        OutlookOptionsEnum.DefaultProfile |
                                     OutlookOptionsEnum.DefaultMailBoxCalendar
                },
                SyncDirection = SyncDirectionEnum.OutlookGoogleOneWay,
                SyncFrequency = new IntervalSyncFrequency { Hours = 1, Minutes = 0, StartTime = DateTime.Now }
            };
            syncProfile.SetSourceDestTypes();
            return syncProfile;
        }
    }
}
