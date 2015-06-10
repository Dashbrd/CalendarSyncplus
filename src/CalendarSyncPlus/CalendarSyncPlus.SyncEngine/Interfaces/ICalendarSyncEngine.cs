using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Wrappers;

namespace CalendarSyncPlus.SyncEngine.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICalendarSyncEngine
    {
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> SourceAppointmentsToUpdate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> SourceOrphanEntries { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> SourceAppointmentsToDelete { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> SourceAppointmentsToAdd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> DestAppointmentsToUpdate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> DestAppointmentsToDelete { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> DestAppointmentsToAdd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        List<Appointment> DestOrphanEntries { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns></returns>
        bool GetSourceEntriesToDelete(CalendarSyncProfile syncProfile, CalendarAppointments sourceList,
            CalendarAppointments destinationList);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns></returns>
        bool GetSourceEntriesToAdd(CalendarSyncProfile syncProfile, CalendarAppointments sourceList,
            CalendarAppointments destinationList);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns></returns>
        bool GetDestEntriesToDelete(CalendarSyncProfile syncProfile, CalendarAppointments sourceList,
            CalendarAppointments destinationList);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns></returns>
        bool GetDestEntriesToAdd(CalendarSyncProfile syncProfile, CalendarAppointments sourceList,
            CalendarAppointments destinationList);
        /// <summary>
        /// 
        /// </summary>
        void Clear();
    }
}
