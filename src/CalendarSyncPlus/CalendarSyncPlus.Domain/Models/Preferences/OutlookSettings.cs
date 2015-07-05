using System;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class OutlookSettings : Model
    {
        private OutlookOptionsEnum _outlookOptions;
        private string _outlookProfileName;
        private OutlookMailBox _outlookMailBox;
        private OutlookFolder _outlookCalendar;

        public OutlookOptionsEnum OutlookOptions
        {
            get { return _outlookOptions; }
            set
            {
                value = ValidateOptions(value);
                SetProperty(ref _outlookOptions, value);
            }
        }
        
        public string OutlookProfileName
        {
            get { return _outlookProfileName; }
            set { SetProperty(ref _outlookProfileName, value); }
        }

        public OutlookMailBox OutlookMailBox
        {
            get { return _outlookMailBox; }
            set { SetProperty(ref _outlookMailBox, value); }
        }

        public OutlookFolder OutlookCalendar
        {
            get { return _outlookCalendar; }
            set { SetProperty(ref _outlookCalendar, value); }
        }

        private OutlookOptionsEnum ValidateOptions(OutlookOptionsEnum value)
        {
            if (value == OutlookOptionsEnum.ExchangeWebServices)
            {
                return value;
            }

            if (value == OutlookOptionsEnum.DefaultProfile)
            {
                if (value.HasFlag(OutlookOptionsEnum.AlternateProfile))
                {
                    value &= ~OutlookOptionsEnum.AlternateProfile;
                }
                return value | OutlookOptionsEnum.DefaultProfile;
            }

            if (value == OutlookOptionsEnum.DefaultMailBoxCalendar)
            {
                if (value.HasFlag(OutlookOptionsEnum.AlternateMailBoxCalendar))
                {
                    value &= ~OutlookOptionsEnum.AlternateMailBoxCalendar;
                }
                return value | OutlookOptionsEnum.DefaultMailBoxCalendar;
            }
            return value;
        }
    }
}