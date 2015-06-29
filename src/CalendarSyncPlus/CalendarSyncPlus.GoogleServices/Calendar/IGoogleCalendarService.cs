using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;

namespace CalendarSyncPlus.GoogleServices.Calendar
{
    public interface IGoogleCalendarService : ICalendarService
    {
        Task<List<GoogleCalendar>> GetAvailableCalendars(string accountName);
    }
}