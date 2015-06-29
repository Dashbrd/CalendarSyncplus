using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;

namespace CalendarSyncPlus.ExchangeWebServices.Calendar
{
    public interface IExchangeWebCalendarService : ICalendarService
    {
        List<Appointment> GetAppointmentsAsync(int daysInPast, int daysInFuture, string profileName,
            EWSCalendar outlookCalendar);

        List<EWSCalendar> GetCalendarsAsync(int maxFoldersToRetrive);
    }
}