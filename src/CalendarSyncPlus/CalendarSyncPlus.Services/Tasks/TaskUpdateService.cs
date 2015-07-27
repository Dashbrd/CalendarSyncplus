using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Foundation;
using CalendarSyncPlus.Analytics.Interfaces;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Calendars;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using CalendarSyncPlus.SyncEngine;
using CalendarSyncPlus.SyncEngine.Interfaces;
using log4net;

namespace CalendarSyncPlus.Services.Tasks
{
    [Export(typeof(ITaskUpdateService))]
    public class TaskUpdateService : Model, ITaskUpdateService
    {
        private ITaskSyncEngine _taskSyncEngine;
        private ITaskServiceFactory _taskServiceFactory;
        private ISyncAnalyticsService _analyticsService;
        private TasksWrapper _destinationTasks;
        private TasksWrapper _sourceTasks;
        private string _taskSyncStatus;
        private ITaskService _sourceTaskService;
        private ITaskService _destinationTaskService;

        #region Constructors

        [ImportingConstructor]
        public TaskUpdateService(ITaskServiceFactory taskServiceFactory,
            ITaskSyncEngine taskSyncEngine,
            ISyncAnalyticsService analyticsService,
            ApplicationLogger applicationLogger)
        {
            Logger = applicationLogger.GetLogger(GetType());
            TaskServiceFactory = taskServiceFactory;
            TaskSyncEngine = taskSyncEngine;
            AnalyticsService = analyticsService;
        }

        #endregion

        #region Properties

        public ITaskSyncEngine TaskSyncEngine
        {
            get { return _taskSyncEngine; }
            set { SetProperty(ref _taskSyncEngine, value); }
        }

        public ITaskServiceFactory TaskServiceFactory
        {
            get { return _taskServiceFactory; }
            set { SetProperty(ref _taskServiceFactory, value); }
        }

        public ISyncAnalyticsService AnalyticsService
        {
            get { return _analyticsService; }
            set { SetProperty(ref _analyticsService, value); }
        }

        public ILog Logger { get; set; }

        public TasksWrapper DestinationTasks
        {
            get { return _destinationTasks; }
            set { SetProperty(ref _destinationTasks, value); }
        }

        public TasksWrapper SourceTasks
        {
            get { return _sourceTasks; }
            set { SetProperty(ref _sourceTasks, value); }
        }

        public string TaskSyncStatus
        {
            get { return _taskSyncStatus; }
            set { SetProperty(ref _taskSyncStatus, value); }
        }

        public ITaskService SourceTaskService
        {
            get { return _sourceTaskService; }
            set { SetProperty(ref _sourceTaskService, value); }
        }

        public ITaskService DestinationTaskService
        {
            get { return _destinationTaskService; }
            set { SetProperty(ref _destinationTaskService, value); }
        } 
        #endregion

        #region Private Methods
        void InitiatePreSyncSetup(TaskSyncProfile syncProfile)
        {
            SourceTaskService = TaskServiceFactory.GetTaskService(syncProfile.Source);
            DestinationTaskService = TaskServiceFactory.GetTaskService(syncProfile.Destination);
        }
        private IDictionary<string, object> GetCalendarSpecificData(ServiceType serviceType,
          TaskSyncProfile syncProfile)
        {
            IDictionary<string, object> calendarSpecificData = null;
            switch (serviceType)
            {
                case ServiceType.Google:
                    calendarSpecificData = new Dictionary<string, object>
                    {
                        {"TaskListId", syncProfile.GoogleSettings.GoogleCalendar.Id},
                        {"AccountName", syncProfile.GoogleSettings.GoogleAccount.Name}
                    };
                    break;
                case ServiceType.OutlookDesktop:
                    calendarSpecificData = new Dictionary<string, object>
                    {
                        {
                            "ProfileName",
                            !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile)
                                ? syncProfile.OutlookSettings.OutlookProfileName
                                : null
                        },
                        {
                            "OutlookTaskList",
                            !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultMailBoxCalendar)
                                ? syncProfile.OutlookSettings.OutlookFolder
                                : null
                        }
                    };
                    break;
                case ServiceType.EWS:
                    return null;
            }

