using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Application.Services;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.OutlookServices.Outlook
{
    public interface IOutlookCalendarService : ICalendarService
    {
        List<OutlookMailBox> GetAllMailBoxes(string profileName);

        Task<List<string>> GetOutLookProfieListAsync();
    }
}