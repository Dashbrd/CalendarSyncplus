using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface IApplicationUpdateService
    {
        bool IsNewVersionAvailable();

        string GetNewAvailableVersion();

        Uri GetDownloadUri();
    }
}
