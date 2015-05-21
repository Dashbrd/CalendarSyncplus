#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     03-02-2015 7:31 PM
//  *      Modified On:    05-02-2015 12:29 PM
//  *      FileName:       CalendarUpdateService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Foundation;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Common.MetaData;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using CalendarSyncPlus.Services.Wrappers;
using CalendarSyncPlus.SyncEngine.Interfaces;

#endregion

namespace CalendarSyncPlus.Services
{
    [Export(typeof(ICalendarUpdateService))]
    public class CalendarUpdateService : Model, ICalendarUpdateService
    {
        #region Fields

        private readonly ApplicationLogger _applicationLogger;

        private Appointment _currentAppointment;
        private CalendarAppointments _destinationAppointments;
        private CalendarAppointments _sourceAppointments;
        private string _syncStatus;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public CalendarUpdateService(ICalendarServiceFactory calendarServiceFactory, ICalendarSyncEngine calendarSyncEngine,
            ApplicationLogger applicationLogger)
        {
            _applicationLogger = applicationLogger;
            CalendarServiceFactory = calendarServiceFactory;
            CalendarSyncEngine = calendarSyncEngine;
        }

        #endregion

        #region Properties

        public ICalendarServiceFactory CalendarServiceFactory { get; set; }
        public ICalendarSyncEngine CalendarSyncEngine { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// </summary>

        /// <param name="endDate"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        private bool LoadAppointments(DateTime startDate, DateTime endDate, IDictionary<string, object> sourceCalendarSpecificData,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            //Update status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsReading,
                SourceCalendarService.CalendarServiceName);

