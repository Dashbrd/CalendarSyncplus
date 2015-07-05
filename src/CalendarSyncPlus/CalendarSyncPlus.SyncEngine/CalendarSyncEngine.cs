using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CalendarSyncPlus.Domain.Helpers;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Domain.Models.Preferences;
using CalendarSyncPlus.Domain.Wrappers;
using CalendarSyncPlus.SyncEngine.Helpers;
using CalendarSyncPlus.SyncEngine.Interfaces;

namespace CalendarSyncPlus.SyncEngine
{
    /// <summary>
    /// 
    /// </summary>
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

        public bool GetSourceEntriesToDelete(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList)
        {
            EvaluateAppointmentsToDelete(syncProfile, destinationList, sourceList, SourceAppointmentsToDelete,
                SourceAppointmentsToUpdate, DestAppointmentsToUpdate, SourceOrphanEntries);
            return true;
        }

        public bool GetSourceEntriesToAdd(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList)
        {
            EvaluateAppointmentsToAdd(syncProfile, destinationList, sourceList, SourceAppointmentsToAdd);
            return true;
        }

        public bool GetDestEntriesToDelete(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList)
        {
            EvaluateAppointmentsToDelete(syncProfile, sourceList, destinationList, DestAppointmentsToDelete,
                DestAppointmentsToUpdate, SourceAppointmentsToUpdate, DestOrphanEntries);
            return true;
        }

        public bool GetDestEntriesToAdd(CalendarSyncProfile syncProfile, AppointmentsWrapper sourceList,
            AppointmentsWrapper destinationList)
        {
            EvaluateAppointmentsToAdd(syncProfile, sourceList, destinationList, DestAppointmentsToAdd);
            return true;
        }

        /// <summary>
        /// </summary>
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

        #region Private Methods

