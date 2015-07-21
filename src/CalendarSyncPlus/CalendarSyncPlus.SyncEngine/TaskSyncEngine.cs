using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.SyncEngine.Interfaces;

namespace CalendarSyncPlus.SyncEngine
{
    [Export(typeof(ITaskSyncEngine))]
    public class TaskSyncEngine : ITaskSyncEngine
    {
        public List<ReminderTask> SourceTasksToUpdate { get; set; }
        public List<ReminderTask> SourceOrphanEntries { get; set; }
        public List<ReminderTask> SourceTasksToDelete { get; set; }
        public List<ReminderTask> SourceTasksToAdd { get; set; }
        public List<ReminderTask> DestTasksToUpdate { get; set; }
        public List<ReminderTask> DestTasksToDelete { get; set; }
        public List<ReminderTask> DestTasksToAdd { get; set; }
        public List<ReminderTask> DestOrphanEntries { get; set; }

        public bool GetSourceEntriesToDelete(TaskSyncProfile syncProfile, TasksWrapper sourceList, TasksWrapper destinationList)
        {
            EvaluateTasksToDelete(syncProfile, destinationList, sourceList, SourceTasksToDelete,
               SourceTasksToUpdate, DestTasksToUpdate, SourceOrphanEntries);
            return true;
        }

        public bool GetSourceEntriesToAdd(TaskSyncProfile syncProfile, TasksWrapper sourceList, TasksWrapper destinationList)
        {
            EvaluateTasksToAdd(syncProfile, destinationList, sourceList, SourceTasksToAdd);
            return true;
        }

        public bool GetDestEntriesToDelete(TaskSyncProfile syncProfile, TasksWrapper sourceList, TasksWrapper destinationList)
        {
            EvaluateTasksToDelete(syncProfile, sourceList, destinationList, DestTasksToDelete,
               DestTasksToUpdate, SourceTasksToUpdate, DestOrphanEntries);
            return true;
        }

        public bool GetDestEntriesToAdd(TaskSyncProfile syncProfile, TasksWrapper sourceList, TasksWrapper destinationList)
        {
            EvaluateTasksToAdd(syncProfile, sourceList, destinationList, DestTasksToAdd);
            return true;
        }

        public void Clear()
        {
            SourceOrphanEntries = new List<ReminderTask>();
            SourceTasksToAdd = new List<ReminderTask>();
            SourceTasksToDelete = new List<ReminderTask>();
            SourceTasksToUpdate  = new List<ReminderTask>();

            DestOrphanEntries = new List<ReminderTask>();
            DestTasksToAdd = new List<ReminderTask>();
            DestTasksToDelete = new List<ReminderTask>();
            DestTasksToUpdate = new List<ReminderTask>();
        }


        #region Private Methods

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <param name="destTasksToDelete"></param>
        /// <param name="destAppointmentsToUpdate"></param>
        /// <param name="sourceTasksToUpdate"></param>
        /// <param name="destOrphanEntries"></param>
        /// <returns>
        /// </returns>
        private void EvaluateTasksToDelete(TaskSyncProfile syncProfile,
            TasksWrapper sourceList, TasksWrapper destinationList,
            List<ReminderTask> destTasksToDelete,
            List<ReminderTask> destAppointmentsToUpdate, 
            List<ReminderTask> sourceTasksToUpdate,
            List<ReminderTask> destOrphanEntries)
        {
           if (!destinationList.Any())
            {
                return;
            }

            foreach (var destTask in destinationList)
            {
                var sourceTask = sourceList.FirstOrDefault(t =>
                    t.Equals(destTask));
                if (destTask == null)
                {
                    destTasksToDelete.Add(sourceTask);
                }
            }
        }

        /// <summary>
        ///     Gets appointments to add in the destination calendar
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <param name="tasksToAdd"></param>
        /// <returns>
        /// </returns>
        private void EvaluateTasksToAdd(TaskSyncProfile syncProfile, List<ReminderTask> sourceList,
            List<ReminderTask> destinationList, List<ReminderTask> tasksToAdd)
        {
            if (!destinationList.Any())
            {
                tasksToAdd.AddRange(sourceList);
                //All entries need to be added
                return;
            }
           
            foreach (var sourceTask in sourceList)
            {
                var destTask = destinationList.FirstOrDefault(t =>
                    t.Equals(sourceTask));
                if (destTask == null)
                {
                    tasksToAdd.Add(sourceTask);
                }
            }

        }

        /// <summary>
        /// </summary>
        /// <param name="destTask"></param>
        /// <param name="sourceTask"></param>
        /// <returns>
        /// </returns>
        private bool CompareAppointments(ReminderTask destTask,
            ReminderTask sourceTask)
        {
            var isFound = destTask.Equals(sourceTask);
            //If both entries have same content


            return isFound;
        }

        #endregion
    }
}
