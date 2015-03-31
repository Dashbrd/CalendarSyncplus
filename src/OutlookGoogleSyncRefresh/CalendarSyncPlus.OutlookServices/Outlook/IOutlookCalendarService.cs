using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Application.Services.Outlook
{
    public interface IOutlookCalendarService : ICalendarService
    {
        List<OutlookMailBox> GetAllMailBoxes(string profileName);

        Task<List<string>> GetOutLookProfieListAsync();
    }
}