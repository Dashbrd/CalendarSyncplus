#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     06-02-2015 11:25 AM
//  *      Modified On:    06-02-2015 11:27 AM
//  *      FileName:       ExtensionMethods.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#endregion

namespace CalendarSyncPlus.Services.Utilities
{
    public static class ExtensionMethods
    {
        #region Private Methods

        public static bool IsValidEmailAddress(this string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            string emailRegex = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" +
                                @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

            return Regex.IsMatch(email, emailRegex);
        }

        private static IEnumerable<T> GetChunk<T>(this IEnumerator<T> enumerator,
            int chunkSize)
        {
            do
                yield return enumerator.Current;
            while (--chunkSize > 0 && enumerator.MoveNext());
        }

        #endregion

        #region Public Methods

        public static IEnumerable<IEnumerable<T>> Chunkify<T>(this IEnumerable<T> enumerable,
            int chunkSize)
        {
            if (chunkSize < 1)
            {
                throw new ArgumentException("chunkSize must be positive");
            }

            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    yield return enumerator.GetChunk(chunkSize);
                }
        }


        public static void AddSorted<T>(this IList<T> list, T item, IComparer<T> comparer = null)
        {
            if (comparer == null)
                comparer = Comparer<T>.Default;

            int i = 0;
            while (i < list.Count && comparer.Compare(list[i], item) < 0)
                i++;

            list.Insert(i, item);
        }
        #endregion
    }
}