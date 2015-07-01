#region Imports

using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;

#endregion

namespace CalendarSyncPlus.Services.Sync.Interfaces
{
    public interface ISyncService : INotifyPropertyChanged, IService
    {
        #region Properties

        string SyncStatus { get; set; }

        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timerCallback"></param>
        /// <returns></returns>
        Task<bool> Start(ElapsedEventHandler timerCallback);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elapsedEventHandler"></param>
        void Stop(ElapsedEventHandler elapsedEventHandler);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="syncCallback"></param>
        /// <returns></returns>
        string SyncNow(CalendarSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="syncCallback"></param>
        /// <returns></returns>
        string SyncNow(ContactsSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="syncCallback"></param>
        /// <returns></returns>
        string SyncNow(TaskSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback);
        #endregion
    }
}