        /// <summary>
        /// </summary>
        /// <param name="syncProfile"></param>
        /// <param name="sourceList"></param>
        /// <param name="destinationList"></param>
        /// <param name="destAppointmentsToDelete"></param>
        /// <param name="destAppointmentsToUpdate"></param>
        /// <param name="sourceAppointmentsToUpdate"></param>
        /// <param name="destOrphanEntries"></param>
        /// <returns>
        /// </returns>
        private void EvaluateAppointmentsToDelete(CalendarSyncProfile syncProfile,
            AppointmentsWrapper sourceList, AppointmentsWrapper destinationList,
            List<Appointment> destAppointmentsToDelete,
            List<Appointment> destAppointmentsToUpdate, List<Appointment> sourceAppointmentsToUpdate,
            List<Appointment> destOrphanEntries)
        {
            var addDescription =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Description);
            var addReminders =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.Reminders);
            var addAttendeesToDescription =
                syncProfile.CalendarEntryOptions.HasFlag(CalendarEntryOptionsEnum.AttendeesToDescription);

            if (!destinationList.Any())
            {
                foreach (var appointment in sourceList)
                {
                    if (appointment.ChildId != null)
                    {
                        var key = AppointmentHelper.GetChildEntryKey(sourceList.CalendarId);
                        if (!appointment.ExtendedProperties.ContainsKey(key))
                        {
                            appointment.ExtendedProperties.Remove(key);
                        }
                        sourceAppointmentsToUpdate.AddCompareForUpdate(appointment);
                    }
                }
                return;
            }

            foreach (var destAppointment in destinationList)
            {
                //If SourceId is null, it is not a copy of any entry from the selected source calendar
                if (destAppointment.SourceId == null)
                {
                    if (syncProfile.SyncMode == SyncModeEnum.OneWay)
                    {
                        //If mode is one way & user has disabled delete, do not remove this entry, as this is an original entry in the calendar
                        //Else this entry is not a copy of any appointment in source calendar so delete it
                        destOrphanEntries.Add(destAppointment);
                    }
                    else
                    {
                        if (destAppointment.ChildId == null)
                        {
                            var childAppointment = sourceList.FirstOrDefault(t => destAppointment.CompareSourceId(t));
                            if (childAppointment != null)
                            {
                                destAppointment.ChildId = childAppointment.AppointmentId;
                                var key = childAppointment.GetChildEntryKey();
                                if (!destAppointment.ExtendedProperties.ContainsKey(key))
                                {
                                    destAppointment.ExtendedProperties.Add(key, childAppointment.AppointmentId);
                                }
                                else
                                {
                                    destAppointment.ExtendedProperties[key] = childAppointment.AppointmentId;
                                }
                                destAppointmentsToUpdate.AddCompareForUpdate(destAppointment);
                            }
                        }
                        else if (syncProfile.SyncSettings.KeepLastModifiedVersion)
                        {
                            var childAppointment =
                                sourceList.FirstOrDefault(t => t.AppointmentId.Equals(destAppointment.ChildId));
                            if (childAppointment == null)
                            {
                                destAppointmentsToDelete.Add(destAppointment);
                            }
                        }
                    }
                }
                else
                {
                    //If the mode is two way, look for its parent copy in Source calendar
                    Appointment sourceAppointment;
                    if (syncProfile.SyncMode == SyncModeEnum.TwoWay
                        && syncProfile.SyncSettings.KeepLastModifiedVersion)
                    {
                        //If no entry was found, it is original entry of the calendar, Ignore
                        //If a child entry is found in source calendar, compare
                        sourceAppointment = sourceList.FirstOrDefault(t => t.CompareSourceId(destAppointment));
                        if (sourceAppointment != null)
                        {
                            //If any entry is found in source appointment and its contents are not equal to source appointment,
                            //If an entry is found and i same, ignore
                            if (!CompareAppointments(destAppointment, sourceAppointment, addDescription,
                                addReminders, addAttendeesToDescription))
                            {
                                if (sourceAppointment.LastModified.HasValue && destAppointment.LastModified.HasValue)
                                {
                                    if (destAppointment.LastModified.GetValueOrDefault() >
                                        sourceAppointment.LastModified.GetValueOrDefault())
                                    {
                                        sourceAppointment.CopyDetail(destAppointment,
                                            syncProfile.CalendarEntryOptions);
                                        sourceAppointmentsToUpdate.AddCompareForUpdate(sourceAppointment);
                                        continue;
                                    }
                                }
                                //Destination Calendar Entry is not Matching its Source Calendar Entry, Update it
                                destAppointment.CopyDetail(sourceAppointment, syncProfile.CalendarEntryOptions);
                                destAppointmentsToUpdate.AddCompareForUpdate(destAppointment);
                                continue;
                            }
                        }
                    }

                    //If source appointment is not null, means it is a copy of an existing entry in Source calendar
                    sourceAppointment = sourceList.FirstOrDefault(t => t.CompareSourceId(destAppointment));
                    if (sourceAppointment != null)
                    {
                        //If any entry is found in source appointment and its contents are not equal to source appointment
                        if (!CompareAppointments(destAppointment, sourceAppointment, addDescription, addReminders,
                            addAttendeesToDescription))
                        {
                            destAppointment.CopyDetail(sourceAppointment, syncProfile.CalendarEntryOptions);
                            destAppointmentsToUpdate.AddCompareForUpdate(destAppointment);
                        }
                    }
                    else
                    {
                        //No parent entry is found, delete it
                        sourceAppointment = sourceList.FirstOrDefault(t =>
                            CompareAppointments(destAppointment, t, addDescription, addReminders,
                                addAttendeesToDescription));
                        if (sourceAppointment == null)
                        {
                            //If parent entry isn't found
                            destAppointmentsToDelete.Add(destAppointment);
                        }
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
        /// <returns>
        /// </returns>
        private void EvaluateAppointmentsToAdd(CalendarSyncProfile syncProfile, List<Appointment> sourceList,
            List<Appointment> destinationList, List<Appointment> appointmentsToAdd)
        {
            if (!destinationList.Any())
            {
                appointmentsToAdd.AddRange(sourceList);
                //All entries need to be added
                return;
            }
            
            foreach (var sourceAppointment in sourceList)
            {
                if (sourceAppointment.SourceId == null)
                {
                    var destinationAppointment = destinationList.FirstOrDefault(t =>
                        t.Equals(sourceAppointment));
                    if (destinationAppointment == null)
                    {
                        appointmentsToAdd.Add(sourceAppointment);
                    }
                }
            }
            
        }

        /// <summary>
        /// </summary>
        /// <param name="destAppointment"></param>
        /// <param name="sourceAppointment"></param>
        /// <param name="addDescription"></param>
        /// <param name="addReminders"></param>
        /// <param name="addAttendeesToDescription"></param>
        /// <returns>
        /// </returns>
        private bool CompareAppointments(Appointment destAppointment,
            Appointment sourceAppointment, bool addDescription, bool addReminders, bool addAttendeesToDescription)
        {
            var isFound = destAppointment.Equals(sourceAppointment);
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
    }
}