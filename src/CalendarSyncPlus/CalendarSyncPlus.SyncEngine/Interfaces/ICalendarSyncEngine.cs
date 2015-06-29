using System.Collections.Generic;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;

namespace CalendarSyncPlus.SyncEngine.Interfaces
{
    /// <summary>
    /// </summary>
    public interface ICalendarSyncEngine
    {
        /// <summary>
        /// </summary>
        List<Appointment> SourceAppointmentsToUpdate { get; set; }

        /// <summary>
        /// </summary>
        List<Appointment> SourceOrphanEntries { get; set; }

        /// <summary>
        /// </summary>
        List<Appointment> SourceAppointmentsToDelete { get; set; }

        /// <summary>
        /// </summary>
        List<Appointment> SourceAppointmentsToAdd { get; set; }

        /// <summary>
        /// </summary>
        List<Appointment> DestAppointmentsToUpdate { get; set; }

        /// <summary>
        /// </summary>
        List<Appointment> DestAppointmentsToDelete { get; set; }

        /// <summary>
        /// </summary>
        List<Appointment> DestAppointmentsToAdd { get; set; }

        /// <summary>
        /// </summary>
        List<Appointment> DestOrphanEntries { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetSourceEntriesToDelete(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetSourceEntriesToAdd(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetDestEntriesToDelete(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList);

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns>
        /// </returns>
        bool GetDestEntriesToAdd(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList);

        /// <summary>
        /// </summary>
        void Clear();
    }
}