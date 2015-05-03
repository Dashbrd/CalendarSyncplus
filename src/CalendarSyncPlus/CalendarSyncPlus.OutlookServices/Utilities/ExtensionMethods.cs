using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Utilities;
using Microsoft.Office.Interop.Outlook;
using Recipient = Microsoft.Office.Interop.Outlook.Recipient;

namespace CalendarSyncPlus.OutlookServices.Utilities
{
    public static class ExtensionMethods
    {
        public static bool GetEmailFromName(this Recipient recipient, out string name, out string email)
        {
            name = null;
            email = null;
            try
            {
                if (!string.IsNullOrEmpty(recipient.Name) && recipient.Name.Contains("<") && recipient.Name.Contains(">"))
                {
                    int startIndex = recipient.Name.LastIndexOf("<");
                    int endIndex = recipient.Name.LastIndexOf(">");
                    if (startIndex < endIndex)
                    {
                        email = recipient.Name.Substring(startIndex + 1, endIndex - startIndex - 1);
                        if (email.IsValidEmailAddress())
                        {
                            name = recipient.Name.Substring(0, startIndex);
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
        public static MeetingResponseStatusEnum GetMeetingResponseStatus(this Recipient recipient)
        {
            switch (recipient.MeetingResponseStatus)
            {
                    case OlResponseStatus.olResponseAccepted:
                     return MeetingResponseStatusEnum.Accepted;
                    case OlResponseStatus.olResponseDeclined:
                     return MeetingResponseStatusEnum.Declined;
                    case OlResponseStatus.olResponseNone:
                     return MeetingResponseStatusEnum.None;
                    case OlResponseStatus.olResponseNotResponded:
                     return MeetingResponseStatusEnum.NotResponded;
                    case OlResponseStatus.olResponseOrganized:
                     return MeetingResponseStatusEnum.Organizer;
                    case OlResponseStatus.olResponseTentative:
                     return MeetingResponseStatusEnum.Tentative;
            }
            return MeetingResponseStatusEnum.None;
        }
        public static MeetingStatusEnum GetMeetingStatus(this AppointmentItem appointment)
        {
            switch (appointment.MeetingStatus)
            {
                case OlMeetingStatus.olMeeting:
                    return MeetingStatusEnum.Meeting;
                case OlMeetingStatus.olMeetingCanceled:
                    return MeetingStatusEnum.MeetingCancelled;
                case OlMeetingStatus.olMeetingReceived:
                    return MeetingStatusEnum.MeetingReceived;
                case OlMeetingStatus.olMeetingReceivedAndCanceled:
                    return MeetingStatusEnum.MeetingReceivedAndCanceled;
                case OlMeetingStatus.olNonMeeting:
                    return MeetingStatusEnum.NonMeeting;
            }
            return MeetingStatusEnum.Meeting;
        }

        public static OlMeetingStatus GetMeetingStatus(this Appointment appointment)
        {
            switch (appointment.MeetingStatus)
            {
                case MeetingStatusEnum.Meeting:
                    return OlMeetingStatus.olMeeting;
                case MeetingStatusEnum.MeetingCancelled:
                    return OlMeetingStatus.olMeetingCanceled;
                case MeetingStatusEnum.MeetingReceived:
                    return OlMeetingStatus.olMeetingReceived;
                case MeetingStatusEnum.MeetingReceivedAndCanceled:
                    return OlMeetingStatus.olMeetingReceivedAndCanceled;
                case MeetingStatusEnum.NonMeeting:
                    return OlMeetingStatus.olNonMeeting;
            }
            return OlMeetingStatus.olMeeting;
        }

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
