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
using OutlookGoogleSyncRefresh.Application.Services.Outlook;
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
        private readonly ApplicationLogger _applicationLogger;

        #region Fields

        private Appointment _currentAppointment;
        private List<Appointment> _getDestinationAppointments;
        private List<Appointment> _getSourceAppointments;
        private string _syncStatus;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public CalendarUpdateService(IOutlookCalendarService outlookCalendarService,
            IGoogleCalendarService googleCalendarService, ApplicationLogger applicationLogger)
        {
            _applicationLogger = applicationLogger;
            OutlookCalendarService = outlookCalendarService;
            GoogleCalendarService = googleCalendarService;
        }

        #endregion

        #region Properties

        public IOutlookCalendarService OutlookCalendarService { get; set; }
        public IGoogleCalendarService GoogleCalendarService { get; set; }

        [ImportMany]
        public Lazy<ICalendarService,ICalendarServiceMetaData>[] CalendarServicesFactoryLazy { get; set; }

        public List<Appointment> GetDestinationAppointments
        {
            get { return _getDestinationAppointments; }
            set { SetProperty(ref _getDestinationAppointments, value); }
        }

        public List<Appointment> GetSourceAppointments
        {
            get { return _getSourceAppointments; }
            set { SetProperty(ref _getSourceAppointments, value); }
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

        #endregion

        #region Private Methods

        private async Task<bool> GetAppointments(int daysInPast, int daysInFuture, string calenderId, string profileName,
            OutlookCalendar outlookCalendar)
        {
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.OutlookAppointmentsReading);

            var outlookCalendarSpecificData= new Dictionary<string, object>
            {
                { "ProfileName", profileName },
                { "OutlookCalendar", outlookCalendar }
            };

            GetSourceAppointments =
                await
                    OutlookCalendarService.GetCalendarEventsInRangeAsync(daysInPast, daysInFuture,outlookCalendarSpecificData);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.OutlookAppointmentsRead, GetSourceAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.GoogleAppointmentReading);


            var googleCalendarSpecificData = new Dictionary<string, object> { { "CalendarId", calenderId } };

            GetDestinationAppointments =
                await GoogleCalendarService.GetCalendarEventsInRangeAsync(daysInPast, daysInFuture, googleCalendarSpecificData);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.GoogleAppointmentRead, GetDestinationAppointments.Count);
            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);

            return true;
        }

        private List<Appointment> GetAppointmentsToDelete()
        {
            if (GetDestinationAppointments.Any() && GetSourceAppointments.Any())
            {
                return (from googleAppointment in GetDestinationAppointments
                        let isFound = GetSourceAppointments.Contains(googleAppointment)
                        where !isFound
                        select googleAppointment).ToList();
            }

            return GetDestinationAppointments;
        }

        private List<Appointment> GetAppointmentsToAdd()
        {
            if (GetDestinationAppointments.Any() && GetSourceAppointments.Any())
            {
                return (from outlookAppointment in GetSourceAppointments
                        let isFound = GetDestinationAppointments.Contains(outlookAppointment)
                        where !isFound
                        select outlookAppointment).ToList();
            }
            return GetSourceAppointments;
        }

        #endregion

        #region ICalendarUpdateService Members

        public async Task<bool> SyncCalendarAsync(Settings settings)
        {
            bool isSuccess = false;
            if (settings != null && settings.SavedCalendar != null)
            {
                SyncStatus = "Calendar Sync Mode : Outlook -> Google";
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                SyncStatus = string.Format("Sync Date Range - {0} - {1}", DateTime.Now.Subtract(new TimeSpan(settings.DaysInPast, 0, 0, 0)).ToString("D"),
                    DateTime.Now.Add(new TimeSpan(settings.DaysInFuture, 0, 0, 0)).ToString("D"));

                isSuccess = await GetAppointments(settings.DaysInPast, settings.DaysInFuture, settings.SavedCalendar.Id,
                            settings.OutlookSettings.OutlookProfileName, settings.OutlookSettings.OutlookCalendar);

                if (isSuccess)
                {
                    var googleCalendarSpecificData = new Dictionary<string, object>();
                    googleCalendarSpecificData.Add("CalendarId", settings.SavedCalendar.Id);


                    //Updating entry delete status
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToDelete);
                    //Getting appointments to delete
                    List<Appointment> appointmentsToDelete = GetAppointmentsToDelete();
                    //Updating Get entry delete status
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
                    //Updating delete status
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntries);
                    //Deleting entries
                    isSuccess = await GoogleCalendarService.DeleteCalendarEvent(appointmentsToDelete, googleCalendarSpecificData);

                    if (isSuccess)
                    {
                        //Deletion complete
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DeletingEntriesComplete);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.Line);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.ReadingEntriesToAdd);
                        List<Appointment> calendarAppointments = GetAppointmentsToAdd();
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, calendarAppointments.Count);
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddingEntries);
                        isSuccess = await GoogleCalendarService.AddCalendarEvent(calendarAppointments,
                            settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                            settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                            settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees), googleCalendarSpecificData);
                        if (isSuccess)
                        {
                            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddEntriesComplete);
                        }
                        else
                        {
                            SyncStatus = StatusHelper.GetMessage(SyncStateEnum.AddEntriesFailed);
                        }
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