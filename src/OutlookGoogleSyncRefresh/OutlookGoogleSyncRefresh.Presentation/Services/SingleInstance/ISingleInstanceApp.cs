#region Imports

using System.Collections.Generic;

#endregion

namespace OutlookGoogleSyncRefresh.Presentation.Services.SingleInstance
{
    public interface ISingleInstanceApp
    {
        bool SignalExternalCommandLineArgs(IList<string> args);
    }
}