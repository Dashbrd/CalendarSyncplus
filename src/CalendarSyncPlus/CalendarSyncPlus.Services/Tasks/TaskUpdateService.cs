using System;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using CalendarSyncPlus.Services.Utilities;

namespace CalendarSyncPlus.Services.Tasks
{
    [Export(typeof(ITaskUpdateService))]
    public class TaskUpdateService : ITaskUpdateService
    {

        public bool SyncTask(TaskSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            throw new NotImplementedException();
        }

        public TasksWrapper DestinationAppointments { get; set; }

        public TasksWrapper SourceAppointments
        {
            get;
            set;
        }

        public string SyncStatus
        {
            get;
            set;
        }

        public ITaskService SourceCalendarService
        {
            get;
            set;
        }

        public ITaskService DestinationCalendarService
        {
            get;
            set;
        }
    }
}
