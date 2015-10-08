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
                if (!string.IsNullOrEmpty(recipient.Name) && recipient.Name.Contains("<") &&
                    recipient.Name.Contains(">"))
                {
                    var startIndex = recipient.Name.LastIndexOf("<");
                    var endIndex = recipient.Name.LastIndexOf(">");
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

        public static OlResponseStatus GetRecipientResponseStatus(this Attendee attendee)
        {
            switch (attendee.MeetingResponseStatus)
            {
                case MeetingResponseStatusEnum.Accepted:
                    return OlResponseStatus.olResponseAccepted;
                case MeetingResponseStatusEnum.Declined:
                    return OlResponseStatus.olResponseDeclined;
                case MeetingResponseStatusEnum.None:
                    return OlResponseStatus.olResponseNone;
                case MeetingResponseStatusEnum.NotResponded:
                    return OlResponseStatus.olResponseNotResponded;
                case MeetingResponseStatusEnum.Organizer:
                    return OlResponseStatus.olResponseOrganized;
                case MeetingResponseStatusEnum.Tentative:
                    return OlResponseStatus.olResponseTentative;
            }
            return OlResponseStatus.olResponseNone;
        }

        public static OlTaskStatus GetOlTaskStatus(this ReminderTask taskItem)
        {
            switch (taskItem.StatusEnum)
            {
                case TaskStatusEnum.TaskComplete:
                    return OlTaskStatus.olTaskComplete;
                case TaskStatusEnum.TaskDeferred:
                    return OlTaskStatus.olTaskDeferred;
                case TaskStatusEnum.TaskInProgress: 
                    return OlTaskStatus.olTaskInProgress;
                case TaskStatusEnum.TaskNotStarted:
                    return OlTaskStatus.olTaskNotStarted;
                case TaskStatusEnum.TaskWaiting:
                    return OlTaskStatus.olTaskWaiting;
            }
            return OlTaskStatus.olTaskNotStarted;
        }
        public static TaskStatusEnum GetTaskStatus(this TaskItem taskItem)
        {
            switch (taskItem.Status)
            {
                case OlTaskStatus.olTaskComplete:
                    return TaskStatusEnum.TaskComplete;
                case OlTaskStatus.olTaskDeferred:
                    return TaskStatusEnum.TaskDeferred;
                case OlTaskStatus.olTaskInProgress:
                    return TaskStatusEnum.TaskInProgress;
                case  OlTaskStatus.olTaskNotStarted:
                    return TaskStatusEnum.TaskNotStarted;
                case OlTaskStatus.olTaskWaiting:
                    return TaskStatusEnum.TaskWaiting;
            }
            return TaskStatusEnum.TaskNotStarted;
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


        public static SensitivityEnum GetAppointmentSensitivity(this AppointmentItem appointmentItem)
        {
            switch (appointmentItem.Sensitivity)
            {
                case OlSensitivity.olConfidential:
                    return SensitivityEnum.Confidential;
                case OlSensitivity.olPersonal:
                    return SensitivityEnum.Personal;
                case OlSensitivity.olPrivate:
                    return SensitivityEnum.Private;
            }
            return SensitivityEnum.Normal;
        }

        public static OlSensitivity GetAppointmentSensitivity(this Appointment appointment)
        {
            switch (appointment.Privacy)
            {
                case SensitivityEnum.Confidential:
                    return OlSensitivity.olConfidential;
                case SensitivityEnum.Personal:
                    return OlSensitivity.olPersonal;
                case SensitivityEnum.Private:
                    return OlSensitivity.olPrivate;
            }
            return OlSensitivity.olNormal;
        }
    }
}