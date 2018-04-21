using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace CalendarSyncPlus.Domain.Models
{
    public enum AnnouncementStateEnum
    {
        New,
        Read,
        UnRead
    }

    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProxyType
    {
        [EnumMember(Value = "0")]
        Auto = 0,
        [EnumMember(Value = "1")]
        NoProxy,
        [EnumMember(Value = "2")]
        ProxyWithAuth
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SyncRangeTypeEnum
    {
        [EnumMember(Value = "0")]
        SyncRangeInDays = 0,
        [EnumMember(Value = "1")]
        SyncFixedDateRange = 1,
        [EnumMember(Value = "2")]
        SyncEntireCalendar = 2
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MeetingResponseStatusEnum
    {
        None,
        NotResponded,
        Organizer,
        Accepted,
        Declined,
        Tentative
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SensitivityEnum
    {
        None,
        Normal,
        Personal,
        Public,
        Private,
        Confidential
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MeetingStatusEnum
    {
        Meeting,
        MeetingCancelled,
        MeetingReceived,
        MeetingReceivedAndCanceled,
        NonMeeting
    }
    [DataContract]
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OutlookOptionsEnum
    {
        [EnumMember(Value = "0")]
        None,
        [EnumMember(Value = "1")]
        OutlookDesktop = 1,
        [EnumMember(Value = "2")]
        ExchangeWebServices = 2,
        [EnumMember(Value = "4")]
        DefaultProfile = 4,
        [EnumMember(Value = "8")]
        AlternateProfile = 8,
        [EnumMember(Value = "16")]
        DefaultMailBoxCalendar = 16,
        [EnumMember(Value = "32")]
        AlternateMailBoxCalendar = 32
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum CalendarEntryOptionsEnum
    {
        [EnumMember(Value = "0")]
        None = 0,
        [EnumMember(Value = "1")]
        Description = 1,
        [EnumMember(Value = "2")]
        Attendees = 2,
        [EnumMember(Value = "4")]
        AttendeesToDescription = 4,
        [EnumMember(Value = "8")]
        Reminders = 8,
        [EnumMember(Value = "16")]
        Attachments = 16,
        [EnumMember(Value = "32")]
        AsAppointments = 32
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SyncDirectionEnum
    {
        [EnumMember(Value = "1")]
        OutlookGoogleOneWay = 0,
        [EnumMember(Value = "2")]
        OutlookGoogleOneWayToSource = 1,
        [EnumMember(Value = "3")]
        OutlookGoogleTwoWay = 2
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SyncModeEnum
    {
        [EnumMember(Value = "1")]
        OneWay,
        [EnumMember(Value = "2")]
        TwoWay
    }
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BusyStatusEnum
    {
        Busy,
        Free,
        OutOfOffice,
        Tentative
    }
}