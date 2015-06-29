using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Domain.Wrappers
{
    public class AppointmentsWrapper : List<Appointment>
    {
        public string CalendarId { get; set; }
        public bool IsSuccess { get; set; }
    }
}