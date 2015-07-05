#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Foundation;
using CalendarSyncPlus.Analytics.Interfaces;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Calendars.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using CalendarSyncPlus.SyncEngine.Helpers;
using CalendarSyncPlus.SyncEngine.Interfaces;
using log4net;

#endregion

namespace CalendarSyncPlus.Services.Calendars
{
    [Export(typeof(ICalendarUpdateService))]
    public class CalendarUpdateService : Model, ICalendarUpdateService
    {
        #region Constructors

        [ImportingConstructor]
        public CalendarUpdateService(ICalendarServiceFactory calendarServiceFactory,
            ICalendarSyncEngine calendarSyncEngine,
            ISyncAnalyticsService analyticsService,
            ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger.GetLogger(GetType());
            CalendarServiceFactory = calendarServiceFactory;
            CalendarSyncEngine = calendarSyncEngine;
            AnalyticsService = analyticsService;
        }

        #endregion

        #region Fields

        private Appointment _currentAppointment;
        private AppointmentsWrapper _destinationAppointments;
        private AppointmentsWrapper _sourceAppointments;
        private string _calendarSyncStatus;

        #endregion

        #region Properties

        public ICalendarServiceFactory CalendarServiceFactory { get; set; }
        public ICalendarSyncEngine CalendarSyncEngine { get; set; }
        public ISyncAnalyticsService AnalyticsService { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <param name="startDate"></param>
        /// <returns>
        /// </returns>
        private bool LoadAppointments(DateTime startDate, DateTime endDate,
            IDictionary<string, object> sourceCalendarSpecificData,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            //Update status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsReading,
                SourceCalendarService.CalendarServiceName);

            //Get source calendar
            SourceAppointments =
                SourceCalendarService.GetCalendarEventsInRangeAsync(startDate, endDate, sourceCalendarSpecificData)
                    .Result;
            if (SourceAppointments == null)
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsReadFailed);
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return false;
            }
            //Update status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsRead,
                SourceCalendarService.CalendarServiceName, SourceAppointments.Count);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentReading,
                DestinationCalendarService.CalendarServiceName);

            //Get destination calendar
            DestinationAppointments = DestinationCalendarService.GetCalendarEventsInRangeAsync(startDate, endDate,
                destinationCalendarSpecificData).Result;
            if (DestinationAppointments == null)
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentReadFailed);
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return false;
            }
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentRead,
                DestinationCalendarService.CalendarServiceName, DestinationAppointments.Count);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            return true;
        }

        private void InitiatePreSyncSetup(CalendarSyncProfile syncProfile)
        {
            SourceCalendarService = CalendarServiceFactory.GetCalendarService(syncProfile.Source);
            DestinationCalendarService =
                CalendarServiceFactory.GetCalendarService(syncProfile.Destination);
        }

        private IDictionary<string, object> GetCalendarSpecificData(ServiceType serviceType,
            CalendarSyncProfile syncProfile)
        {
            IDictionary<string, object> calendarSpecificData = null;
            switch (serviceType)
            {
                case ServiceType.Google:
                    calendarSpecificData = new Dictionary<string, object>
                    {
                        {"CalendarId", syncProfile.GoogleSettings.GoogleCalendar.Id},
                        {"AccountName", syncProfile.GoogleSettings.GoogleAccount.Name}
                    };
                    break;
                case ServiceType.OutlookDesktop:
                    calendarSpecificData = new Dictionary<string, object>
                    {
                        {
                            "ProfileName",
                            !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile)
                                ? syncProfile.OutlookSettings.OutlookProfileName
                                : null
                        },
                        {
                            "OutlookCalendar",
                            !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultMailBoxCalendar)
                                ? syncProfile.OutlookSettings.OutlookCalendar
                                : null
                        },
                        {
                            "AddAsAppointments",
                            syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AddAsAppointments)
                        }
                    };
                    break;
                case ServiceType.EWS:
                    return null;
            }
            if (calendarSpecificData != null && syncProfile.SetCalendarCategory)
            {
                calendarSpecificData.Add("EventCategory", syncProfile.EventCategory);
            }
            return calendarSpecificData;
        }

        private void LoadSourceId(List<Appointment> appointmentList, string calendarId)
        {
            if (appointmentList.Any())
            {
                foreach (var sourceAppointment in appointmentList)
                {
                    sourceAppointment.LoadSourceId(calendarId);
                    sourceAppointment.LoadChildId(calendarId);
                }
            }
        }

        /// <summary>
        ///     Add appointments to destination
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <returns>
        /// </returns>
        private bool AddDestinationAppointments(CalendarSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            //Update status for reading entries to add
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd,
                DestinationCalendarService.CalendarServiceName);
            //Get entries to add
            CalendarSyncEngine.GetDestEntriesToAdd(syncProfile, SourceAppointments,
                DestinationAppointments);
            var appointmentsToAdd = CalendarSyncEngine.DestAppointmentsToAdd;
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, appointmentsToAdd.Count);
            if (appointmentsToAdd.Count == 0)
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries,
                DestinationCalendarService.CalendarServiceName);
            //Add entries to destination calendar
            var addedAppointments = DestinationCalendarService.AddCalendarEvents(appointmentsToAdd,
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription),
                destinationCalendarSpecificData)
                .Result;
            var isSuccess = addedAppointments.IsSuccess;
            //Update status if entries were successfully added
            CalendarSyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            if (isSuccess)
            {
                syncMetric.DestMetric.AddCount = appointmentsToAdd.Count;
                LoadSourceId(addedAppointments, SourceAppointments.CalendarId);
                DestinationAppointments.AddRange(addedAppointments);
                if (syncProfile.SyncMode == SyncModeEnum.TwoWay)
                {
                    //Add appointments to update
                    var updateSourceList = UpdateWithChildId(addedAppointments, SourceAppointments);
                    CalendarSyncEngine.SourceAppointmentsToUpdate.AddRangeCompareForUpdate(updateSourceList);
                }
            }
            return isSuccess;
        }

        /// <summary>
        ///     Delete appointments in destination
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <param name="syncCallback"></param>
        /// <returns>
        /// </returns>
        private bool DeleteDestinationAppointments(CalendarSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> destinationCalendarSpecificData, SyncCallback syncCallback)
        {
            if (syncProfile.SyncSettings.DisableDelete)
            {
                return true;
            }
            //Updating entry isDeleteOperation status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete,
                DestinationCalendarService.CalendarServiceName);
            //Getting appointments to isDeleteOperation
            CalendarSyncEngine.GetDestEntriesToDelete(syncProfile,
                SourceAppointments, DestinationAppointments);
            var appointmentsToDelete = CalendarSyncEngine.DestAppointmentsToDelete;

            if (syncProfile.SyncMode == SyncModeEnum.OneWay)
            {
                if (syncProfile.SyncSettings.ConfirmOnDelete && syncCallback != null)
                {
                    var orphanEntries = Environment.NewLine +
                                        string.Join(Environment.NewLine, CalendarSyncEngine.DestOrphanEntries);
                    //Log Orphan Entries
                    ApplicationLogger.Warn("Orphan entries to delete: " + orphanEntries);

                    var message = string.Format("Are you sure you want to delete {0} orphan entries from {1}?{2}",
                        appointmentsToDelete.Count, DestinationCalendarService.CalendarServiceName,
                        orphanEntries);
                    var e = new SyncEventArgs(message, UserActionEnum.ConfirmDelete);

                    var task = syncCallback(e);
                    if (task.Result)
                    {
                        appointmentsToDelete.AddRange(CalendarSyncEngine.DestOrphanEntries);
                    }
                }
                else if (!syncProfile.SyncSettings.DisableDelete)
                {
                    appointmentsToDelete.AddRange(CalendarSyncEngine.DestOrphanEntries);
                }
            }

            //Updating Get entry isDeleteOperation status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);

            if (appointmentsToDelete.Count == 0)
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }

            //Updating isDeleteOperation status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries,
                DestinationCalendarService.CalendarServiceName);

            //Deleting entries
            
            var deletedAppointments = DestinationCalendarService.DeleteCalendarEvents(appointmentsToDelete, destinationCalendarSpecificData)
                    .Result;
            var isSuccess = deletedAppointments.IsSuccess;
            //Update status if entries were successfully deleted
            CalendarSyncStatus =
                StatusHelper.GetMessage(isSuccess
                    ? SyncStateEnum.DeletingEntriesComplete
                    : SyncStateEnum.DeletingEntriesFailed);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            if (isSuccess)
            {
                syncMetric.DestMetric.DeleteCount = appointmentsToDelete.Count;
                syncMetric.DestMetric.DeleteFailedCount = appointmentsToDelete.Count - deletedAppointments.Count;
                for (var index = 0; index < appointmentsToDelete.Count; index++)
                {
                    DestinationAppointments.Remove(appointmentsToDelete[index]);
                }
            }

            return isSuccess;
        }

        /// <summary>
        ///     Add appointments to source
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <returns>
        /// </returns>
        private bool AddSourceAppointments(CalendarSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> sourceCalendarSpecificData)
        {
            //Update status for reading entries to add
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd,
                SourceCalendarService.CalendarServiceName);
            //Get entries to add
            CalendarSyncEngine.GetSourceEntriesToAdd(syncProfile, SourceAppointments, DestinationAppointments);
            var appointmentsToAdd = CalendarSyncEngine.SourceAppointmentsToAdd;
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, appointmentsToAdd.Count);
            if (appointmentsToAdd.Count == 0)
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries, SourceCalendarService.CalendarServiceName);

            //Add entries to calendar
            var addedAppointments = SourceCalendarService.AddCalendarEvents(appointmentsToAdd,
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription),
                sourceCalendarSpecificData)
                .Result;
            var isSuccess = addedAppointments.IsSuccess;
            //Update status if entries were successfully added
            CalendarSyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            if (isSuccess)
            {
                syncMetric.SourceMetric.AddCount = appointmentsToAdd.Count;
                LoadSourceId(addedAppointments, DestinationAppointments.CalendarId);
                SourceAppointments.AddRange(addedAppointments);
                if (syncProfile.SyncMode == SyncModeEnum.TwoWay)
                {
                    var updateDestList = UpdateWithChildId(addedAppointments, DestinationAppointments);
                    CalendarSyncEngine.DestAppointmentsToUpdate.AddRangeCompareForUpdate(updateDestList);
                }
            }

            return isSuccess;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addedAppointments"></param>
        /// <param name="existingAppointments"></param>
        /// <returns></returns>
        private List<Appointment> UpdateWithChildId(AppointmentsWrapper addedAppointments,
            AppointmentsWrapper existingAppointments)
        {
            //Add appointments to update
            var updateList = new List<Appointment>();
            foreach (var appointment in addedAppointments)
            {
                var presentAppointment = existingAppointments.FirstOrDefault(t => t.CompareSourceId(appointment));
                if (presentAppointment != null)
                {
                    var childKey = appointment.GetChildEntryKey();
                    if (!presentAppointment.ExtendedProperties.ContainsKey(childKey))
                    {
                        presentAppointment.ExtendedProperties.Add(childKey, appointment.AppointmentId);
                    }
                    else
                    {
                        presentAppointment.ExtendedProperties[childKey] = appointment.AppointmentId;
                    }
                    updateList.Add(presentAppointment);
                }
            }
            return updateList;
        }

        /// <summary>
        ///     Delete appointments from source
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <param name="syncCallback"></param>
        /// <returns>
        /// </returns>
        private bool DeleteSourceAppointments(CalendarSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> sourceCalendarSpecificData, SyncCallback syncCallback)
        {
            if (syncProfile.SyncSettings.DisableDelete)
            {
                return true;
            }
            //Updating entry isDeleteOperation status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete,
                SourceCalendarService.CalendarServiceName);
            //Getting appointments to isDeleteOperation
            CalendarSyncEngine.GetSourceEntriesToDelete(syncProfile, SourceAppointments, DestinationAppointments);
            var appointmentsToDelete = CalendarSyncEngine.SourceAppointmentsToDelete;
            //Updating Get entry isDeleteOperation status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
            if (appointmentsToDelete.Count == 0)
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }

            //Updating isDeleteOperation status
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries,
                SourceCalendarService.CalendarServiceName);
            //Deleting entries
            var deletedAppointments =
                SourceCalendarService.DeleteCalendarEvents(appointmentsToDelete, sourceCalendarSpecificData).Result;
            var isSuccess = deletedAppointments.IsSuccess;
            //Update status if entries were successfully deleted
            CalendarSyncStatus =
                StatusHelper.GetMessage(isSuccess
                    ? SyncStateEnum.DeletingEntriesComplete
                    : SyncStateEnum.DeletingEntriesFailed);
            CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            if (isSuccess)
            {
                syncMetric.SourceMetric.DeleteCount = appointmentsToDelete.Count;
                syncMetric.SourceMetric.DeleteFailedCount = appointmentsToDelete.Count - deletedAppointments.Count;
                for (var index = 0; index < appointmentsToDelete.Count; index++)
                {
                    SourceAppointments.Remove(appointmentsToDelete[index]);
                }
            }
            return isSuccess;
        }

        #endregion

        #region ICalendarUpdateService Members

        public AppointmentsWrapper DestinationAppointments
        {
            get { return _destinationAppointments; }
            set { SetProperty(ref _destinationAppointments, value); }
        }

        public AppointmentsWrapper SourceAppointments
        {
            get { return _sourceAppointments; }
            set { SetProperty(ref _sourceAppointments, value); }
        }

        public Appointment CurrentAppointment
        {
            get { return _currentAppointment; }
            set { SetProperty(ref _currentAppointment, value); }
        }

        public string CalendarSyncStatus
        {
            get { return _calendarSyncStatus; }
            set { SetProperty(ref _calendarSyncStatus, value); }
        }

        public ICalendarService SourceCalendarService { get; set; }

        public ICalendarService DestinationCalendarService { get; set; }

        public ILog ApplicationLogger { get; set; }

        public bool SyncCalendar(CalendarSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback syncCallback)
        {
            InitiatePreSyncSetup(syncProfile);

            var isSuccess = false;
            if (syncProfile != null)
            {
                CalendarSyncEngine.Clear();
                //Add log for sync mode
                CalendarSyncStatus = string.Format("Calendar Sync : {0} {2} {1}", SourceCalendarService.CalendarServiceName,
                    DestinationCalendarService.CalendarServiceName,
                    syncProfile.SyncMode == SyncModeEnum.TwoWay ? "<===>" : "===>");
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                DateTime startDate, endDate;
                GetDateRange(syncProfile, out startDate, out endDate);
                //Add log for date range
                CalendarSyncStatus = string.Format("Date Range : {0} - {1}",
                    startDate.ToString("d"),
                    endDate.ToString("d"));

                //Load calendar specific data
                var sourceCalendarSpecificData =
                    GetCalendarSpecificData(syncProfile.Source, syncProfile);
                var destinationCalendarSpecificData =
                    GetCalendarSpecificData(syncProfile.Destination, syncProfile);

                //Get source and destination appointments
                isSuccess = LoadAppointments(startDate, endDate,
                    sourceCalendarSpecificData,
                    destinationCalendarSpecificData);

                if (isSuccess)
                {
                    syncMetric.SourceMetric.OriginalCount = SourceAppointments.Count;
                    syncMetric.DestMetric.OriginalCount = DestinationAppointments.Count;
                    LoadSourceId(DestinationAppointments, SourceAppointments.CalendarId);
                    LoadSourceId(SourceAppointments, DestinationAppointments.CalendarId);
                }

                if (isSuccess)
                {
                    //Delete destination appointments
                    isSuccess = DeleteDestinationAppointments(syncProfile, syncMetric, destinationCalendarSpecificData, syncCallback);
                }

                if (isSuccess)
                {
                    //Add appointments to destination
                    isSuccess = AddDestinationAppointments(syncProfile, syncMetric, destinationCalendarSpecificData);
                }

                if (isSuccess && syncProfile.SyncMode == SyncModeEnum.TwoWay)
                {
                    //Delete destination appointments
                    isSuccess = DeleteSourceAppointments(syncProfile, syncMetric, sourceCalendarSpecificData, syncCallback);
                    if (isSuccess)
                    {
                        //If sync mode is two way... add events to source
                        isSuccess = AddSourceAppointments(syncProfile, syncMetric, sourceCalendarSpecificData);
                    }
                }

                if (isSuccess)
                {
                    isSuccess = UpdateEntries(syncProfile, syncMetric, sourceCalendarSpecificData, destinationCalendarSpecificData);
                }
            }
            syncMetric.IsSuccess = isSuccess;
            SourceAppointments = null;
            DestinationAppointments = null;
            SourceCalendarService = null;
            DestinationCalendarService = null;
            return isSuccess;
        }

        private void UploadAnalyticsData(CalendarSyncProfile syncProfile, bool isSuccess)
        {
            var syncMetric = new SyncMetric
            {
                IsSuccess = isSuccess,
            };

            //AnalyticsService.UploadSyncData(syncMetric, syncProfile.GoogleAccount.Name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <returns></returns>
        private bool UpdateEntries(CalendarSyncProfile syncProfile, SyncMetric syncMetric,
            IDictionary<string, object> sourceCalendarSpecificData,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            var isSuccess = true;
            if (CalendarSyncEngine.SourceAppointmentsToUpdate.Any())
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                //Update status for reading entries to update
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToUpdate,
                    CalendarSyncEngine.SourceAppointmentsToUpdate.Count,
                    SourceCalendarService.CalendarServiceName);
                var updatedAppointments = SourceCalendarService.UpdateCalendarEvents(CalendarSyncEngine.SourceAppointmentsToUpdate,
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees),
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription),
                    sourceCalendarSpecificData).Result;
                isSuccess = updatedAppointments.IsSuccess;
                CalendarSyncStatus =
                    StatusHelper.GetMessage(isSuccess
                        ? SyncStateEnum.UpdateEntriesSuccess
                        : SyncStateEnum.UpdateEntriesFailed);
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                syncMetric.SourceMetric.UpdateCount = CalendarSyncEngine.SourceAppointmentsToUpdate.Count;
                syncMetric.SourceMetric.UpdateFailedCount = 
                    CalendarSyncEngine.SourceAppointmentsToUpdate.Count - updatedAppointments.Count;
            }

            if (CalendarSyncEngine.DestAppointmentsToUpdate.Any())
            {
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                //Update status for reading entries to update
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToUpdate,
                    CalendarSyncEngine.DestAppointmentsToUpdate.Count,
                    DestinationCalendarService.CalendarServiceName);
                var updatedAppointments = DestinationCalendarService.UpdateCalendarEvents(CalendarSyncEngine.DestAppointmentsToUpdate,
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees),
                    syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription),
                    destinationCalendarSpecificData).Result;
                isSuccess = updatedAppointments.IsSuccess;
                CalendarSyncStatus =
                    StatusHelper.GetMessage(isSuccess
                        ? SyncStateEnum.UpdateEntriesSuccess
                        : SyncStateEnum.UpdateEntriesFailed);
                CalendarSyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                syncMetric.DestMetric.UpdateCount = CalendarSyncEngine.DestAppointmentsToUpdate.Count;
                syncMetric.DestMetric.UpdateFailedCount =
                    CalendarSyncEngine.DestAppointmentsToUpdate.Count - updatedAppointments.Count;
            }

            return isSuccess;
        }

        private void GetDateRange(CalendarSyncProfile syncProfile, out DateTime startDate, out DateTime endDate)
        {
            startDate = syncProfile.SyncSettings.StartDate.Date;
            endDate = syncProfile.SyncSettings.EndDate.Date;
            if (syncProfile.SyncSettings.SyncRangeType == SyncRangeTypeEnum.SyncRangeInDays)
            {
                startDate = DateTime.Today.AddDays((-syncProfile.SyncSettings.DaysInPast));
                endDate = DateTime.Today.AddDays((syncProfile.SyncSettings.DaysInFuture + 1));
            }
            else if (syncProfile.SyncSettings.SyncRangeType == SyncRangeTypeEnum.SyncEntireCalendar)
            {
                startDate = DateTime.Parse("1990/01/01 12:00:00 AM");
                endDate = DateTime.Today.AddYears(10);
            }
        }

        #endregion
    }
}