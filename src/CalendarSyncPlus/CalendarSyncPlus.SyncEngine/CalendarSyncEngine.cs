using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.SyncEngine.Helpers;
using CalendarSyncPlus.SyncEngine.Interfaces;

namespace CalendarSyncPlus.SyncEngine
{
    public class CalendarSyncEngine : ICalendarSyncEngine
    {

        public List<Appointment> SourceAppointmentsToUpdate { get; set; }

        public List<Appointment> SourceAppointmentsToDelete { get; set; }

        public List<Appointment> SourceAppointmentsToAdd { get; set; }

        public List<Appointment> DestAppointmentsToUpdate { get; set; }

        public List<Appointment> DestAppointmentsToDelete { get; set; }

        public List<Appointment> DestAppointmentsToAdd { get; set; }


        public bool RunOptimizedAlgorithm(CalendarSyncProfile syncProfile,
            List<Appointment> sourceList, List<Appointment> destinationList)
        {
            return true;
        }


        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns></returns>
        public List<Appointment> GetAppointmentsToDelete(CalendarSyncProfile syncProfile,
            List<Appointment> sourceList, List<Appointment> destinationList)
        {
            bool addDescription =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
            bool addReminders =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);

            var destAppointmentsToDelete = new List<Appointment>();
            var sourceAppointmentsToUpdate = new List<Appointment>();

            foreach (Appointment destAppointment in destinationList)
            {
                //If SourceId is null, it is not a copy of any entry from the selected source calendar
                if (destAppointment.SourceId == null)
                {
                    //If the mode is two way, look for its parent copy in Source calendar
                    if (syncProfile.SyncSettings.SyncMode == SyncModeEnum.TwoWay)
                    {
                        //If no entry was found, it is original entry of the calendar, Ignore
                        //If a child entry is found in source calendar, compare
                        var sourceAppointment = sourceList.FirstOrDefault(t => destAppointment.CompareSourceId(t));
                        if (sourceAppointment != null)
                        {
                            //If any entry is found in source appointment and its contents are not equal to source appointment,
                            //If an entry is found and i same, ignore
                            if (!CompareAppointments(destAppointment, sourceAppointment, addDescription, addReminders))
                            {
                                if (syncProfile.SyncSettings.KeepLastModifiedVersion)
                                {
                                    if (destAppointment.LastModified.HasValue && sourceAppointment.LastModified.HasValue)
                                    {
                                        if (destAppointment.LastModified.GetValueOrDefault() >
                                            sourceAppointment.LastModified.GetValueOrDefault())
                                        {
                                            sourceAppointment.CopyDetail(destAppointment, syncProfile.CalendarEntryOptions);
                                            sourceAppointmentsToUpdate.Add(sourceAppointment);
                                            continue;
                                        }
                                    }
                                }
                                //Destination Calendar Entry is not Matching its Source Calendar Entry, Delete it
                                destAppointmentsToDelete.Add(destAppointment);
                            }
                        }
                    }
                    else
                    {
                        //If mode is one way & user has disabled delete, do not remove this entry, as this is an original entry in the calendar
                        //Else this entry is not a copy of any appointment in source calendar so delete it
                        if (!syncProfile.SyncSettings.DisableDelete)
                        {
                            destAppointmentsToDelete.Add(destAppointment);
                        }
                    }
                }
                else
                {
                    //If source appointment is not null, means it is a copy of an existing entry in Source calendar
                    var sourceAppointment = sourceList.FirstOrDefault(t => t.CompareSourceId(destAppointment));
                    if (sourceAppointment != null)
                    {
                        //If any entry is found in source appointment and its contents are not equal to source appointment
                        if (!CompareAppointments(destAppointment, sourceAppointment, addDescription, addReminders))
                        {
                            destAppointmentsToDelete.Add(destAppointment);
                        }
                    }
                    else
                    {
                        //If parent entry isn't found
                        destAppointmentsToDelete.Add(destAppointment);
                    }
                }
            }
            return destAppointmentsToDelete;
        }

        /// <summary>
        ///     Gets appointments to add in the destination calendar
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <returns></returns>
        public List<Appointment> GetAppointmentsToAdd(CalendarSyncProfile syncProfile, List<Appointment> sourceList,
            List<Appointment> destinationList)
        {
            if (!destinationList.Any())
            {
                //All entries need to be added
                return sourceList;
            }
            bool addDescription =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
            bool addReminders =
            syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);

            var appointmentsToAdd = new List<Appointment>();
            foreach (Appointment sourceAppointment in sourceList)
            {
                if (sourceAppointment.SourceId == null)
                {
                    var destinationAppointment = destinationList.FirstOrDefault(t =>
                                    CompareAppointments(sourceAppointment, t, addDescription, addReminders));
                    if (destinationAppointment == null)
                    {
                        appointmentsToAdd.Add(sourceAppointment);
                    }
                }
            }
            return appointmentsToAdd;
        }


        private bool CompareAppointments(Appointment destAppointment,
          Appointment sourceAppointment, bool addDescription, bool addReminders)
        {
            bool isFound = destAppointment.Equals(sourceAppointment);
            //If both entries have same content

            if (isFound)
            {
                //If description flag is on, compare description
                if (addDescription)
                {
                    if (!sourceAppointment.CompareDescription(destAppointment))
                    {
                        isFound = false;
                    }
                }
            }

            if (isFound)
            {
                //If reminder flag is on, compare reminder
                if (addReminders)
                {
                    //Check if reminders match
                    if (sourceAppointment.ReminderSet != destAppointment.ReminderSet)
                    {
                        isFound = false;
                    }
                    else if (sourceAppointment.ReminderSet)
                    {
                        if (sourceAppointment.ReminderMinutesBeforeStart !=
                            destAppointment.ReminderMinutesBeforeStart)
                        {
                            isFound = false;
                        }
                    }
                }
            }

            return isFound;
        }
    }
}
