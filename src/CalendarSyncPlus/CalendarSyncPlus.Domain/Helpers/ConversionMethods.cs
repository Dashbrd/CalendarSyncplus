using CalendarSyncPlus.Domain.Models;
using System;

namespace CalendarSyncPlus.Domain.Helpers
{
    public static class ConversionMethods
    {
        public static string ConvertDaysToString(DayOfWeekMask source)
        {
            string day = "";
            foreach (var token in Enum.GetValues(typeof(DayOfWeekMask)))
            {
                DayOfWeekMask value = source & (DayOfWeekMask)token;
                switch (value)
                {
                    case DayOfWeekMask.Monday:
                        day += "Mo";
                        break;
                    case DayOfWeekMask.Tuesday:
                        day += "Mo";
                        break;
                    case DayOfWeekMask.Wednesday:
                        day += "Mo";
                        break;
                    case DayOfWeekMask.Thursday:
                        day += "Mo";
                        break;
                    case DayOfWeekMask.Friday:
                        day += "Mo";
                        break;
                    case DayOfWeekMask.Saturday:
                        day += "Mo";
                        break;
                    case DayOfWeekMask.Sunday:
                        day += "Mo";
                        break;
                }
            }
            return day;
        }

        public static DayOfWeekMask ConvertStringToDays(string source)
        {
            if(string.IsNullOrEmpty(source))
            {
                return 0;
            }
            DayOfWeekMask days = 0;
            foreach (var token in source.Split(','))
            {
                switch(token.ToUpper())
                {
                    case "MO":
                        days = days | DayOfWeekMask.Monday;
                        break;
                    case "TU":
                        days = days | DayOfWeekMask.Tuesday;
                        break;
                    case "WE":
                        days = days | DayOfWeekMask.Wednesday;
                        break;
                    case "TH":
                        days = days | DayOfWeekMask.Thursday;
                        break;
                    case "FR":
                        days = days | DayOfWeekMask.Friday;
                        break;
                    case "SA":
                        days = days | DayOfWeekMask.Saturday;
                        break;
                    case "SU":
                        days = days | DayOfWeekMask.Sunday;
                        break;
                }
            }
            return days;
        }

        public static string ConvertPatternToString(RecurrencePatternType source)
        {
            string day = "";
            foreach (var token in Enum.GetValues(typeof(RecurrencePatternType)))
            {
                RecurrencePatternType value = source & (RecurrencePatternType)token;
                switch (value)
                {
                    case RecurrencePatternType.Daily:
                        return "DAILY";
                    case RecurrencePatternType.Monthly:
                        return "MONTHLY";
                    case RecurrencePatternType.Weekly:
                        return "WEEKLY";
                    case RecurrencePatternType.Yearly:
                        return "YEARLY";
                }
            }
            return day;
        }

        public static RecurrencePatternType ConvertStringToPattern(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return 0;
            }
            foreach (var token in source.Split(','))
            {
                switch (token.ToUpper())
                {
                    case "DAILY":
                        return RecurrencePatternType.Daily;
                    case "WEEKLY":
                        return RecurrencePatternType.Weekly;
                    case "MONTHLY":
                        return RecurrencePatternType.Monthly;
                    case "YEARLY":
                        return RecurrencePatternType.Yearly;
                }
            }
            return 0;
        }
    }
}
