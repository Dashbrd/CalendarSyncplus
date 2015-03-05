using System;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    public enum SyncStateEnum
    {
        NewLog,
        Line,
        SyncStarted,
        OutlookAppointmentsReading,
        OutlookAppointmentsRead,
        GoogleAppointmentReading,
        GoogleAppointmentRead,
        ReadingEntriesToDelete,
        EntriesToDelete,
        DeletingEntries,
        DeletingEntriesComplete,
        DeletingEntriesFailed,
        ReadingEntriesToAdd,
        EntriesToAdd,
        AddingEntries,
        AddEntriesComplete,
        AddEntriesFailed,
        SyncSuccess,
        SyncFailed,
    }

    public static class StatusHelper
    {
        private const string BreakConstant =  "---------------------------------------------------------------------------";
        private const string NewLogConstant = "***************************************************************************";
        public static string GetMessage(SyncStateEnum syncStateEnum, params object[] values)
        {
            string message = string.Empty;
            switch (syncStateEnum)
            {
                case SyncStateEnum.NewLog:
                    message = NewLogConstant;
                    break;
                case SyncStateEnum.Line:
                    message = BreakConstant;
                    break;
                case SyncStateEnum.SyncStarted:
                    message = "Sync started : {0}";
                    break;
                case SyncStateEnum.OutlookAppointmentsReading:
                    message = "Reading outlook calendar...";
                    break;
                case SyncStateEnum.OutlookAppointmentsRead:
                    message = "Outlook entries read : {0}";
                    break;
                case SyncStateEnum.GoogleAppointmentReading:
                    message = "Reading Google calendar...";
                    break;
                case SyncStateEnum.GoogleAppointmentRead:
                    message = "Google entries read : {0}";
                    break;
                case SyncStateEnum.ReadingEntriesToDelete:
                    message = "Getting Google entries to be deleted...";
                    break;
                case SyncStateEnum.EntriesToDelete:
                    message = "Found {0} Google entries to delete";
                    break;
                case SyncStateEnum.DeletingEntries:
                    message = "Deleting Google entries...";
                    break;
                case SyncStateEnum.DeletingEntriesComplete:
                    message = "Delete Complete.";
                    break;
                case SyncStateEnum.DeletingEntriesFailed:
                    message = "Delete Failed.";
                    break;
                case SyncStateEnum.ReadingEntriesToAdd:
                    message = "Getting Outlook entries to be added to Google...";
                    break;
                case SyncStateEnum.EntriesToAdd:
                    message = "Found {0} Outlook entries to add";
                    break;
                case SyncStateEnum.AddingEntries:
                    message = "Adding Google entries...";
                    break;
                case SyncStateEnum.AddEntriesComplete:
                    message = "Add Complete.";
                    break;
                case SyncStateEnum.AddEntriesFailed:
                    message = "Add Failed.";
                    break;
                case SyncStateEnum.SyncSuccess:
                    message = "Sync completed";
                    break;
                case SyncStateEnum.SyncFailed:
                    message = "Sync failed : {0}";
                    break;
            }
            if (values == null)
                return message;
            return string.Format(message, values);
        }
    }
}