using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.GoogleServices.Google
{
    public interface IGoogleCalendarService : ICalendarService
    {
        Task<List<GoogleCalendar>> GetAvailableCalendars(string accountName);
    }
}