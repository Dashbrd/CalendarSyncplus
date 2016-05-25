using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Tasks.Interfaces;

namespace CalendarSyncPlus.OutlookServices.Task
{
    public interface IOutlookTaskService : ITaskService
    {
        List<OutlookMailBox> GetAllMailBoxes(string profileName);
        Task<List<string>> GetOutLookProfieListAsync();
    }
}