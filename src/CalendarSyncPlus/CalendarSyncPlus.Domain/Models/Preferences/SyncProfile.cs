using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using CalendarSyncPlus.Common.MetaData;
using Newtonsoft.Json;

namespace CalendarSyncPlus.Domain.Models.Preferences
{  
    [DataContract]
    public class SyncProfile : Model
    {
        private ServiceType _destination;
        private ExchangeServerSettings _exchangeServerSettings;
        private GoogleSettings _googleSettings;
        private bool _isDefault;

        private bool _isLoaded;
        private bool _isSyncEnabled;
        private DateTime? _lastSync;
        private ServiceType _master;
        private string _name;
        private DateTime? _nextSync;
        private OutlookSettings _outlookSettings;
        private ServiceType _source;
        private SyncDirectionEnum _syncDirection;
        private SyncFrequency _syncFrequency;
        private SyncModeEnum _syncMode;

        public SyncProfile()
        {
            OutlookSettings = new OutlookSettings();
            GoogleSettings = new GoogleSettings();
        }

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
                Master = ServiceType.OutlookDesktop;
            }
        }

        #region Properties
        [DataMember]
        /// <summary>
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public bool IsSyncEnabled
        {
            get { return _isSyncEnabled; }
            set { SetProperty(ref _isSyncEnabled, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
            set { SetProperty(ref _isDefault, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public DateTime? LastSync
        {
            get { return _lastSync; }
            set { SetProperty(ref _lastSync, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public DateTime? NextSync
        {
            get { return _nextSync; }
            set { SetProperty(ref _nextSync, value); }
        }
        [DataMember]
        public GoogleSettings GoogleSettings
        {
            get { return _googleSettings; }
            set { SetProperty(ref _googleSettings, value); }
        }
        [DataMember]
        /// <summary>
        /// </summary>
        public OutlookSettings OutlookSettings
        {
            get { return _outlookSettings; }
            set { SetProperty(ref _outlookSettings, value); }
        }
        [DataMember]
        /// <summary>
        ///     To be implemented in future
        /// </summary>
        public ExchangeServerSettings ExchangeServerSettings
        {
            get { return _exchangeServerSettings; }
            set { SetProperty(ref _exchangeServerSettings, value); }
        }

        [DataMember]
        public SyncFrequency SyncFrequency
        {
            get { return _syncFrequency; }
            set { SetProperty(ref _syncFrequency, value); }
        }
        [DataMember]
        public SyncDirectionEnum SyncDirection
        {
            get { return _syncDirection; }
            set { SetProperty(ref _syncDirection, value); }
        }
        [DataMember]
        public ServiceType Master
        {
            get { return _master; }
            set { SetProperty(ref _master, value); }
        }
        [DataMember]
        public ServiceType Source
        {
            get { return _source; }
            set { SetProperty(ref _source, value); }
        }
        [DataMember]
        public ServiceType Destination
        {
            get { return _destination; }
            set { SetProperty(ref _destination, value); }
        }
        [DataMember]
        public SyncModeEnum SyncMode
        {
            get { return _syncMode; }
            set { SetProperty(ref _syncMode, value); }
        }

        [DataMember]
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        #endregion
    }
}