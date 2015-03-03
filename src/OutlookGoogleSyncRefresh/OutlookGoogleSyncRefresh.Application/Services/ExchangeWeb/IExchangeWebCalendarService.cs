using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Autodiscover;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Services.ExchangeWeb
{
    public interface IExchangeWebCalendarService
    {
        Task<List<Appointment>> GetAppointmentsAsync(int daysInPast, int daysInFuture, string profileName,
            OutlookCalendar outlookCalendar);

        Task<List<OutlookCalendar>> GetCalendarsAsync();

    }
}