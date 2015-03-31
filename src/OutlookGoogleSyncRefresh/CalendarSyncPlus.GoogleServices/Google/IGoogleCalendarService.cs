using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Application.Services;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.GoogleServices.Google
{
    public interface IGoogleCalendarService : ICalendarService
    {
        Task<List<Calendar>> GetAvailableCalendars(IDictionary<string, object> calendarSpecificData);
    }
}