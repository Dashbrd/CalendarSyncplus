using System.Collections.Generic;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Wrappers
{
    public class AppointmentListWrapper
    {
        public List<Appointment> Appointments { get; set; }

        public bool WaitForApplicationQuit { get; set; }

        public bool Success { get; set; }
    }
}