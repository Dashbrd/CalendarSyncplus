using System;
using System.Collections.Generic;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Wrappers
{
    public class Frequency
    {
        public Frequency()
        {
            Count = -1;
        }

        public FrequencyTypeEnum FrequencyType { get; set; }

        public DateTime StartDate { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }

        public int RepeatGap { get; set; }

        public int Count { get; set; }

        public DateTime? EndDate { get; set; }

        public bool ValidateDate(DateTime dateTime)
        {
            if (dateTime.CompareTo(StartDate) < 0)
            {
                return false;
            }

            switch (FrequencyType)
            {
                case FrequencyTypeEnum.Daily:
                    return ValidateDailyOccurence(dateTime);
                case FrequencyTypeEnum.Monthly:
                    break;
                case FrequencyTypeEnum.Weekly:
                    return ValidateWeeklyOccurrence(dateTime);
                case FrequencyTypeEnum.Yearly:
                    break;
            }

            return false;
        }

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
    }
}