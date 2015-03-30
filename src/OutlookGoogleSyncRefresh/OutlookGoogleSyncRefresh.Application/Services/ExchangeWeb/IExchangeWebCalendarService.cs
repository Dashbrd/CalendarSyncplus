using System.Collections.Generic;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.ExchangeWeb
{
    public interface IExchangeWebCalendarService : ICalendarService
    {
        List<Appointment> GetAppointmentsAsync(int daysInPast, int daysInFuture, string profileName,
            OutlookCalendar outlookCalendar);

        List<OutlookCalendar> GetCalendarsAsync();
    }
}