#region Imports

using System;
using System.Waf.Foundation;

#endregion

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    [Serializable]
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

        #endregion

        #region Properties

        public bool AutoDetectExchangeServer
        {
            get { return _autoDetectExchangeServer; }
            set { SetProperty(ref _autoDetectExchangeServer, value); }
        }

        public string ExchangeVersion
        {
            get { return _exchangeVersion; }
            set { SetProperty(ref _exchangeVersion, value); }
        }

        public string EmailId
        {
            get { return _emailId; }
            set { SetProperty(ref _emailId, value); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public string ExchangeServerUrl
        {
            get { return _exchangeServerUrl; }
            set { SetProperty(ref _exchangeServerUrl, value); }
        }

        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value); }
        }

        public bool UsingCorporateNetwork
        {
            get { return _usingCorporateNetwork; }
            set { SetProperty(ref _usingCorporateNetwork, value); }
        }

        #endregion
    }
}