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
//  *      Modified On:    05-02-2015 12:40 PM
//  *      FileName:       ICalendarUpdateService.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.ComponentModel;
using CalendarSyncPlus.Application.Utilities;
using CalendarSyncPlus.Application.Wrappers;
using CalendarSyncPlus.Domain.Models;

#endregion

namespace CalendarSyncPlus.Application.Services.CalendarUpdate
{
    public interface ICalendarUpdateService : INotifyPropertyChanged
    {
        #region Properties

        CalendarAppointments DestinationAppointments { get; set; }
        CalendarAppointments SourceAppointments { get; set; }
        Appointment CurrentAppointment { get; set; }
        string SyncStatus { get; set; }
        ICalendarService SourceCalendarService { get; set; }
        ICalendarService DestinationCalendarService { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <returns></returns>
        bool SyncCalendar(CalendarSyncProfile syncProfile, SyncCallback synccallback);

        #endregion
    }
}