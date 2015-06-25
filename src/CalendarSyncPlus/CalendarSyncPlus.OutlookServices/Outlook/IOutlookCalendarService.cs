using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.OutlookServices.Outlook
{
    public interface IOutlookCalendarService : ICalendarService
    {
        List<OutlookMailBox> GetAllMailBoxes(string profileName);
        Task<List<string>> GetOutLookProfieListAsync();
    }
}