using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Services.Wrappers
{
    public class CalendarAppointments : List<Appointment>
    {
        public string CalendarId { get; set; }

        public bool IsSuccess { get; set; }
    }
}