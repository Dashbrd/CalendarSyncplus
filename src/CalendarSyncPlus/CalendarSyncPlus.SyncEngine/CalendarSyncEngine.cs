using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.SyncEngine.Helpers;
using CalendarSyncPlus.SyncEngine.Interfaces;

namespace CalendarSyncPlus.SyncEngine
{
    [Export(typeof(ICalendarSyncEngine))]
    public class CalendarSyncEngine : ICalendarSyncEngine
    {
        public List<Appointment> SourceAppointmentsToUpdate { get; set; }

        public List<Appointment> SourceOrphanEntries { get; set; }

        public List<Appointment> SourceAppointmentsToDelete { get; set; }

        public List<Appointment> SourceAppointmentsToAdd { get; set; }

        public List<Appointment> DestAppointmentsToUpdate { get; set; }

        public List<Appointment> DestAppointmentsToDelete { get; set; }

        public List<Appointment> DestAppointmentsToAdd { get; set; }

        public List<Appointment> DestOrphanEntries { get; set; }


        #region Private Methods

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <param name="destAppointmentsToDelete"></param>
        /// <param name="sourceAppointmentsToUpdate"></param>
        /// <param name="destOrphanEntries"></param>
        /// <returns></returns>
        void GetAppointmentsToDelete(CalendarSyncProfile syncProfile,
            List<Appointment> sourceList, List<Appointment> destinationList, List<Appointment> destAppointmentsToDelete,
            List<Appointment> sourceAppointmentsToUpdate, List<Appointment> destOrphanEntries)
        {
            bool addDescription =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
            bool addReminders =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);
            bool addAttendeesToDescription =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription);

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
                            if (!CompareAppointments(destAppointment, sourceAppointment, addDescription, addReminders, addAttendeesToDescription))
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
                        destOrphanEntries.Add(destAppointment);
                    }
                }
                else
                {
                    //If source appointment is not null, means it is a copy of an existing entry in Source calendar
                    var sourceAppointment = sourceList.FirstOrDefault(t => t.CompareSourceId(destAppointment));
                    if (sourceAppointment != null)
                    {
                        //If any entry is found in source appointment and its contents are not equal to source appointment
                        if (!CompareAppointments(destAppointment, sourceAppointment, addDescription, addReminders, addAttendeesToDescription))
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
        }

        /// <summary>
        ///     Gets appointments to add in the destination calendar
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <param name="appointmentsToAdd"></param>
        /// <returns></returns>
        void GetAppointmentsToAdd(CalendarSyncProfile syncProfile, List<Appointment> sourceList,
            List<Appointment> destinationList, List<Appointment> appointmentsToAdd)
        {
            if (!destinationList.Any())
            {
                appointmentsToAdd.AddRange(sourceList);
                //All entries need to be added
                return;
            }
            bool addDescription =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
            bool addReminders =
            syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);
            bool addAttendeesToDescription =
            syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription);

            foreach (Appointment sourceAppointment in sourceList)
            {
                if (sourceAppointment.SourceId == null)
                {
                    var destinationAppointment = destinationList.FirstOrDefault(t =>
                                    CompareAppointments(sourceAppointment, t, addDescription, addReminders, addAttendeesToDescription));
                    if (destinationAppointment == null)
                    {
                        appointmentsToAdd.Add(sourceAppointment);
                    }
                }
            }
        }


        bool CompareAppointments(Appointment destAppointment,
          Appointment sourceAppointment, bool addDescription, bool addReminders, bool addAttendeesToDescription)
        {
            bool isFound = destAppointment.Equals(sourceAppointment);
            //If both entries have same content

            if (isFound)
            {
                //If description flag is on, compare description
                if (addDescription)
                {
                    if (!sourceAppointment.CompareDescription(destAppointment, addAttendeesToDescription))
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


        #endregion
        public bool GetSourceEntriesToDelete(CalendarSyncProfile syncProfile, List<Appointment> sourceList, List<Appointment> destinationList)
        {
            GetAppointmentsToDelete(syncProfile, destinationList, sourceList, SourceAppointmentsToDelete, DestAppointmentsToUpdate, SourceOrphanEntries);
            return true;
        }

        public bool GetSourceEntriesToAdd(CalendarSyncProfile syncProfile, List<Appointment> sourceList, List<Appointment> destinationList)
        {
            GetAppointmentsToAdd(syncProfile, destinationList, sourceList, SourceAppointmentsToAdd);
            return true;
        }

        public bool GetDestEntriesToDelete(CalendarSyncProfile syncProfile, List<Appointment> sourceList, List<Appointment> destinationList)
        {
            GetAppointmentsToDelete(syncProfile, sourceList, destinationList, DestAppointmentsToDelete, SourceAppointmentsToUpdate, DestOrphanEntries);
            return true;
        }

        public bool GetDestEntriesToAdd(CalendarSyncProfile syncProfile, List<Appointment> sourceList, List<Appointment> destinationList)
        {
            GetAppointmentsToAdd(syncProfile, sourceList, destinationList, DestAppointmentsToAdd);
            return true;
        }

        public void Clear()
        {
            SourceAppointmentsToAdd = new List<Appointment>();
            DestAppointmentsToAdd = new List<Appointment>();

            SourceAppointmentsToDelete = new List<Appointment>();
            DestAppointmentsToDelete = new List<Appointment>();

            SourceAppointmentsToUpdate = new List<Appointment>();
            DestAppointmentsToUpdate = new List<Appointment>();

            SourceOrphanEntries = new List<Appointment>();
            DestOrphanEntries = new List<Appointment>();
        }
    }
}
