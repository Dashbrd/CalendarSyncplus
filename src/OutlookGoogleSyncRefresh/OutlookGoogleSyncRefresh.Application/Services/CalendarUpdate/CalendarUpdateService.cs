#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
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
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Waf.Foundation;

using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.Utilities;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Common.MetaData;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    [Export(typeof(ICalendarUpdateService))]
    public class CalendarUpdateService : Model, ICalendarUpdateService
    {
        #region Fields

        private readonly ApplicationLogger _applicationLogger;

        private Appointment _currentAppointment;
        private List<Appointment> _destinationAppointments;
        private List<Appointment> _sourceAppointments;
        private string _syncStatus;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public CalendarUpdateService(ICalendarServiceFactory calendarServiceFactory, ApplicationLogger applicationLogger)
        {
            _applicationLogger = applicationLogger;
            CalendarServiceFactory = calendarServiceFactory;
        }

        #endregion

        #region Properties

        public ICalendarServiceFactory CalendarServiceFactory { get; set; }

        #endregion

        #region Private Methods

        private bool GetAppointments(int daysInPast, int daysInFuture,
            IDictionary<string, object> sourceCalendarSpecificData, IDictionary<string, object> destinationCalendarSpecificData)
        {
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsReading, SourceCalendarService.CalendarServiceName);

            SourceAppointments = SourceCalendarService.GetCalendarEventsInRangeAsync(daysInPast, daysInFuture, sourceCalendarSpecificData).Result;
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsRead, SourceCalendarService.CalendarServiceName, SourceAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentReading, DestinationCalendarService.CalendarServiceName);

            DestinationAppointments = DestinationCalendarService.GetCalendarEventsInRangeAsync(daysInPast, daysInFuture,
                        destinationCalendarSpecificData).Result;
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentRead, DestinationCalendarService.CalendarServiceName, DestinationAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            return true;
        }
        private List<Appointment> GetAppointmentsToDelete(SyncModeEnum syncMode, List<Appointment> sourceList, List<Appointment> destinationList)
        {
            if (sourceList.Any() && destinationList.Any())
            {
                var appointmentsToDelete = new List<Appointment>();
                foreach (var destAppointment in destinationList)
                {
                    if (destAppointment.SourceId == null)
                    {
                        if (syncMode != SyncModeEnum.TwoWay)
                        {
                            appointmentsToDelete.Add(destAppointment);
                        }
                        continue;
                    }

                    bool isFound = false;
                    foreach (var sourceAppointment in sourceList)
                    {
                        if (destAppointment.SourceId.Equals(sourceAppointment.AppointmentId))
                        {
                            isFound = true;
                            if (!destAppointment.Equals(sourceAppointment))
                            {
                                isFound = false;
                            }
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        appointmentsToDelete.Add(destAppointment);
                    }
                }
                return appointmentsToDelete;
            }

            return destinationList;
        }

        private List<Appointment> GetAppointmentsToAdd(List<Appointment> sourceList, List<Appointment> destinationList)
        {
            if (destinationList.Any() && sourceList.Any())
            {
                var appointmentsToAdd = new List<Appointment>();
                foreach (var sourceAppointment in sourceList)
                {
                    bool isFound = false;
                    foreach (var destAppointment in destinationList)
                    {
                        if (destAppointment.SourceId != null &&
                            destAppointment.SourceId.Equals(sourceAppointment.AppointmentId))
                        {
                            isFound = true;
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        appointmentsToAdd.Add(sourceAppointment);
                    }
                }

                return appointmentsToAdd;
            }
            return sourceList;
        }


        //private List<Appointment> GetAppointmentsToDelete()
        //{
        //    if (DestinationAppointments.Any() && SourceAppointments.Any())
        //    {
        //        return (from googleAppointment in DestinationAppointments
        //                let isFound = SourceAppointments.Contains(googleAppointment)
        //                where !isFound
        //                select googleAppointment).ToList();
        //    }

        //    return DestinationAppointments;
        //}

        //private List<Appointment> GetAppointmentsToAdd()
        //{
        //    if (DestinationAppointments.Any() && SourceAppointments.Any())
        //    {
        //        return (from outlookAppointment in SourceAppointments
        //                let isFound = DestinationAppointments.Contains(outlookAppointment)
        //                where !isFound
        //                select outlookAppointment).ToList();
        //    }
        //    return SourceAppointments;
        //}

        private void InitiatePreSyncSetup(Settings settings)
        {
            SourceCalendarService = CalendarServiceFactory.GetCalendarService(settings.SyncSettings.SourceCalendar);
            DestinationCalendarService = CalendarServiceFactory.GetCalendarService(settings.SyncSettings.DestinationCalendar);
        }

        private IDictionary<string, object> GetCalendarSpecificData(CalendarServiceType serviceType, Settings settings)
        {
            switch (serviceType)
            {
                case CalendarServiceType.Google:
                    return new Dictionary<string, object> { { "CalendarId", settings.GoogleCalendar.Id } };
                case CalendarServiceType.OutlookDesktop:
                    return new Dictionary<string, object>
                    {
                        { "ProfileName", settings.OutlookSettings.OutlookProfileName },
                        { "OutlookCalendar", settings.OutlookSettings.OutlookCalendar }
                    };
                case CalendarServiceType.EWS:
                    return null;
            }
            return null;
        }

        #endregion

        #region ICalendarUpdateService Members

        public List<Appointment> DestinationAppointments
        {
            get { return _destinationAppointments; }
            set { SetProperty(ref _destinationAppointments, value); }
        }

        public List<Appointment> SourceAppointments
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

        public bool SyncCalendar(Settings settings)
        {
            InitiatePreSyncSetup(settings);

            bool isSuccess = false;
            if (settings != null && settings.GoogleCalendar != null)
            {
                //Add log for sync mode
                SyncStatus = string.Format("Calendar Sync : {0} {2} {1}", SourceCalendarService.CalendarServiceName, DestinationCalendarService.CalendarServiceName,
                    settings.SyncSettings.SyncMode == SyncModeEnum.TwoWay ? "<===>" : "===>");
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                //Add log for date range
                SyncStatus = string.Format("Sync Date Range - {0} - {1}",
                    DateTime.Now.Subtract(new TimeSpan(settings.DaysInPast, 0, 0, 0)).ToString("D"),
                    DateTime.Now.Add(new TimeSpan(settings.DaysInFuture, 0, 0, 0)).ToString("D"));
                //Load calendar specific data

                var sourceCalendarSpecificData = GetCalendarSpecificData(settings.SyncSettings.SourceCalendar, settings);
                var destinationCalendarSpecificData = GetCalendarSpecificData(settings.SyncSettings.DestinationCalendar, settings);

                //Get source and destination appointments
                isSuccess = GetAppointments(settings.DaysInPast, settings.DaysInFuture, sourceCalendarSpecificData,
                            destinationCalendarSpecificData);
                if (isSuccess)
                {
                    //Delete destination appointments
                    isSuccess = DeleteDestinationAppointments(settings, destinationCalendarSpecificData);
                }
                if (isSuccess)
                {
                    //Add appointments to destination
                    isSuccess = AddDestinationAppointments(settings, destinationCalendarSpecificData);
                }

                if (settings.SyncSettings.SyncMode == SyncModeEnum.TwoWay)
                {
                    if (isSuccess)
                    {
                        //Delete destination appointments
                        isSuccess = DeleteSourceAppointments(settings, sourceCalendarSpecificData);
                    }

                    if (isSuccess)
                    {
                        //If sync mode is two way... add events to source
                        isSuccess = AddSourceAppointments(settings, sourceCalendarSpecificData);
                    }
                }
            }
            return isSuccess;
        }



        private bool AddSourceAppointments(Settings settings, IDictionary<string, object> sourceCalendarSpecificData)
        {
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd);
            List<Appointment> calendarAppointments = GetAppointmentsToAdd(DestinationAppointments, SourceAppointments);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, calendarAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries, SourceCalendarService.CalendarServiceName);
            bool isSuccess = SourceCalendarService.AddCalendarEvent(calendarAppointments,
                settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees), sourceCalendarSpecificData)
                .Result;
            SyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            return isSuccess;
        }

        private bool AddDestinationAppointments(Settings settings, IDictionary<string, object> destinationCalendarSpecificData)
        {
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd);
            List<Appointment> calendarAppointments = GetAppointmentsToAdd(SourceAppointments, DestinationAppointments);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, calendarAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries, DestinationCalendarService.CalendarServiceName);
            bool isSuccess = DestinationCalendarService.AddCalendarEvent(calendarAppointments,
                settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees), destinationCalendarSpecificData)
                .Result;
            SyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            return isSuccess;
        }

        private bool DeleteDestinationAppointments(Settings settings,
            IDictionary<string, object> destinationCalendarSpecificData)
        {
            //Updating entry delete status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete);
            //Getting appointments to delete
            List<Appointment> appointmentsToDelete = GetAppointmentsToDelete(settings.SyncSettings.SyncMode, SourceAppointments, DestinationAppointments);
            //Updating Get entry delete status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
            //Updating delete status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries, DestinationCalendarService.CalendarServiceName);
            //Deleting entries
            bool isSuccess =
                DestinationCalendarService.DeleteCalendarEvent(appointmentsToDelete, destinationCalendarSpecificData).Result;
            SyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.DeletingEntriesComplete : SyncStateEnum.DeletingEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            return isSuccess;
        }

        private bool DeleteSourceAppointments(Settings settings, IDictionary<string, object> sourceCalendarSpecificData)
        {
            //Updating entry delete status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete);
            //Getting appointments to delete
            List<Appointment> appointmentsToDelete = GetAppointmentsToDelete(settings.SyncSettings.SyncMode, DestinationAppointments, SourceAppointments);
            //Updating Get entry delete status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
            //Updating delete status
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries, SourceCalendarService.CalendarServiceName);
            //Deleting entries
            bool isSuccess =
                SourceCalendarService.DeleteCalendarEvent(appointmentsToDelete, sourceCalendarSpecificData).Result;
            SyncStatus =
                StatusHelper.GetMessage(isSuccess ? SyncStateEnum.DeletingEntriesComplete : SyncStateEnum.DeletingEntriesFailed);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            return isSuccess;
        }
        #endregion
    }
}