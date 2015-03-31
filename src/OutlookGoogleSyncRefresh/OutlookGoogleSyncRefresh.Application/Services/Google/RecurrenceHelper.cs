using System;
using System.Collections.Generic;
using System.Globalization;
using CalendarSyncPlus.Domain.Models;

namespace CalendarSyncPlus.Application.Services.Google
{
    internal class RecurrenceHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recurringAppointment"></param>
        /// <param name="recurrence"></param>
        /// <param name="startDateRange"></param>
        /// <param name="endDateRange"></param>
        /// <returns></returns>
        public static List<Appointment> SplitRecurringAppointments(Appointment recurringAppointment, string recurrence,
            DateTime startDateRange, DateTime endDateRange)
        {
            Recurrence frequency = Recurrence.Parse(recurrence,recurringAppointment.StartTime.GetValueOrDefault());
            var appointmentList = new List<Appointment>();
            DateTime dateTime = startDateRange.Date;
            while (endDateRange.CompareTo(dateTime) > 0)
            {
                if (frequency.ValidateDate(dateTime))
                {
                    var newAppointment = (Appointment) recurringAppointment.Clone();
                    newAppointment.AppointmentId = string.Format("{0}_{1}", recurringAppointment.AppointmentId,
                        dateTime.ToString("yy-MM-dd"));
                    newAppointment.StartTime = dateTime.Date.Add(recurringAppointment.StartTime.GetValueOrDefault().TimeOfDay);
                    newAppointment.EndTime = dateTime.Date.Add(recurringAppointment.EndTime.GetValueOrDefault().TimeOfDay);
                    appointmentList.Add(newAppointment);
                }

                dateTime = dateTime.AddDays(1);
            }
            return appointmentList;
        }
    }

}