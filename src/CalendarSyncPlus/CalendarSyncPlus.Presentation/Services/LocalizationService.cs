#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Presentation
//  *      Author:         Dave, Ankesh
//  *      Created On:     29-04-2015 12:00 PM
//  *      Modified On:    29-04-2015 12:00 PM
//  *      FileName:       LocalizationService.cs
//  * 
//  *****************************************************************************/

#endregion

using System.ComponentModel.Composition;
using System.Globalization;
using CalendarSyncPlus.Common;
using WPFLocalizeExtension.Engine;

namespace CalendarSyncPlus.Presentation.Services
{
    [Export(typeof(ILocalizationService))]
    public class LocalizationService : ILocalizationService
    {
        #region ILocalizationService Members

        public string GetLocalizedString(string key)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key.Trim()))
            {
                return string.Empty;
            }
            var localizedObject = LocalizeDictionary.Instance.GetLocalizedObject(string.Empty, string.Empty, key,
                LocalizeDictionary.Instance.Culture);
            var actualString = localizedObject as string;
            return actualString;
        }

        public CultureInfo CurrentCulture
        {
            get { return LocalizeDictionary.Instance.Culture; }
        }

        #endregion
    }
}