using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.ExchangeWebServices.ExchangeWeb
{
    public interface IExchangeWebCalendarService : ICalendarService
    {
        List<Appointment> GetAppointmentsAsync(int daysInPast, int daysInFuture, string profileName,
            OutlookCalendar outlookCalendar);

        List<OutlookCalendar> GetCalendarsAsync();
    }
}