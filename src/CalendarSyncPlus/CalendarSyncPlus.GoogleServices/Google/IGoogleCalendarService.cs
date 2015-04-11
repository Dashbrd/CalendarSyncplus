using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.GoogleServices.Google
{
    public interface IGoogleCalendarService : ICalendarService
    {
        Task<List<Calendar>> GetAvailableCalendars(IDictionary<string, object> calendarSpecificData);
    }
}