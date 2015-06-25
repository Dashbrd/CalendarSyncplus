
#region Imports

using System.Waf.Foundation;

#endregion

namespace CalendarSyncPlus.Domain.Models.Preferences
{
    public class ProxySetting : Model
    {
        #region Fields

        private bool _bypassOnLocal = true;
        private string _domain;
        private string _password;
        private int _port;
        private string _proxyAddress;
        private ProxyType _proxyType;
        private bool _useDefaultCredentials;
        private string _userName;

        #endregion

        #region Properties

        public string ProxyAddress
        {
            get { return _proxyAddress; }
            set { SetProperty(ref _proxyAddress, value); }
        }

        public int Port
        {
            get { return _port; }
            set { SetProperty(ref _port, value); }
        }


        public ProxyType ProxyType
        {
            get { return _proxyType; }
            set { SetProperty(ref _proxyType, value); }
        }

        public bool BypassOnLocal
        {
            get { return _bypassOnLocal; }
            set { SetProperty(ref _bypassOnLocal, value); }
        }

        public bool UseDefaultCredentials
        {
            get { return _useDefaultCredentials; }
            set { SetProperty(ref _useDefaultCredentials, value); }
        }

        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public string Domain
        {
            get { return _domain; }
            set { SetProperty(ref _domain, value); }
        }

        #endregion
    }
}