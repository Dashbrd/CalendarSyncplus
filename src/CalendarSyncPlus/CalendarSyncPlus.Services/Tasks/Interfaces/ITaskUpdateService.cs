using System.ComponentModel;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Utilities;

namespace CalendarSyncPlus.Services.Tasks.Interfaces
{
    public interface ITaskUpdateService : INotifyPropertyChanged
    {
        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="syncCallback"></param>
        /// <returns>
        /// </returns>
        bool SyncTask(TaskSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback);

        #endregion

        #region Properties

        TasksWrapper DestinationTasks { get; set; }
        TasksWrapper SourceTasks { get; set; }

        string TaskSyncStatus { get; set; }
        ITaskService SourceTaskService { get; set; }
        ITaskService DestinationTaskService { get; set; }

        #endregion
    }
}