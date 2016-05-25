using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Wrappers;

namespace CalendarSyncPlus.Services.Calendars.Interfaces
{
    public interface ICalendarService
    {
        /// <summary>
        /// </summary>
        string CalendarServiceName { get; }

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        Task<AppointmentsWrapper> DeleteCalendarEvents(List<Appointment> calendarAppointments,
            IDictionary<string, object> calendarSpecificData);

        /// <summary>
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="skipPrivateEntries"></param>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        Task<AppointmentsWrapper> GetCalendarEventsInRangeAsync(DateTime startDate, DateTime endDate,
            bool skipPrivateEntries,
            IDictionary<string, object> calendarSpecificData);

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        Task<AppointmentsWrapper> AddCalendarEvents(List<Appointment> calendarAppointments, bool addDescription,
            bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData);

        /// <summary>
        /// </summary>
        /// <param name="calendarSpecificData"></param>
        void CheckCalendarSpecificData(IDictionary<string, object> calendarSpecificData);

        /// <summary>
        /// </summary>
        /// <param name="calendarAppointments"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminder"></param>
        /// <param name="addAttendees"></param>
        /// <param name="attendeesToDescription"></param>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        Task<AppointmentsWrapper> UpdateCalendarEvents(List<Appointment> calendarAppointments,
            bool addDescription, bool addReminder, bool addAttendees, bool attendeesToDescription,
            IDictionary<string, object> calendarSpecificData);

        /// <summary>
        /// </summary>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        Task<bool> ClearCalendar(IDictionary<string, object> calendarSpecificData);

        /// <summary>
        /// </summary>
        /// <param name="calendarSpecificData"></param>
        /// <returns></returns>
        Task<bool> ResetCalendarEntries(IDictionary<string, object> calendarSpecificData);
    }
}