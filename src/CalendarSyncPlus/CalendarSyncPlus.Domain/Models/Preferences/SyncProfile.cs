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
        private SyncDirectionEnum _syncDirection;
        private ServiceType _master;
        private ServiceType _source;
        private ServiceType _destination;
        private SyncModeEnum _syncMode;
        private SyncFrequency _syncFrequency;
        private ExchangeServerSettings _exchangeServerSettings;
        [NonSerialized]
        private bool _isLoaded;

        public SyncProfile()
        {
            OutlookSettings = new OutlookSettings();
            GoogleSettings  = new GoogleSettings();
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
    }
}
