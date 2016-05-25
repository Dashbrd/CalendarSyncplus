using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Tasks.Interfaces;

namespace CalendarSyncPlus.ExchangeWebServices.Task
{
    [Export(typeof(ITaskService)), Export(typeof(IExchangeWebTaskService))]
    [ExportMetadata("ServiceType", ServiceType.EWS)]
    public class ExchangeWebTaskService : IExchangeWebTaskService
    {
        #region IExchangeWebTaskService Members

        public string TaskServiceName
        {
            get { return "ExchangeWeb"; }
        }

        public Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> taskListSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> GetReminderTasksInRangeAsync(IDictionary<string, object> taskListSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> AddReminderTasks(List<ReminderTask> tasks,
            IDictionary<string, object> taskListSpecificData)
        {
            throw new NotImplementedException();
        }

        public void CheckTaskListSpecificData(IDictionary<string, object> taskListSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> taskListSpecificData)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ClearCalendar(IDictionary<string, object> taskListSpecificData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}