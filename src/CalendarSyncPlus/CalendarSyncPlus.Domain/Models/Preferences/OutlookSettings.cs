using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    [DataContract]
    public class OutlookSettings : Model
    {
        private OutlookFolder _outlookFolder;
        private OutlookMailBox _outlookMailBox;
        private OutlookOptionsEnum _outlookOptions;
        private string _outlookProfileName;
        private bool _setOrganizer;
        [DataMember]
        public OutlookOptionsEnum OutlookOptions
        {
            get { return _outlookOptions; }
            set
            {
                value = ValidateOptions(value);
                SetProperty(ref _outlookOptions, value);
            }
        }
        [DataMember]

        public string OutlookProfileName
        {
            get { return _outlookProfileName; }
            set { SetProperty(ref _outlookProfileName, value); }
        }
        [DataMember]
        public OutlookMailBox OutlookMailBox
        {
            get { return _outlookMailBox; }
            set { SetProperty(ref _outlookMailBox, value); }
        }
        [DataMember]
        public OutlookFolder OutlookFolder
        {
            get { return _outlookFolder; }
            set { SetProperty(ref _outlookFolder, value); }
        }
        [DataMember]
        public bool SetOrganizer
        {
            get { return _setOrganizer; }
            set { SetProperty(ref _setOrganizer, value); }
        }

        private OutlookOptionsEnum ValidateOptions(OutlookOptionsEnum value)
        {
            switch (value)
            {
                case OutlookOptionsEnum.ExchangeWebServices:
                    return _outlookOptions & ~OutlookOptionsEnum.OutlookDesktop | value;
                case OutlookOptionsEnum.OutlookDesktop:
                    return _outlookOptions & ~OutlookOptionsEnum.ExchangeWebServices | value;
                case OutlookOptionsEnum.DefaultProfile:
                    return _outlookOptions & ~OutlookOptionsEnum.AlternateProfile | value;
                case OutlookOptionsEnum.AlternateProfile:
                    return _outlookOptions & ~OutlookOptionsEnum.DefaultProfile | value;
                case OutlookOptionsEnum.DefaultMailBoxCalendar:
                    return _outlookOptions & ~OutlookOptionsEnum.AlternateMailBoxCalendar | value;
                case OutlookOptionsEnum.AlternateMailBoxCalendar:
                    return _outlookOptions & ~OutlookOptionsEnum.DefaultMailBoxCalendar | value;
            }
            return _outlookOptions | value;
        }
    }
}