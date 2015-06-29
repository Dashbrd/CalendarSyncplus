using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;

namespace CalendarSyncPlus.OutlookServices.Calendar
{
    public interface IOutlookCalendarService : ICalendarService
    {
        List<OutlookMailBox> GetAllMailBoxes(string profileName);
        Task<List<string>> GetOutLookProfieListAsync();
    }
}