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
//  *      Modified On:    05-02-2015 12:40 PM
//  *      FileName:       ICalendarUpdateService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.ComponentModel;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Metrics;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.Services.Utilities;

#endregion

namespace CalendarSyncPlus.Services.Calendars.Interfaces
{
    public interface ICalendarUpdateService : INotifyPropertyChanged
    {
        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="syncMetric"></param>
        /// <param name="synccallback"></param>
        /// <returns>
        /// </returns>
        bool SyncCalendar(CalendarSyncProfile syncProfile, SyncMetric syncMetric, SyncCallback synccallback);

        #endregion

        #region Properties

        CalendarAppointments DestinationAppointments { get; set; }
        CalendarAppointments SourceAppointments { get; set; }
        Appointment CurrentAppointment { get; set; }
        string SyncStatus { get; set; }
        ICalendarService SourceCalendarService { get; set; }
        ICalendarService DestinationCalendarService { get; set; }

        #endregion
    }
}