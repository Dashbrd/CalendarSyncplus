using System.Collections.Generic;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.Google
{
    public interface IGoogleCalendarService
    {
        Task<List<Calendar>> GetAvailableCalendars();
        Task<List<Appointment>> GetCalendarEventsInRangeAsync(int daysInPast, int daysInFuture, string calenderId);

        Task<bool> AddCalendarEvent(Appointment calenderAppointment, string calenderId, bool addDescription,
            bool addReminder, bool addAttendees);

        Task<bool> AddCalendarEvent(List<Appointment> calenderAppointments, string calendarId, bool addDescription,
            bool addReminder, bool addAttendees);

        Task<bool> DeleteCalendarEvent(Appointment calendarAppointment, string calenderId);

        Task<bool> DeleteCalendarEvent(List<Appointment> calendarAppointments, string calenderId);
       
    }
}