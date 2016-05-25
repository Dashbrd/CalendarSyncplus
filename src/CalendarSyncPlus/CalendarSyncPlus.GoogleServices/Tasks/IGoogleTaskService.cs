using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Tasks.Interfaces;

namespace CalendarSyncPlus.GoogleServices.Tasks
{
    public interface IGoogleTaskService : ITaskService
    {
        Task<List<GoogleCalendar>> GetAvailableTaskList(string accountName);
    }
}