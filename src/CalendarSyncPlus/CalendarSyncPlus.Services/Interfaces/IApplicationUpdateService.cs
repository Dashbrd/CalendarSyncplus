using System;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface IApplicationUpdateService
    {
        string GetLatestReleaseFromServer(bool includeAlpha);

        bool IsNewVersionAvailable();

        string GetNewAvailableVersion();

        Uri GetDownloadUri();
    }
}