using System;
using System.Waf.Foundation;
using System.Xml.Serialization;
using CalendarSyncPlus.Common.MetaData;

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
    public class SyncProfile : Model
    {
        private bool _isDefault;
        private bool _isSyncEnabled;
        private DateTime? _lastSync;
        private string _name;
        private DateTime? _nextSync;
        private GoogleSettings _googleSettings;
        private OutlookSettings _outlookSettings;

        public SyncProfile()
        {
            OutlookSettings = new OutlookSettings();
            GoogleSettings  = new GoogleSettings();
        }

        /// <summary>
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// </summary>
        public bool IsSyncEnabled
        {
            get { return _isSyncEnabled; }
            set { SetProperty(ref _isSyncEnabled, value); }
        }

        /// <summary>
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
            set { SetProperty(ref _isDefault, value); }
        }

        /// <summary>
        /// </summary>
        public DateTime? LastSync
        {
            get { return _lastSync; }
            set { SetProperty(ref _lastSync, value); }
        }

        /// <summary>
        /// </summary>
        public DateTime? NextSync
        {
            get { return _nextSync; }
            set { SetProperty(ref _nextSync, value); }
        }

        public GoogleSettings GoogleSettings
        {
            get { return _googleSettings; }
            set { _googleSettings = value; }
        }

        /// <summary>
        /// </summary>
        public OutlookSettings OutlookSettings
        {
            get { return _outlookSettings; }
            set { _outlookSettings = value; }
        }

        /// <summary>
        ///     To be implemented in future
        /// </summary>
        public ExchangeServerSettings ExchangeServerSettings { get; set; }


        public SyncFrequency SyncFrequency { get; set; }
        public SyncDirectionEnum SyncDirection { get; set; }
        public ServiceType Master { get; set; }
        public ServiceType Source { get; set; }
        public ServiceType Destination { get; set; }
        public SyncModeEnum SyncMode { get; set; }

        
        /// <summary>
        /// </summary>
        public void SetSourceDestTypes()
        {
            if (SyncDirection == SyncDirectionEnum.OutlookGoogleOneWay)
            {
                Source =
                    OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                        ? ServiceType.EWS
                        : ServiceType.OutlookDesktop;
                Destination = ServiceType.Google;
            }
            else
            {
                Source = ServiceType.Google;
                Destination =
                    OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                        ? ServiceType.EWS
                        : ServiceType.OutlookDesktop;
            }

            if (SyncDirection == SyncDirectionEnum.OutlookGoogleTwoWay)
            {
                SyncMode = SyncModeEnum.TwoWay;
                if (Master == ServiceType.OutlookDesktop)
                {
                    Source =
                        OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                            ? ServiceType.EWS
                            : ServiceType.OutlookDesktop;
                    Destination = ServiceType.Google;
                }
                else
                {
                    Source = ServiceType.Google;
                    Destination =
                        OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.ExchangeWebServices)
                            ? ServiceType.EWS
                            : ServiceType.OutlookDesktop;
                }
            }
            else
            {
                SyncMode = SyncModeEnum.OneWay;
            }
        }
    }
}
