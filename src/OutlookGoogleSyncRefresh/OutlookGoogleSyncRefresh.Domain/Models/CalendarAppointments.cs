using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class CalendarAppointments : List<Appointment>
    {
        public string CalendarId { get; set; }
    }
}
