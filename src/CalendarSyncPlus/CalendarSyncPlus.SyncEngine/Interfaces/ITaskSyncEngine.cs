using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;

namespace CalendarSyncPlus.SyncEngine.Interfaces
{
    public interface ITaskSyncEngine
    {
        /// <summary>
        /// </summary>
        List<ReminderTask> SourceTasksToUpdate { get; set; }

        /// <summary>
        /// </summary>
        List<ReminderTask> SourceOrphanEntries { get; set; }

        /// <summary>
        /// </summary>
        List<ReminderTask> SourceTasksToDelete { get; set; }

        /// <summary>
        /// </summary>
        List<ReminderTask> SourceTasksToAdd { get; set; }

        /// <summary>
        /// </summary>
        List<ReminderTask> DestTasksToUpdate { get; set; }

        /// <summary>
        /// </summary>
        List<ReminderTask> DestTasksToDelete { get; set; }

        /// <summary>
        /// </summary>
        List<ReminderTask> DestTasksToAdd { get; set; }

        /// <summary>
        /// </summary>
        List<ReminderTask> DestOrphanEntries { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetSourceEntriesToDelete(TaskSyncProfile syncProfile, TasksWrapper sourceList,
            TasksWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetSourceEntriesToAdd(TaskSyncProfile syncProfile, TasksWrapper sourceList,
            TasksWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetDestEntriesToDelete(TaskSyncProfile syncProfile, TasksWrapper sourceList,
            TasksWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetDestEntriesToAdd(TaskSyncProfile syncProfile, TasksWrapper sourceList,
            TasksWrapper destinationList);

        /// <summary>
        /// </summary>
        void Clear();
    }
}