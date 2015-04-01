#region Imports

using System.Waf.Foundation;

#endregion

namespace CalendarSyncPlus.Domain.Models
{
    public class ExchangeServerSettings : Model
    {
        #region Fields

        private string _domain;
        private string _exchangeServerUrl;
        private string _exchangeVersion;
        private string _password;
        private string _username;
        private bool _usingCorporateNetwork;

        #endregion

        #region Properties

        public string ExchangeVersion
        {
            get { return _exchangeVersion; }
            set { SetProperty(ref _exchangeVersion, value); }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
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