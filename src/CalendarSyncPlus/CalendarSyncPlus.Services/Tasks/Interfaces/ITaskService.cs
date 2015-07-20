using System;
using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Wrappers;
using System.Threading.Tasks;

namespace CalendarSyncPlus.Services.Tasks.Interfaces
{
    public interface ITaskService
    {
        string TaskServiceName { get; }

        Task<TasksWrapper> DeleteReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> taskListSpecificData);

        Task<TasksWrapper> GetReminderTasksInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> taskListSpecificData);

        Task<TasksWrapper> AddReminderTasks(List<ReminderTask> tasks,
            IDictionary<string, object> taskListSpecificData);

        void CheckTaskListSpecificData(IDictionary<string, object> taskListSpecificData);

        Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> taskListSpecificData);

        //Task<bool> ResetReminderTasks(IDictionary<string, object> calendarSpecificData);

        Task<bool> ClearCalendar(IDictionary<string, object> taskListSpecificData);
    }
}
