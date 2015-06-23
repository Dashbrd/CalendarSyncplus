using System;

namespace CalendarSyncPlus.GoogleServices.Utilities
{
    public static class ExtensionMethods
    {
        public static DateTime LastOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        public static DateTime FirstOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        ///     Gets the first week day following a date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="dayOfWeek">The day of week to return.</param>
        /// <returns>
        ///     The first <paramref name="dayOfWeek" /> day following date, or
        ///     <paramref name="date" /> if it is on dayOfWeek.
        /// </returns>
        public static DateTime Previous(this DateTime date, DayOfWeek dayOfWeek)
        {
            var days = (dayOfWeek > date.DayOfWeek ? 7 : 0) + date.DayOfWeek - dayOfWeek;
            return date.AddDays(-days);
        }

        /// <summary>
        /// </summary>
        /// <param name="date"></param>
        /// <param name="nthWeek"></param>
        /// <param name="dayofWeek"></param>
        /// <returns>
        /// </returns>
        public static DateTime GetLastNthWeekofMonth(this DateTime date, int nthWeek, DayOfWeek dayofWeek)
        {
            return date.Previous(dayofWeek).AddDays((nthWeek - 1)*-7);
        }

        /// <summary>
        ///     Gets the first week day following a date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="dayOfWeek">The day of week to return.</param>
        /// <returns>
        ///     The first <paramref name="dayOfWeek" /> day following date, or
        ///     <paramref name="date" /> if it is on dayOfWeek.
        /// </returns>
        public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
        {
            return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
        }

        /// <summary>
        /// </summary>
        /// <param name="date"></param>
        /// <param name="nthWeek"></param>
        /// <param name="dayofWeek"></param>
        /// <returns>
        /// </returns>
        public static DateTime GetNthWeekofMonth(this DateTime date, int nthWeek, DayOfWeek dayofWeek)
        {
            return date.Next(dayofWeek).AddDays((nthWeek - 1)*7);
        }
    }
}