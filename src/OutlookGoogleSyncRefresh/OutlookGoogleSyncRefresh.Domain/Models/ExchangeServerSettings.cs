using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    public class ExchangeServerSettings
    {
        public string ExchangeVersion { get; set; }

        public bool AutoDetectExchangeServer { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string ExchangeServerUrl { get; set; }
    }
}
