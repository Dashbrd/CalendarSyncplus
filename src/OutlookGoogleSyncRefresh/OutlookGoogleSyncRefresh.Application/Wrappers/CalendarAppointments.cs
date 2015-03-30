using System.Collections.Generic;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Wrappers
{
    public class CalendarAppointments : List<Appointment>
    {
        public string CalendarId { get; set; }

        public bool IsSuccess { get; set; }
    }
}