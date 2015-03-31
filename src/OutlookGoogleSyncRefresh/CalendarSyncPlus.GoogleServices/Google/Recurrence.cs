using System;
using System.Collections.Generic;
using System.Globalization;

namespace CalendarSyncPlus.GoogleServices.Google
{
    internal class Recurrence
    {
        #region Constructor
        private Recurrence(DateTime startDate)
        {
            StartDate = startDate;
            Count = -1;
        }
        #endregion
        #region Properties

        public RecurrenceTypeEnum RecurrenceType { get; set; }

        public DateTime StartDate { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }

        public int RepeatGap { get; set; }

        public int Count { get; set; }

        public DateTime? EndDate { get; set; }
        #endregion

        #region Private Methods
        private bool ValidateWeeklyOccurrence(DateTime dateTime)
        {
            if (!DaysOfWeek.Contains(dateTime.DayOfWeek))
            {
                return false;
            }

            if (Count > 0)
            {
                int occurrenceCount = 0;
                DateTime currentDate = StartDate.Date;
                while (dateTime.Date.CompareTo(currentDate) > 0)
                {
                    if (DaysOfWeek.Contains(currentDate.DayOfWeek))
                    {
                        occurrenceCount++;
                    }
                    currentDate = currentDate.AddDays(1);
                }

                if (occurrenceCount > Count)
                {
                    return false;
                }
            }

            if (EndDate != null)
            {
                if (dateTime.Date.CompareTo(EndDate.GetValueOrDefault().Date) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ValidateDailyOccurence(DateTime dateTime)
        {
            if (Count > 0)
            {
                if (dateTime.Date.Subtract(StartDate.Date).Days > Count)
                {
                    return false;
                }
            }
            if (EndDate != null)
            {
                if (dateTime.Date.CompareTo(EndDate.GetValueOrDefault().Date) < 0)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Public Methods
        public bool ValidateDate(DateTime dateTime)
        {
            if (dateTime.CompareTo(StartDate) < 0)
            {
                return false;
            }

            switch (RecurrenceType)
            {
                case RecurrenceTypeEnum.Daily:
                    return ValidateDailyOccurence(dateTime);
                case RecurrenceTypeEnum.Monthly:
                    break;
                case RecurrenceTypeEnum.Weekly:
                    return ValidateWeeklyOccurrence(dateTime);
                case RecurrenceTypeEnum.Yearly:
                    break;
            }

            return false;
        }
        #endregion

        public static Recurrence Parse(string recurrence, DateTime startDate)
        {
            var frequency = new Recurrence(startDate);

            string[] values = recurrence.Split(new[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in values)
            {
                if (value.Contains("="))
                {
                    string[] parameter = value.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    switch (parameter[0])
                    {
                        case "FREQ":
                            frequency.RecurrenceType = GetFrequencyType(parameter[1]);
                            break;
                        case "BYDAY":
                            frequency.DaysOfWeek = GetDaysOfWeek(parameter[1]);
                            break;
                        case "UNTIL":
                            DateTime endDate;
                            if (DateTime.TryParseExact(parameter[1], new[] { "yyyyMMddTHHmmssZ" },
                                CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal, out endDate))
                            {
                                frequency.EndDate = endDate;
                            }
                            break;
                        case "COUNT":
                            int count;
                            if (Int32.TryParse(parameter[1], out count))
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

            foreach (string value in values)
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

        private static RecurrenceTypeEnum GetFrequencyType(string parameter)
        {
            switch (parameter)
            {
                case "WEEKLY":
                    return RecurrenceTypeEnum.Weekly;
                case "DAILY":
                    return RecurrenceTypeEnum.Daily;
                case "MONTHLY":
                    return RecurrenceTypeEnum.Monthly;
                case "YEARLY":
                    return RecurrenceTypeEnum.Yearly;
            }
            return RecurrenceTypeEnum.None;
        }
    }

    internal enum RecurrenceTypeEnum
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
