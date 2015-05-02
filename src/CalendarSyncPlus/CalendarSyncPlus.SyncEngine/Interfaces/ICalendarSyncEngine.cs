using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.SyncEngine.Interfaces
{
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

        List<Appointment> SourceAppointmentsToAdd { get; set; }

        List<Appointment> DestAppointmentsToUpdate { get; set; }

        List<Appointment> DestAppointmentsToDelete { get; set; }

        List<Appointment> DestAppointmentsToAdd { get; set; }

        List<Appointment> DestOrphanEntries { get; set; }

        bool GetSourceEntriesToDelete(CalendarSyncProfile syncProfile, List<Appointment> sourceList,
            List<Appointment> destinationList);

        bool GetSourceEntriesToAdd(CalendarSyncProfile syncProfile, List<Appointment> sourceList,
            List<Appointment> destinationList);

        bool GetDestEntriesToDelete(CalendarSyncProfile syncProfile, List<Appointment> sourceList,
            List<Appointment> destinationList);

        bool GetDestEntriesToAdd(CalendarSyncProfile syncProfile, List<Appointment> sourceList,
            List<Appointment> destinationList);

    }
}
