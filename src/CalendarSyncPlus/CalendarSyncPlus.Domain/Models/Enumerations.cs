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

    public enum SensitivityEnum
    {
        None,
        Normal,
        Personal,
        Public,
        Private,
        Confidential,
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

    public enum FrequencyType
    {
        None,
        Secondly,
        Minutely,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public enum RecurrenceRestrictionType
    {
        /// <summary>
        /// Same as RestrictSecondly.
        /// </summary>
        Default,

        /// <summary>
        /// Does not restrict recurrence evaluation - WARNING: this may cause very slow performance!
        /// </summary>
        NoRestriction,

        /// <summary>
        /// Disallows use of the SECONDLY frequency for recurrence evaluation
        /// </summary>
        RestrictSecondly,

        /// <summary>
        /// Disallows use of the MINUTELY and SECONDLY frequencies for recurrence evaluation
        /// </summary>
        RestrictMinutely,

        /// <summary>
        /// Disallows use of the HOURLY, MINUTELY, and SECONDLY frequencies for recurrence evaluation
        /// </summary>
        RestrictHourly
    }

    public enum RecurrenceEvaluationModeType
    {
        /// <summary>
        /// Same as ThrowException.
        /// </summary>
        Default,

        /// <summary>
        /// Automatically adjusts the evaluation to the next-best frequency based on the restriction type.
        /// For example, if the restriction were IgnoreSeconds, and the frequency were SECONDLY, then
        /// this would cause the frequency to be adjusted to MINUTELY, the next closest thing.
        /// </summary>
        AdjustAutomatically,

        /// <summary>
        /// This will throw an exception if a recurrence rule is evaluated that does not meet the minimum
        /// restrictions.  For example, if the restriction were IgnoreSeconds, and a SECONDLY frequency
        /// were evaluated, an exception would be thrown.
        /// </summary>
        ThrowException
    }
}