            //Get source calendar
            SourceAppointments =
                SourceCalendarService.GetCalendarEventsInRangeAsync(startDate, endDate, sourceCalendarSpecificData)
                    .Result;
            if (SourceAppointments == null)
            {
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsReadFailed);
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return false;
            }
            //Update status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsRead,
                SourceCalendarService.CalendarServiceName, SourceAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentReading,
                DestinationCalendarService.CalendarServiceName);

            //Get destination calendar
            DestinationAppointments = DestinationCalendarService.GetCalendarEventsInRangeAsync(startDate, endDate,
                destinationCalendarSpecificData).Result;
            if (DestinationAppointments == null)
            {
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentReadFailed);
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return false;
            }
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentRead,
                DestinationCalendarService.CalendarServiceName, DestinationAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            return true;
        }

        private void InitiatePreSyncSetup(CalendarSyncProfile syncProfile)
        {
            SourceCalendarService = CalendarServiceFactory.GetCalendarService(syncProfile.SyncSettings.SourceCalendar);
            DestinationCalendarService =
                CalendarServiceFactory.GetCalendarService(syncProfile.SyncSettings.DestinationCalendar);
        }

        private IDictionary<string, object> GetCalendarSpecificData(CalendarServiceType serviceType,
            CalendarSyncProfile syncProfile)
        {
            IDictionary<string, object> calendarSpecificData = null;
            switch (serviceType)
            {
                case CalendarServiceType.Google:
                    calendarSpecificData = new Dictionary<string, object> 
                    { 
                        {"CalendarId", syncProfile.GoogleAccount.GoogleCalendar.Id},
                        {"AccountName",syncProfile.GoogleAccount.Name}
                    };
                    break;
                case CalendarServiceType.OutlookDesktop:
                    calendarSpecificData = new Dictionary<string, object>
                    {
                        {
                            "ProfileName", !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultProfile)
                                ? syncProfile.OutlookSettings.OutlookProfileName
                                : null
                        },
                        {
                            "OutlookCalendar",
                            !syncProfile.OutlookSettings.OutlookOptions.HasFlag(OutlookOptionsEnum.DefaultCalendar)
                                ? syncProfile.OutlookSettings.OutlookCalendar
                                : null
                        },
                        { "AddAsAppointments", syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AsAppointments) }
                    };
                    break;
                case CalendarServiceType.EWS:
                    return null;
            }
            if (calendarSpecificData != null && syncProfile.SetCalendarCategory)
            {
                calendarSpecificData.Add("EventCategory", syncProfile.EventCategory);
            }
            return calendarSpecificData;
        }

        private void LoadSourceId()
        {
            if (SourceAppointments.Any())
            {
                string calendarId = DestinationAppointments.CalendarId;
                foreach (Appointment sourceAppointment in SourceAppointments)
                {
                    sourceAppointment.LoadSourceId(calendarId);
                }
            }

            if (DestinationAppointments.Any())
            {
                string calendarId = SourceAppointments.CalendarId;
                foreach (Appointment destAppointment in DestinationAppointments)
                {
                    destAppointment.LoadSourceId(calendarId);
                }
            }
        }

        /// <summary>
        ///     Add appointments to destination
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <returns></returns>
        private bool AddDestinationAppointments(CalendarSyncProfile syncProfile,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            //Update status for reading entries to add
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd);
            //Get entries to add
            CalendarSyncEngine.GetDestEntriesToAdd(syncProfile, SourceAppointments,
                DestinationAppointments);
            List<Appointment> appointmentsToAdd = CalendarSyncEngine.DestAppointmentsToAdd;
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, appointmentsToAdd.Count);
            if (appointmentsToAdd.Count == 0)
            {
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries,
                DestinationCalendarService.CalendarServiceName);
            //Add entries to destination calendar
            bool isSuccess = DestinationCalendarService.AddCalendarEvents(appointmentsToAdd,
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription),
                destinationCalendarSpecificData)
                .Result;
            //Update status if entries were successfully added
            SyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            return isSuccess;
        }

        /// <summary>
        ///     Delete appointments in destination
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="destinationCalendarSpecificData"></param>
        /// <returns></returns>
        private bool DeleteDestinationAppointments(CalendarSyncProfile syncProfile,
            IDictionary<string, object> destinationCalendarSpecificData, SyncCallback syncCallback)
        {
            if (syncProfile.SyncSettings.DisableDelete)
            {
                return true;
            }
            //Updating entry isDeleteOperation status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete);
            //Getting appointments to isDeleteOperation
            //List<Appointment> appointmentsToDelete = GetAppointmentsToDelete(syncProfile, SourceAppointments,
            //    DestinationAppointments);
            bool result = CalendarSyncEngine.GetDestEntriesToDelete(syncProfile,
                SourceAppointments, DestinationAppointments);
            List<Appointment> appointmentsToDelete = CalendarSyncEngine.DestAppointmentsToDelete;
            //Updating Get entry isDeleteOperation status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);

            if (appointmentsToDelete.Count == 0)
            {
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }

            if (syncProfile.SyncSettings.ConfirmOnDelete && syncCallback != null)
            {
                string message = string.Format("Are you sure you want to delete {0} items from {1}?",
                    appointmentsToDelete.Count, DestinationCalendarService.CalendarServiceName);
                var e = new SyncEventArgs(message, UserActionEnum.ConfirmDelete);
                Task<bool> task = syncCallback(e);
                if (!task.Result)
                {
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                    return true;
                }
            }

            //Updating isDeleteOperation status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries,
                DestinationCalendarService.CalendarServiceName);

            //Deleting entries
            bool isSuccess =
                DestinationCalendarService.DeleteCalendarEvents(appointmentsToDelete, destinationCalendarSpecificData)
                    .Result;
            //Update status if entries were successfully deleted
            SyncStatus =
                StatusHelper.GetMessage(isSuccess
                    ? SyncStateEnum.DeletingEntriesComplete
                    : SyncStateEnum.DeletingEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            if (isSuccess)
            {
                for (int index = 0; index < appointmentsToDelete.Count; index++)
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
        /// <param name="sourceCalendarSpecificData"></param>
        /// <returns></returns>
        private bool AddSourceAppointments(CalendarSyncProfile syncProfile,
            IDictionary<string, object> sourceCalendarSpecificData)
        {
            //Update status for reading entries to add
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd);
            //Get entries to add
            CalendarSyncEngine.GetSourceEntriesToAdd(syncProfile, DestinationAppointments,
                SourceAppointments);
            List<Appointment> appointmentsToAdd = CalendarSyncEngine.SourceAppointmentsToAdd;
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, appointmentsToAdd.Count);
            if (appointmentsToAdd.Count == 0)
            {
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries, SourceCalendarService.CalendarServiceName);

            //Add entries to calendar
            bool isSuccess = SourceCalendarService.AddCalendarEvents(appointmentsToAdd,
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees),
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription),
                sourceCalendarSpecificData)
                .Result;
            //Update status if entries were successfully added
            SyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            return isSuccess;
        }

        /// <summary>
        ///     Delete appointments from source
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceCalendarSpecificData"></param>
        /// <returns></returns>
        private bool DeleteSourceAppointments(CalendarSyncProfile syncProfile,
            IDictionary<string, object> sourceCalendarSpecificData, SyncCallback syncCallback)
        {
            if (syncProfile.SyncSettings.DisableDelete)
            {
                return true;
            }
            //Updating entry isDeleteOperation status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete);
            //Getting appointments to isDeleteOperation
             CalendarSyncEngine.GetSourceEntriesToDelete(syncProfile, DestinationAppointments,
                SourceAppointments);
            List<Appointment> appointmentsToDelete = CalendarSyncEngine.SourceAppointmentsToDelete;
            //Updating Get entry isDeleteOperation status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
            if (appointmentsToDelete.Count == 0)
            {
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                return true;
            }

            if (syncProfile.SyncSettings.ConfirmOnDelete && syncCallback != null)
            {
                string message = string.Format("Are you sure you want to delete {0} items from {1}?",
                    appointmentsToDelete.Count, DestinationCalendarService.CalendarServiceName);
                var e = new SyncEventArgs(message, UserActionEnum.ConfirmDelete);
                Task<bool> task = syncCallback(e);
                if (!task.Result)
                {
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SkipDelete);
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                    return true;
                }
            }
            //Updating isDeleteOperation status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries,
                SourceCalendarService.CalendarServiceName);
            //Deleting entries
            bool isSuccess =
                SourceCalendarService.DeleteCalendarEvents(appointmentsToDelete, sourceCalendarSpecificData).Result;
            //Update status if entries were successfully deleted
            SyncStatus =
                StatusHelper.GetMessage(isSuccess
                    ? SyncStateEnum.DeletingEntriesComplete
                    : SyncStateEnum.DeletingEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            if (isSuccess)
            {
                for (int index = 0; index < appointmentsToDelete.Count; index++)
                {
                    SourceAppointments.Remove(appointmentsToDelete[index]);
                }
            }
            return isSuccess;
        }

        #endregion

        #region ICalendarUpdateService Members

        public CalendarAppointments DestinationAppointments
        {
            get { return _destinationAppointments; }
            set { SetProperty(ref _destinationAppointments, value); }
        }

        public CalendarAppointments SourceAppointments
        {
            get { return _sourceAppointments; }
            set { SetProperty(ref _sourceAppointments, value); }
        }

        public Appointment CurrentAppointment
        {
            get { return _currentAppointment; }
            set { SetProperty(ref _currentAppointment, value); }
        }

        public string SyncStatus
        {
            get { return _syncStatus; }
            set { SetProperty(ref _syncStatus, value); }
        }

        public ICalendarService SourceCalendarService { get; set; }

        public ICalendarService DestinationCalendarService { get; set; }

        public ApplicationLogger ApplicationLogger
        {
            get { return _applicationLogger; }
        }

        public bool SyncCalendar(CalendarSyncProfile syncProfile, SyncCallback syncCallback)
        {
            InitiatePreSyncSetup(syncProfile);

            bool isSuccess = false;
            if (syncProfile != null)
            {
                CalendarSyncEngine.Clear();
                //Add log for sync mode
                SyncStatus = string.Format("Calendar Sync : {0} {2} {1}", SourceCalendarService.CalendarServiceName,
                    DestinationCalendarService.CalendarServiceName,
                    syncProfile.SyncSettings.SyncMode == SyncModeEnum.TwoWay ? "<===>" : "===>");
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                DateTime startDate, endDate;
                GetDateRange(syncProfile, out startDate, out endDate);
                //Add log for date range
                SyncStatus = string.Format("Date Range : {0} - {1}",
                    startDate.ToString("d"),
                    endDate.ToString("d"));

                //Load calendar specific data
                IDictionary<string, object> sourceCalendarSpecificData =
                    GetCalendarSpecificData(syncProfile.SyncSettings.SourceCalendar, syncProfile);
                IDictionary<string, object> destinationCalendarSpecificData =
                    GetCalendarSpecificData(syncProfile.SyncSettings.DestinationCalendar, syncProfile);

                //Get source and destination appointments
                isSuccess = LoadAppointments(startDate, endDate,
                    sourceCalendarSpecificData,
                    destinationCalendarSpecificData);

                if (isSuccess)
                {
                    LoadSourceId();
                }

                if (isSuccess)
                {
                    //Delete destination appointments
                    isSuccess = DeleteDestinationAppointments(syncProfile, destinationCalendarSpecificData, syncCallback);
                }

                if (isSuccess)
                {
                    //Add appointments to destination
                    isSuccess = AddDestinationAppointments(syncProfile, destinationCalendarSpecificData);
                }

                if (isSuccess && syncProfile.SyncSettings.SyncMode == SyncModeEnum.TwoWay)
                {
                    //Delete destination appointments
                    isSuccess = DeleteSourceAppointments(syncProfile, sourceCalendarSpecificData, syncCallback);

                    if (isSuccess)
                    {
                        //If sync mode is two way... add events to source
                        isSuccess = AddSourceAppointments(syncProfile, sourceCalendarSpecificData);
                    }
                }
            }
            SourceAppointments = null;
            DestinationAppointments = null;
            SourceCalendarService = null;
            DestinationCalendarService = null;
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