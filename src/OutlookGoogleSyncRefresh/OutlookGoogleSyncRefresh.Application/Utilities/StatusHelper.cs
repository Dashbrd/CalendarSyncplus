using System;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    public enum SyncStateEnum
    {
        SyncStarted,
        SourceAppointmentRead,
        DestinationAppointmentRead,
        EntriesToDelete,
        EntriesToAdd,
        SyncSuccess,
        SyncFailed,
    }

    public static class StatusHelper
    {
        private const string BreakConstant = "-------------------------------------------------------";

        public static string GetMessage(SyncStateEnum syncStateEnum, params object[] values)
        {
            string message = string.Empty;
            switch (syncStateEnum)
            {
                case SyncStateEnum.SyncStarted:
                    message = BreakConstant + Environment.NewLine + "Sync started : {0}";
                    break;
                case SyncStateEnum.SourceAppointmentRead:
                    message = "Outlook entries read : {0}";
                    break;
                case SyncStateEnum.DestinationAppointmentRead:
                    message = "Google entries read : {0}";
                    break;
                case SyncStateEnum.EntriesToDelete:
                    message = "Google entries to delete : {0}";
                    break;
                case SyncStateEnum.EntriesToAdd:
                    message = "Google entries to add : {0}";
                    break;
                case SyncStateEnum.SyncSuccess:
                    message = "Sync completed" + Environment.NewLine + BreakConstant;
                    break;
                case SyncStateEnum.SyncFailed:
                    message = "Sync failed : {0}" + Environment.NewLine + BreakConstant;
                    break;
            }
            return string.Format(message, values);
        }
    }
}