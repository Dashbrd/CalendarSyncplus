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

        private async Task<bool> GetAppointments(int daysInPast, int daysInFuture,
            IDictionary<string, object> sourceCalendarSpecificData, IDictionary<string, object> destinationCalendarSpecificData)
        {
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsReading, SourceCalendarService.CalendarServiceName);

            SourceAppointments =
                await
                    SourceCalendarService.GetCalendarEventsInRangeAsync(daysInPast, daysInFuture, sourceCalendarSpecificData);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentsRead, SourceCalendarService.CalendarServiceName, SourceAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentReading, DestinationCalendarService.CalendarServiceName);

            DestinationAppointments =
                await
                    DestinationCalendarService.GetCalendarEventsInRangeAsync(daysInPast, daysInFuture,
                        destinationCalendarSpecificData);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestAppointmentRead, DestinationCalendarService.CalendarServiceName, DestinationAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            return true;
        }

        private List<Appointment> GetAppointmentsToDelete()
        {
            if (DestinationAppointments.Any() && SourceAppointments.Any())
            {
                return (from googleAppointment in DestinationAppointments
                        let isFound = SourceAppointments.Contains(googleAppointment)
                        where !isFound
                        select googleAppointment).ToList();
            }

            return DestinationAppointments;
        }

        private List<Appointment> GetAppointmentsToAdd()
        {
            if (DestinationAppointments.Any() && SourceAppointments.Any())
            {
                return (from outlookAppointment in SourceAppointments
                        let isFound = DestinationAppointments.Contains(outlookAppointment)
                        where !isFound
                        select outlookAppointment).ToList();
            }
            return SourceAppointments;
        }

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

        public async Task<bool> SyncCalendarAsync(Settings settings)
        {
            InitiatePreSyncSetup(settings);

            bool isSuccess = false;
            if (settings != null && settings.GoogleCalendar != null)
            {
                SyncStatus = "Calendar Sync Mode : Outlook -> Google";
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                SyncStatus = string.Format("Sync Date Range - {0} - {1}",
                    DateTime.Now.Subtract(new TimeSpan(settings.DaysInPast, 0, 0, 0)).ToString("D"),
                    DateTime.Now.Add(new TimeSpan(settings.DaysInFuture, 0, 0, 0)).ToString("D"));
                var sourceCalendarSpecificData = GetCalendarSpecificData(settings.SyncSettings.SourceCalendar, settings);
                var destinationCalendarSpecificData = GetCalendarSpecificData(settings.SyncSettings.DestinationCalendar, settings);

                isSuccess =
                    await
                        GetAppointments(settings.DaysInPast, settings.DaysInFuture, sourceCalendarSpecificData,
                            destinationCalendarSpecificData);

                if (isSuccess)
                {

                    //Updating entry delete status
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete);
                    //Getting appointments to delete
                    List<Appointment> appointmentsToDelete = GetAppointmentsToDelete();
                    //Updating Get entry delete status
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
                    //Updating delete status
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries, DestinationCalendarService.CalendarServiceName);
                    //Deleting entries
                    isSuccess =
                        await DestinationCalendarService.DeleteCalendarEvent(appointmentsToDelete, destinationCalendarSpecificData);

                    if (isSuccess)
                    {
                        //Deletion complete
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntriesComplete);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd);
                        List<Appointment> calendarAppointments = GetAppointmentsToAdd();
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, calendarAppointments.Count);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries, DestinationCalendarService.CalendarServiceName);
                        isSuccess = await DestinationCalendarService.AddCalendarEvent(calendarAppointments,
                            settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                            settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                            settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees), destinationCalendarSpecificData);
                        SyncStatus =
                            StatusHelper.GetMessage(isSuccess ? SyncStateEnum.AddEntriesComplete : SyncStateEnum.AddEntriesFailed);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                    }
                    else
                    {
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntriesFailed);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                    }
                }
            }
            return isSuccess;
        }

        #endregion
    }
}