#region Imports

using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;

#endregion

namespace CalendarSyncPlus.Domain.Models.Preferences
{   
    [DataContract]
    public class ExchangeServerSettings : Model
    {
        #region Fields
        private string _domain;
        private string _exchangeServerUrl;
        private string _exchangeVersion;
        private string _password;
        private string _emailId;
        private bool _usingCorporateNetwork;
        private bool _autoDetectExchangeServer;
        private EWSCalendar _exchangeCalendar;

        #endregion

        #region Properties
        [DataMember]
        public bool AutoDetectExchangeServer
        {
            get { return _autoDetectExchangeServer; }
            set { SetProperty(ref _autoDetectExchangeServer, value); }
        }
        [DataMember]
        public string ExchangeVersion
        {
            get { return _exchangeVersion; }
            set { SetProperty(ref _exchangeVersion, value); }
        }
        [DataMember]
        public string EmailId
        {
            get { return _emailId; }
            set { SetProperty(ref _emailId, value); }
        }
        [DataMember]
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }
        [DataMember]
        public string ExchangeServerUrl
        {
            get { return _exchangeServerUrl; }
            set { SetProperty(ref _exchangeServerUrl, value); }
        }
        [DataMember]
        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value); }
        }
        [DataMember]
        public bool UsingCorporateNetwork
        {
            get { return _usingCorporateNetwork; }
            set { SetProperty(ref _usingCorporateNetwork, value); }
        }
        [DataMember]
        public EWSCalendar ExchangeCalendar
        {
            get { return _exchangeCalendar; }
            set { SetProperty(ref _exchangeCalendar, value); }
        }

        #endregion
    }
}