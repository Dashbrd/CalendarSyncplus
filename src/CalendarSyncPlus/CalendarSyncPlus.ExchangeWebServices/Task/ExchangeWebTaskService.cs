using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Tasks.Interfaces;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.ExchangeWebServices.Task
{
    [Export(typeof(ITaskService)),Export(typeof(IExchangeWebTaskService))]
    [ExportMetadata("ServiceType", ServiceType.EWS)]
    public class ExchangeWebTaskService : IExchangeWebTaskService
    {

        public string TaskServiceName
        {
            get { return "ExchangeWeb"; }
        }

        public Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks, IDictionary<string, object> taskListSpecificData)
        {
            throw new System.NotImplementedException();
        }

        public Task<TasksWrapper> GetReminderTasksInRangeAsync(IDictionary<string, object> taskListSpecificData)
        {
            throw new System.NotImplementedException();
        }

        public Task<TasksWrapper> AddReminderTasks(List<ReminderTask> tasks, 
            IDictionary<string, object> taskListSpecificData)
        {
            throw new System.NotImplementedException();
        }

        public void CheckTaskListSpecificData(IDictionary<string, object> taskListSpecificData)
        {
            throw new System.NotImplementedException();
        }

        public Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,  IDictionary<string, object> taskListSpecificData)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ClearCalendar(IDictionary<string, object> taskListSpecificData)
        {
            throw new System.NotImplementedException();
        }
    }
}