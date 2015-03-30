using System.Collections.Generic;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    public enum SyncStateEnum
    {
        LogSeparator,
        Line,
        SyncStarted,
        Profile,
        SourceAppointmentsReading,
        SourceAppointmentsRead,
        SourceAppointmentsReadFailed,
        DestAppointmentReading,
        DestAppointmentRead,
        DestAppointmentReadFailed,
        ReadingEntriesToDelete,
        EntriesToDelete,
        DeletingEntries,
        SkipDelete,
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

    public class StatusHelper
    {
        private const string LineConstant =
            "------------------------------------------------------------------------------";

        private const string LogSeparatorConstant =
            "***************************************************************************";

        private static readonly Dictionary<SyncStateEnum, string> StatusDictionary =
            new Dictionary<SyncStateEnum, string>();

        static StatusHelper()
        {
            StatusDictionary.Add(SyncStateEnum.LogSeparator, LogSeparatorConstant);
            StatusDictionary.Add(SyncStateEnum.Line, LineConstant);
            StatusDictionary.Add(SyncStateEnum.SyncStarted, "Sync started : {0}");
            StatusDictionary.Add(SyncStateEnum.Profile, "Profile : {0}");
            StatusDictionary.Add(SyncStateEnum.SourceAppointmentsReading, "Reading {0} calendar...");
            StatusDictionary.Add(SyncStateEnum.SourceAppointmentsRead, "{0} entries read : {1}");
            StatusDictionary.Add(SyncStateEnum.SourceAppointmentsReadFailed, "Read failed.");
            StatusDictionary.Add(SyncStateEnum.DestAppointmentReading, "Reading {0} calendar...");
            StatusDictionary.Add(SyncStateEnum.DestAppointmentRead, "{0} entries read : {1}");
            StatusDictionary.Add(SyncStateEnum.DestAppointmentReadFailed, "Read failed.");
            StatusDictionary.Add(SyncStateEnum.ReadingEntriesToDelete, "Getting entries to be deleted...");
            StatusDictionary.Add(SyncStateEnum.EntriesToDelete, "Found {0} entries to delete");
            StatusDictionary.Add(SyncStateEnum.SkipDelete, "Skipping Delete.");
            StatusDictionary.Add(SyncStateEnum.DeletingEntries, "Deleting {0} entries...");
            StatusDictionary.Add(SyncStateEnum.DeletingEntriesComplete, "Delete Complete.");
            StatusDictionary.Add(SyncStateEnum.DeletingEntriesFailed, "Delete Failed.");
            StatusDictionary.Add(SyncStateEnum.ReadingEntriesToAdd, "Getting entries to be added...");
            StatusDictionary.Add(SyncStateEnum.EntriesToAdd, "Found {0} entries to add");
            StatusDictionary.Add(SyncStateEnum.AddingEntries, "Adding {0} entries...");
            StatusDictionary.Add(SyncStateEnum.AddEntriesComplete, "Add Complete.");
            StatusDictionary.Add(SyncStateEnum.AddEntriesFailed, "Add Failed.");
            StatusDictionary.Add(SyncStateEnum.SyncSuccess, "Sync completed");
            StatusDictionary.Add(SyncStateEnum.SyncFailed, "Sync failed : {0}");
        }

        public static string GetMessage(SyncStateEnum syncStateEnum, params object[] values)
        {
            string message = string.Empty;
            if (StatusDictionary.ContainsKey(syncStateEnum))
            {
                message = StatusDictionary[syncStateEnum];
            }
            if (values == null)
            {
                return message;
            }
            return string.Format(message, values);
        }
    }
}