            return calendarSpecificData;
        }


        private void GetDateRange(TaskSyncProfile syncProfile, out DateTime startDate, out DateTime endDate)
        {
            startDate = syncProfile.SyncSettings.StartDate.Date;
            endDate = syncProfile.SyncSettings.EndDate.Date;
            if (syncProfile.SyncSettings.SyncRangeType == SyncRangeTypeEnum.SyncRangeInDays)
            {
                startDate = DateTime.Today.AddDays((-syncProfile.SyncSettings.DaysInPast));
                endDate = DateTime.Today.AddDays((syncProfile.SyncSettings.DaysInFuture + 1));
            }
            else if (syncProfile.SyncSettings.SyncRangeType == SyncRangeTypeEnum.SyncEntireCalendar)
            {
                startDate = DateTime.Parse("1990/01/01 12:00:00 AM");
                endDate = DateTime.Today.AddYears(10);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <param name="startDate"></param>
        /// <returns>
        /// </returns>
        private bool LoadTasks(IDictionary<string, object> sourceCalendarSpecificData,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            //Update status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceReading,
                SourceTaskService.TaskServiceName);

            //Get source calendar
            SourceTasks =
                SourceTaskService.GetReminderTasksInRangeAsync(sourceCalendarSpecificData)
                    .Result;
            if (SourceTasks == null)
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceReadFailed);
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return false;
            }
            //Update status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceRead,
                SourceTaskService.TaskServiceName, SourceTasks.Count);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestReading,
                DestinationTaskService.TaskServiceName);

            //Get destination calendar
            DestinationTasks = DestinationTaskService.GetReminderTasksInRangeAsync(
                destinationCalendarSpecificData).Result;
            if (DestinationTasks == null)
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestReadFailed);
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return false;
            }
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestRead,
                DestinationTaskService.TaskServiceName, DestinationTasks.Count);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            return true;
        }
        /// <summary>
        ///     Add appointments to destination
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <returns>
        /// </returns>
        private bool AddDestinationTasks(TaskSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            //Update status for reading entries to add
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd,
                DestinationTaskService.TaskServiceName);
            //Get entries to add
            TaskSyncEngine.GetDestEntriesToAdd(syncProfile, SourceTasks,
                DestinationTasks);
            var appointmentsToAdd = TaskSyncEngine.DestTasksToAdd;
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, appointmentsToAdd.Count);
            if (appointmentsToAdd.Count == 0)
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries,
                DestinationTaskService.TaskServiceName);
            //Add entries to destination calendar
            var addedTasks = DestinationTaskService.AddReminderTasks(appointmentsToAdd,
                destinationCalendarSpecificData)
                .Result;
            var isSuccess = addedTasks.IsSuccess;
            //Update status if entries were successfully added
            TaskSyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            if (isSuccess)
            {
                syncMetric.DestMetric.AddCount = appointmentsToAdd.Count;
                //LoadSourceId(addedTasks, SourceTasks.CalendarId);
                DestinationTasks.AddRange(addedTasks);
                if (syncProfile.SyncMode == SyncModeEnum.TwoWay)
                {
                    //Add appointments to update
                    //var updateSourceList = UpdateWithChildId(addedTasks, SourceTasks);
                    //TaskSyncEngine.SourceTasksToUpdate.AddRangeCompareForUpdate(updateSourceList);
                }
            }
            return isSuccess;
        }

        /// <summary>
        ///     Delete appointments in destination
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <param name="syncCallback"></param>
        /// <returns>
        /// </returns>
        private bool DeleteDestinationTasks(TaskSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> destinationCalendarSpecificData, SyncCallback syncCallback)
        {
            //if (syncProfile.SyncSettings.DisableDelete)
            //{
            //    return true;
            //}
            //Updating entry isDeleteOperation status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete,
                DestinationTaskService.TaskServiceName);
            //Getting appointments to isDeleteOperation
            TaskSyncEngine.GetDestEntriesToDelete(syncProfile,
                SourceTasks, DestinationTasks);
            var appointmentsToDelete = TaskSyncEngine.DestTasksToDelete;

            if (syncProfile.SyncMode == SyncModeEnum.OneWay)
            {
                //if (syncProfile.SyncSettings.ConfirmOnDelete && syncCallback != null)
                //{
                //    var orphanEntries = Environment.NewLine +
                //                        string.Join(Environment.NewLine, TaskSyncEngine.DestOrphanEntries);
                //    //Log Orphan Entries
                //    Logger.Warn("Orphan entries to delete: " + orphanEntries);

                //    var message = string.Format("Are you sure you want to delete {0} orphan entries from {1}?{2}",
                //        appointmentsToDelete.Count, DestinationTaskService.TaskServiceName,
                //        orphanEntries);
                //    var e = new SyncEventArgs(message, UserActionEnum.ConfirmDelete);

                //    var task = syncCallback(e);
                //    if (task.Result)
                //    {
                //        appointmentsToDelete.AddRange(TaskSyncEngine.DestOrphanEntries);
                //    }
                //}
                //else if (!syncProfile.SyncSettings.DisableDelete)
                //{
                //    appointmentsToDelete.AddRange(TaskSyncEngine.DestOrphanEntries);
                //}
            }

            //Updating Get entry isDeleteOperation status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);

            if (appointmentsToDelete.Count == 0)
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }

            //Updating isDeleteOperation status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries,
                DestinationTaskService.TaskServiceName);

            //Deleting entries

            var deletedTasks = DestinationTaskService.DeleteReminderTasks(appointmentsToDelete, destinationCalendarSpecificData)
                    .Result;
            var isSuccess = deletedTasks.IsSuccess;
            //Update status if entries were successfully deleted
            TaskSyncStatus =
                StatusHelper.GetMessage(isSuccess
                    ? SyncStateEnum.DeletingEntriesComplete
                    : SyncStateEnum.DeletingEntriesFailed);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            if (isSuccess)
            {
                syncMetric.DestMetric.DeleteCount = appointmentsToDelete.Count;
                syncMetric.DestMetric.DeleteFailedCount = appointmentsToDelete.Count - deletedTasks.Count;
                for (var index = 0; index < appointmentsToDelete.Count; index++)
                {
                    DestinationTasks.Remove(appointmentsToDelete[index]);
                }
            }

            return isSuccess;
        }

        /// <summary>
        ///     Add appointments to source
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <returns>
        /// </returns>
        private bool AddSourceTasks(TaskSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> sourceCalendarSpecificData)
        {
            //Update status for reading entries to add
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd,
                SourceTaskService.TaskServiceName);
            //Get entries to add
            TaskSyncEngine.GetSourceEntriesToAdd(syncProfile, SourceTasks, DestinationTasks);
            var appointmentsToAdd = TaskSyncEngine.SourceTasksToAdd;
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, appointmentsToAdd.Count);
            if (appointmentsToAdd.Count == 0)
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries, SourceTaskService.TaskServiceName);

            //Add entries to calendar
            var addedTasks = SourceTaskService.AddReminderTasks(appointmentsToAdd,
                sourceCalendarSpecificData)
                .Result;
            var isSuccess = addedTasks.IsSuccess;
            //Update status if entries were successfully added
            TaskSyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            if (isSuccess)
            {
                syncMetric.SourceMetric.AddCount = appointmentsToAdd.Count;
                //LoadSourceId(addedTasks, DestinationTasks.TaskListId);
                SourceTasks.AddRange(addedTasks);
                if (syncProfile.SyncMode == SyncModeEnum.TwoWay)
                {
                    //var updateDestList = UpdateWithChildId(addedTasks, DestinationTasks);
                    //TaskSyncEngine.DestTasksToUpdate.AddRangeCompareForUpdate(updateDestList);
                }
            }

            return isSuccess;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addedTasks"></param>
        /// <param name="existingTasks"></param>
        /// <returns></returns>
        private List<Appointment> UpdateWithChildId(TasksWrapper addedTasks,
            TasksWrapper existingTasks)
        {
            //Add appointments to update
            var updateList = new List<Appointment>();
            foreach (var appointment in addedTasks)
            {
                //var presentAppointment = existingTasks.FirstOrDefault(t => t.CompareSourceId(appointment));
                //if (presentAppointment != null)
                //{
                //    var childKey = appointment.GetChildEntryKey();
                //    if (!presentAppointment.ExtendedProperties.ContainsKey(childKey))
                //    {
                //        presentAppointment.ExtendedProperties.Add(childKey, appointment.AppointmentId);
                //    }
                //    else
                //    {
                //        presentAppointment.ExtendedProperties[childKey] = appointment.AppointmentId;
                //    }
                //    updateList.Add(presentAppointment);
                //}
            }
            return updateList;
        }

        /// <summary>
        ///     Delete appointments from source
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <param name="syncCallback"></param>
        /// <returns>
        /// </returns>
        private bool DeleteSourceTasks(TaskSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> sourceCalendarSpecificData, SyncCallback syncCallback)
        {
            //if (syncProfile.SyncSettings.DisableDelete)
            //{
            //    return true;
            //}
            //Updating entry isDeleteOperation status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete,
                SourceTaskService.TaskServiceName);
            //Getting appointments to isDeleteOperation
            TaskSyncEngine.GetSourceEntriesToDelete(syncProfile, SourceTasks, DestinationTasks);
            var appointmentsToDelete = TaskSyncEngine.SourceTasksToDelete;
            //Updating Get entry isDeleteOperation status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
            if (appointmentsToDelete.Count == 0)
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }

            //Updating isDeleteOperation status
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries,
                SourceTaskService.TaskServiceName);
            //Deleting entries
            var deletedTasks =
                SourceTaskService.DeleteReminderTasks(appointmentsToDelete, sourceCalendarSpecificData).Result;
            var isSuccess = deletedTasks.IsSuccess;
            //Update status if entries were successfully deleted
            TaskSyncStatus =
                StatusHelper.GetMessage(isSuccess
                    ? SyncStateEnum.DeletingEntriesComplete
                    : SyncStateEnum.DeletingEntriesFailed);
            TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            if (isSuccess)
            {
                syncMetric.SourceMetric.DeleteCount = appointmentsToDelete.Count;
                syncMetric.SourceMetric.DeleteFailedCount = appointmentsToDelete.Count - deletedTasks.Count;
                for (var index = 0; index < appointmentsToDelete.Count; index++)
                {
                    SourceTasks.Remove(appointmentsToDelete[index]);
                }
            }
            return isSuccess;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <returns></returns>
        private bool UpdateEntries(TaskSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> sourceCalendarSpecificData,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            var isSuccess = true;
            if (TaskSyncEngine.SourceTasksToUpdate.Any())
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                //Update status for reading entries to update
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToUpdate,
                    TaskSyncEngine.SourceTasksToUpdate.Count,
                    SourceTaskService.TaskServiceName);
                var updatedTasks = SourceTaskService.UpdateReminderTasks(TaskSyncEngine.SourceTasksToUpdate,
                    sourceCalendarSpecificData).Result;
                isSuccess = updatedTasks.IsSuccess;
                TaskSyncStatus =
                    StatusHelper.GetMessage(isSuccess
                        ? SyncStateEnum.UpdateEntriesSuccess
                        : SyncStateEnum.UpdateEntriesFailed);
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                syncMetric.SourceMetric.UpdateCount = TaskSyncEngine.SourceTasksToUpdate.Count;
                syncMetric.SourceMetric.UpdateFailedCount =
                    TaskSyncEngine.SourceTasksToUpdate.Count - updatedTasks.Count;
            }

            if (TaskSyncEngine.DestTasksToUpdate.Any())
            {
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                //Update status for reading entries to update
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToUpdate,
                    TaskSyncEngine.DestTasksToUpdate.Count,
                    DestinationTaskService.TaskServiceName);
                var updatedTasks = DestinationTaskService.UpdateReminderTasks(TaskSyncEngine.DestTasksToUpdate,
                    destinationCalendarSpecificData).Result;
                isSuccess = updatedTasks.IsSuccess;
                TaskSyncStatus =
                    StatusHelper.GetMessage(isSuccess
                        ? SyncStateEnum.UpdateEntriesSuccess
                        : SyncStateEnum.UpdateEntriesFailed);
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                syncMetric.DestMetric.UpdateCount = TaskSyncEngine.DestTasksToUpdate.Count;
                syncMetric.DestMetric.UpdateFailedCount =
                    TaskSyncEngine.DestTasksToUpdate.Count - updatedTasks.Count;
            }

            return isSuccess;
        }
        #endregion

        #region Public Methods
        public bool SyncTask(TaskSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            InitiatePreSyncSetup(syncProfile);

            var isSuccess = false;
            if (syncProfile != null)
            {
                TaskSyncEngine.Clear();
                //Add log for sync mode
                TaskSyncStatus = string.Format("Calendar Sync : {0} {2} {1}", SourceTaskService.TaskServiceName,
                    DestinationTaskService.TaskServiceName,
                    syncProfile.SyncMode == SyncModeEnum.TwoWay ? "<===>" : "===>");
                TaskSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                DateTime startDate, endDate;
                GetDateRange(syncProfile, out startDate, out endDate);
                //Add log for date range
                TaskSyncStatus = string.Format("Date Range : {0} - {1}",
                    startDate.ToString("d"),
                    endDate.ToString("d"));

                //Load calendar specific data
                var sourceCalendarSpecificData =
                    GetCalendarSpecificData(syncProfile.Source, syncProfile);
                var destinationCalendarSpecificData =
                    GetCalendarSpecificData(syncProfile.Destination, syncProfile);

                //Get source and destination Tasks
                isSuccess = LoadTasks(sourceCalendarSpecificData,
                    destinationCalendarSpecificData);

                if (isSuccess)
                {
                    syncMetric.SourceMetric.OriginalCount = SourceTasks.Count;
                    syncMetric.DestMetric.OriginalCount = DestinationTasks.Count;
                    //LoadSourceId(DestinationTasks, SourceTasks.TaskListId);
                    //LoadSourceId(SourceTasks, DestinationTasks.TaskListId);
                }

                if (isSuccess)
                {
                    //Delete destination Tasks
                    isSuccess = DeleteDestinationTasks(syncProfile, syncMetric, destinationCalendarSpecificData, syncCallback);
                }

                if (isSuccess)
                {
                    //Add Tasks to destination
                    isSuccess = AddDestinationTasks(syncProfile, syncMetric, destinationCalendarSpecificData);
                }

                if (isSuccess && syncProfile.SyncMode == SyncModeEnum.TwoWay)
                {
                    //Delete destination appointments
                    isSuccess = DeleteSourceTasks(syncProfile, syncMetric, sourceCalendarSpecificData, syncCallback);
                    if (isSuccess)
                    {
                        //If sync mode is two way... add events to source
                        isSuccess = AddSourceTasks(syncProfile, syncMetric, sourceCalendarSpecificData);
                    }
                }

                if (isSuccess)
                {
                    isSuccess = UpdateEntries(syncProfile, syncMetric, sourceCalendarSpecificData, destinationCalendarSpecificData);
                }
            }
            syncMetric.IsSuccess = isSuccess;
            SourceTasks = null;
            DestinationTasks = null;
            SourceTaskService = null;
            DestinationTaskService = null;
            return isSuccess;
        } 
        #endregion

    }
}
