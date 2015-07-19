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
            IDictionary<string, object> calendarSpecificData);

        Task<TasksWrapper> GetReminderTasksInRangeAsync(DateTime startDate, DateTime endDate,
            IDictionary<string, object> calendarSpecificData);

        Task<TasksWrapper> AddReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> calendarSpecificData);

        void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData);

        Task<TasksWrapper> UpdateReminderTasks(List<ReminderTask> reminderTasks,
            IDictionary<string, object> calendarSpecificData);

        //Task<bool> ResetReminderTasks(IDictionary<string, object> calendarSpecificData);

        Task<bool> ClearCalendar(IDictionary<string, object> calendarSpecificData);
    }
}
