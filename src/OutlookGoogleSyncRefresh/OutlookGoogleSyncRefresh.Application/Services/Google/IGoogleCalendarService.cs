using System.Collections.Generic;
using System.Threading.Tasks;

using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.Google
{
    public interface IGoogleCalendarService :ICalendarService
    {
        Task<List<Calendar>> GetAvailableCalendars(IDictionary<string, object> calendarSpecificData);

    }
}