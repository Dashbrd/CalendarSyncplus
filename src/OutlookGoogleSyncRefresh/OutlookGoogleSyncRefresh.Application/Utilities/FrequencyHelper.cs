using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutlookGoogleSyncRefresh.Domain.Models;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    public class FrequencyHelper
    {
        public static Frequency GetGoogleFrequency(string recurrence)
        {
            var frequency = new Frequency();
            //RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR
            string[] values = recurrence.Split(new[] {":", ";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in values)
            {
                if (value.Contains("="))
                {
                    var parameter = value.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries);
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
                            if (DateTime.TryParse(parameter[1], out endDate))
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

      
    }
}
