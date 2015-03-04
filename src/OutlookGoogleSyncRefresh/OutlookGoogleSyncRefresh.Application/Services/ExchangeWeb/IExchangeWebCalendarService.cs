using System.Collections.Generic;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.ExchangeWeb
{
    public interface IExchangeWebCalendarService
    {
        List<Appointment> GetAppointmentsAsync(int daysInPast, int daysInFuture, string profileName,
            OutlookCalendar outlookCalendar);

        List<OutlookCalendar> GetCalendarsAsync();
    }
}