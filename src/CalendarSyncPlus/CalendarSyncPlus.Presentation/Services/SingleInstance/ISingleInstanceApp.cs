#region Imports

using System.Collections.Generic;

#endregion

namespace CalendarSyncPlus.Presentation.Services.SingleInstance
{
    public interface ISingleInstanceApp
    {
        bool SignalExternalCommandLineArgs(IList<string> args);
    }
}