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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Foundation;

using OutlookGoogleSyncRefresh.Application.Services.Google;
using OutlookGoogleSyncRefresh.Application.Services.Outlook;
using OutlookGoogleSyncRefresh.Application.Utilities;
using OutlookGoogleSyncRefresh.Common.Log;
using OutlookGoogleSyncRefresh.Domain.Models;

#endregion

namespace OutlookGoogleSyncRefresh.Application.Services.CalendarUpdate
{
    [Export(typeof (ICalendarUpdateService))]
    public class OutlookGoogleCalendarUpdateService : Model, ICalendarUpdateService
    {
        private readonly ApplicationLogger _applicationLogger;

        #region Fields

        private Appointment _currentAppointment;
        private List<Appointment> _googleAppointments;
        private List<Appointment> _outlookAppointments;
        private string _syncStatus;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public OutlookGoogleCalendarUpdateService(IOutlookCalendarService outlookCalendarService,
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

        public List<Appointment> GoogleAppointments
        {
            get { return _googleAppointments; }
            set { SetProperty(ref _googleAppointments, value); }
        }

        public List<Appointment> OutlookAppointments
        {
            get { return _outlookAppointments; }
            set { SetProperty(ref _outlookAppointments, value); }
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
            GoogleAppointments =
                await GoogleCalendarService.GetCalendarEventsInRangeAsync(daysInPast, daysInFuture, calenderId);
            OutlookAppointments =
                await
                    OutlookCalendarService.GetOutlookAppointmentsAsync(daysInPast, daysInFuture, profileName,
                        outlookCalendar);

            return true;
        }

        private List<Appointment> GetAppointmentsToDelete()
        {
            if (GoogleAppointments.Any() && OutlookAppointments.Any())
            {
                return (from googleAppointment in GoogleAppointments
                    let isFound = OutlookAppointments.Contains(googleAppointment)
                    where !isFound
                    select googleAppointment).ToList();
            }

            return GoogleAppointments;
        }

        private List<Appointment> GetAppointmentsToAdd()
        {
            if (GoogleAppointments.Any() && OutlookAppointments.Any())
            {
                return (from outlookAppointment in OutlookAppointments
                    let isFound = GoogleAppointments.Contains(outlookAppointment)
                    where !isFound
                    select outlookAppointment).ToList();
            }
            return OutlookAppointments;
        }

        #endregion

        #region ICalendarUpdateService Members

        public async Task<bool> SyncCalendarAsync(Settings settings)
        {
            bool isSuccess = false;
            if (settings != null && settings.SavedCalendar != null)
            {
                _applicationLogger.LogInfo("Reading appointments...");
                isSuccess =
                    await
                        GetAppointments(settings.DaysInPast, settings.DaysInFuture, settings.SavedCalendar.Id,
                            settings.OutlookSettings.OutlookProfileName, settings.OutlookSettings.OutlookCalendar);
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.SourceAppointmentRead, OutlookAppointments.Count);
                SyncStatus = StatusHelper.GetMessage(SyncStateEnum.DestinationAppointmentRead, GoogleAppointments.Count);
                if (isSuccess)
                {
                    _applicationLogger.LogInfo("Getting appointments to delete...");
                    List<Appointment> appointmentsToDelete = GetAppointmentsToDelete();
                    SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToDelete, appointmentsToDelete.Count);
                    _applicationLogger.LogInfo("Deleting calendar events...");
                    isSuccess =
                        await GoogleCalendarService.DeleteCalendarEvent(appointmentsToDelete, settings.SavedCalendar.Id);
                    if (isSuccess)
                    {
                        _applicationLogger.LogInfo("Getting appointments to add...");
                        List<Appointment> calendarAppointments = GetAppointmentsToAdd();
                        SyncStatus = StatusHelper.GetMessage(SyncStateEnum.EntriesToAdd, calendarAppointments.Count);
                        _applicationLogger.LogInfo("Adding appointments...");
                        isSuccess =
                            await
                                GoogleCalendarService.AddCalendarEvent(calendarAppointments, settings.SavedCalendar.Id,
                                    settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description),
                                    settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders),
                                    settings.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Attendees));
                        _applicationLogger.LogInfo("Update complete.");
                    }
                }
            }
            return isSuccess;
        }

        #endregion
    }
}