#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Application
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

#endregion

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    public static class ExtensionMethods
    {
        #region Private Methods

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

            using (var enumerator = enumerable.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    yield return enumerator.GetChunk(chunkSize);
                }
        }

        #endregion
    }
}