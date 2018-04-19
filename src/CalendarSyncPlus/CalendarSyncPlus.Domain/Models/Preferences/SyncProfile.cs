using System;
using System.Waf.Foundation;
using CalendarSyncPlus.Common.MetaData;
using Newtonsoft.Json;

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    public class SyncProfile : Model
    {
        [JsonProperty("destination")]
        private ServiceType _destination;
        [JsonProperty("exchangeSettings")]
        private ExchangeServerSettings _exchangeServerSettings;
        [JsonProperty("googleSettings")]
        private GoogleSettings _googleSettings;
        [JsonProperty("isDefault")]
        private bool _isDefault;

        private bool _isLoaded;
        [JsonProperty("isSyncEnabled")]
        private bool _isSyncEnabled;
        [JsonProperty("lastSync")]
        private DateTime? _lastSync;
        [JsonProperty("master")]
        private ServiceType _master;
        [JsonProperty("name")]
        private string _name;
        private DateTime? _nextSync;
        [JsonProperty("outlookSettings")]
        private OutlookSettings _outlookSettings;
        [JsonProperty("source")]
        private ServiceType _source;
        [JsonProperty("syncDirection")]
        private SyncDirectionEnum _syncDirection;
        [JsonProperty("syncFrequency")]
        private SyncFrequency _syncFrequency;
        [JsonProperty("syncMode")]
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
            set { SetProperty(ref _googleSettings, value); }
        }

        /// <summary>
        /// </summary>
        public OutlookSettings OutlookSettings
        {
            get { return _outlookSettings; }
            set { SetProperty(ref _outlookSettings, value); }
        }

        /// <summary>
        ///     To be implemented in future
        /// </summary>
        public ExchangeServerSettings ExchangeServerSettings
        {
            get { return _exchangeServerSettings; }
            set { SetProperty(ref _exchangeServerSettings, value); }
        }


        public SyncFrequency SyncFrequency
        {
            get { return _syncFrequency; }
            set { SetProperty(ref _syncFrequency, value); }
        }

        public SyncDirectionEnum SyncDirection
        {
            get { return _syncDirection; }
            set { SetProperty(ref _syncDirection, value); }
        }

        public ServiceType Master
        {
            get { return _master; }
            set { SetProperty(ref _master, value); }
        }

        public ServiceType Source
        {
            get { return _source; }
            set { SetProperty(ref _source, value); }
        }

        public ServiceType Destination
        {
            get { return _destination; }
            set { SetProperty(ref _destination, value); }
        }

        public SyncModeEnum SyncMode
        {
            get { return _syncMode; }
            set { SetProperty(ref _syncMode, value); }
        }


        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        #endregion
    }
}