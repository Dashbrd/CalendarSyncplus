using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using Google.Apis.Calendar.v3.Data;

namespace CalendarSyncPlus.GoogleServices.Calendar
{
    public class RecurrenceHelper
    {
        public static void HandleRecurrence(Event googleEvent, Appointment appointment)
        {
            if(!googleEvent.Recurrence.Any())
                return;
            RecurrencePattern pattern = new RecurrencePattern();
            pattern.FrequencyType = Enum.Parse(typeof(FrequencyType), googleEvent.Recurrence[0].Split(new [] {"FREQ="}, StringSplitOptions.RemoveEmptyEntries).Last())};

        }
    }
}
