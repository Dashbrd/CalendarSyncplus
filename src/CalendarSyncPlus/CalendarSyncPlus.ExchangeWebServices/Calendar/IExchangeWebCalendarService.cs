using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using Appointment = CalendarSyncPlus.Domain.Models.Appointment;

namespace CalendarSyncPlus.ExchangeWebServices.Calendar
{
    public interface IExchangeWebCalendarService : ICalendarService
    {
        ExchangeService GetExchangeService(ExchangeServerSettings exchangeServerSettings);

        List<Appointment> GetAppointmentsAsync(int daysInPast, int daysInFuture, string profileName,
            EWSCalendar outlookCalendar);

        List<EWSCalendar> GetCalendarsAsync(int maxFoldersToRetrive, Dictionary<string, object> calendarSpecificData);
    }
}