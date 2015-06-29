using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.OutlookServices.Wrappers
{
    public class OutlookAppointmentsWrapper
    {
        public List<Appointment> Appointments { get; set; }
        public bool WaitForApplicationQuit { get; set; }
        public bool Success { get; set; }
    }
}