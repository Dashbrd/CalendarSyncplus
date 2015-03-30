using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Application.Wrappers;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    public class FrequencyHelper
    {
        private static Frequency GetGoogleFrequency(string recurrence)
        {
            var frequency = new Frequency();
            //RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR
            string[] values = recurrence.Split(new[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in values)
            {
                if (value.Contains("="))
                {
                    var parameter = value.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    switch (parameter[0])
                    {
                        case "FREQ":
                            frequency.FrequencyType = GetFrequencyType(parameter[1]);
                            break;
                        case "BYDAY":
                            frequency.DaysOfWeek = GetDaysOfWeek(parameter[1]);
                            break;
                        case "UNTIL":
                            DateTime endDate;
                            if (DateTime.TryParseExact(parameter[1], new[] { "yyyyMMddTHHmmssZ" }, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out endDate))
                            {
                                frequency.EndDate = endDate;
                            }
                            break;
                        case "COUNT":
                            int count;
                            if (int.TryParse(parameter[1], out count))
                            {
                                frequency.Count = count;
                            }
                            break;
                        default:
                            //Do Nothing
                            break;
                    }
                }
            }
            return frequency;
        }

        private static List<DayOfWeek> GetDaysOfWeek(string parameter)
        {
            var daysOfWeek = new List<DayOfWeek>();
            string[] values = parameter.Split(',');

            foreach (var value in values)
            {
                switch (value)
                {
                    case "MO":
                        daysOfWeek.Add(DayOfWeek.Monday);
                        break;
                    case "TU":
                        daysOfWeek.Add(DayOfWeek.Tuesday);
                        break;
                    case "WE":
                        daysOfWeek.Add(DayOfWeek.Wednesday);
                        break;
                    case "TH":
                        daysOfWeek.Add(DayOfWeek.Thursday);
                        break;
                    case "FR":
                        daysOfWeek.Add(DayOfWeek.Friday);
                        break;
                    case "SA":
                        daysOfWeek.Add(DayOfWeek.Saturday);
                        break;
                    case "SU":
                        daysOfWeek.Add(DayOfWeek.Sunday);
                        break;
                }
            }
            return daysOfWeek;
        }

        private static FrequencyTypeEnum GetFrequencyType(string parameter)
        {
            switch (parameter)
            {
                case "WEEKLY":
                    return FrequencyTypeEnum.Weekly;
                case "DAILY":
                    return FrequencyTypeEnum.Daily;
                case "MONTHLY":
                    return FrequencyTypeEnum.Monthly;
                case "YEARLY":
                    return FrequencyTypeEnum.Yearly;
            }
            return FrequencyTypeEnum.None;
        }


        public static List<Appointment> SplitRecurringAppointments(Appointment recurringAppointment, string recurrence,
            DateTime startDate, DateTime endDate)
        {
            var frequency = GetGoogleFrequency(recurrence);
            frequency.StartDate = recurringAppointment.StartTime.GetValueOrDefault();

            var appointmentList = new List<Appointment>();
            var dateTime = startDate.Date;
            while (endDate.CompareTo(dateTime) > 0)
            {
                if (frequency.ValidateDate(dateTime))
                {
                    var newAppointment = (Appointment)recurringAppointment.Clone();
                    newAppointment.AppointmentId = string.Format("{0}_{1}", recurringAppointment.AppointmentId,
                        dateTime.ToString("yy-MM-dd"));
                    newAppointment.StartTime =
                        dateTime.Date.Add(new TimeSpan(recurringAppointment.StartTime.GetValueOrDefault().Hour,
                            recurringAppointment.StartTime.GetValueOrDefault().Minute,
                            recurringAppointment.StartTime.GetValueOrDefault().Second));
                    newAppointment.EndTime =
                        dateTime.Date.Add(new TimeSpan(recurringAppointment.EndTime.GetValueOrDefault().Hour,
                            recurringAppointment.EndTime.GetValueOrDefault().Minute,
                            recurringAppointment.EndTime.GetValueOrDefault().Second));
                    appointmentList.Add(newAppointment);
                }

                dateTime = dateTime.AddDays(1);
            }
            return appointmentList;
        }

    }
}
