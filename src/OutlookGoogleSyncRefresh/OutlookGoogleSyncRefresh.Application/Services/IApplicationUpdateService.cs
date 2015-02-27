using System;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface IApplicationUpdateService
    {
        string GetLatestReleaseFromServer();

        bool IsNewVersionAvailable();

        string GetNewAvailableVersion();

        Uri GetDownloadUri();
    }
}