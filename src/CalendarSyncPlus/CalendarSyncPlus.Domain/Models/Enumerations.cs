using System;

namespace CalendarSyncPlus.Domain.Models
{
    public enum AnnouncementStateEnum
    {
        New,
        Read,
        UnRead
    }

    public enum ProxyType
    {
        Auto = 0,
        NoProxy,
        ProxyWithAuth
    }

    public enum SyncRangeTypeEnum
    {
        SyncRangeInDays = 0,
        SyncFixedDateRange = 1,
        SyncEntireCalendar = 2
    }

    public enum MeetingResponseStatusEnum
    {
        None,
        NotResponded,
        Organizer,
        Accepted,
        Declined,
        Tentative
    }

    public enum MeetingStatusEnum
    {
        Meeting,
        MeetingCancelled,
        MeetingReceived,
        MeetingReceivedAndCanceled,
        NonMeeting
    }

    [Flags]
    public enum OutlookOptionsEnum
    {
        None,
        OutlookDesktop = 1,
        ExchangeWebServices = 2,
        DefaultProfile = 4,
        AlternateProfile = 8,
        DefaultMailBoxCalendar = 16,
        AlternateMailBoxCalendar = 32,
    }

    [Flags]
    public enum CalendarEntryOptionsEnum
    {
        None = 0,
        Description = 1,
        Attendees = 2,
        AttendeesToDescription = 4,
        Reminders = 8,
        Attachments = 16,
        AsAppointments = 32
    }

    public enum SyncDirectionEnum
    {
        OutlookGoogleOneWay = 0,
        OutlookGoogleOneWayToSource = 1,
        OutlookGoogleTwoWay = 2
    }

    public enum SyncModeEnum
    {
        OneWay,
        TwoWay
    }

    public enum BusyStatusEnum
    {
        Busy,
        Free,
        OutOfOffice,
        Tentative
    }
}