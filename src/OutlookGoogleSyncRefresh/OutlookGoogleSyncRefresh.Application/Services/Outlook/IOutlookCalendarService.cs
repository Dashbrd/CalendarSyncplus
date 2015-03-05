using System.Collections.Generic;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.Outlook
{
    public interface IOutlookCalendarService : ICalendarService
    {
        List<OutlookMailBox> GetAllMailBoxes(string profileName);

        Task<List<string>> GetOutLookProfieListAsync();
    }
}