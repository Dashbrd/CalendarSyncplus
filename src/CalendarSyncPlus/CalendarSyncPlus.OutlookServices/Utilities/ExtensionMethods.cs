using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using Microsoft.Office.Interop.Outlook;

namespace CalendarSyncPlus.OutlookServices.Utilities
{
    public static class ExtensionMethods
    {

        public static OlBusyStatus GetOutlookBusyStatus(this Appointment calendarAppointment)
        {
            if (calendarAppointment.BusyStatus == BusyStatusEnum.Busy)
            {
                return OlBusyStatus.olBusy;
            }
            if (calendarAppointment.BusyStatus == BusyStatusEnum.Free)
            {
                return OlBusyStatus.olFree;
            }
            if (calendarAppointment.BusyStatus == BusyStatusEnum.OutOfOffice)
            {
                return OlBusyStatus.olOutOfOffice;
            }
            if (calendarAppointment.BusyStatus == BusyStatusEnum.Tentative)
            {
                return OlBusyStatus.olTentative;
            }
            return OlBusyStatus.olFree;
        }

        public static void SetBusyStatus(this Appointment calendarAppointment, OlBusyStatus busyStatus)
        {
            if (busyStatus == OlBusyStatus.olBusy)
            {
                calendarAppointment.BusyStatus = BusyStatusEnum.Busy;
            }
            else if (busyStatus == OlBusyStatus.olFree)
            {
                calendarAppointment.BusyStatus = BusyStatusEnum.Free;
            }
            else if (busyStatus == OlBusyStatus.olOutOfOffice)
            {
                calendarAppointment.BusyStatus = BusyStatusEnum.OutOfOffice;
            }
            else if (busyStatus == OlBusyStatus.olTentative)
            {
                calendarAppointment.BusyStatus = BusyStatusEnum.Tentative;
            }
        }

    }
}
