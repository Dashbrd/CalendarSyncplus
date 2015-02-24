using System.Collections.Generic;
using System.Threading.Tasks;

using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.Outlook
{
    public interface IOutlookCalendarService
    {
        Task<List<Appointment>> GetOutlookAppointmentsAsync(int daysInPast, int daysInFuture, string profileName, OutlookCalendar outlookCalendar);

        List<OutlookMailBox> GetAllMailBoxes(string profileName);

        Task<List<string>> GetOutLookProfieListAsync();
    }
}