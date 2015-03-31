using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Services.Wrappers
{
    public class AppointmentListWrapper
    {
        public List<Appointment> Appointments { get; set; }

        public bool WaitForApplicationQuit { get; set; }

        public bool Success { get; set; }
    }
}