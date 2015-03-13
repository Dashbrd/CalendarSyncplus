using System;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    [Flags]
    public enum OutlookOptionsEnum
    {
        None = 0,
        DefaultProfile = 1,
        AlternateProfile = 2,
        DefaultCalendar = 4,
        AlternateCalendar = 8,
        ExchangeWebServices = 16,
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
    }

    public enum CalendarSyncDirectionEnum
    {
        OutlookGoogleOneWay = 0,
        OutlookGoogleOneWayToSource = 1,
        OutlookGoogleTwoWay = 2,
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