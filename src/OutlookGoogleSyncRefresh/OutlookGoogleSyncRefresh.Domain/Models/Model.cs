#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Domain
//  *      Author:         Dave, Ankesh
//  *      Created On:     06-02-2015 12:22 PM
//  *      Modified On:    06-02-2015 12:22 PM
//  *      FileName:       Model.cs
//  * 
//  *****************************************************************************/

#endregion

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

using OutlookGoogleSyncRefresh.Common;

namespace OutlookGoogleSyncRefresh.Domain.Models
{
    [XmlType("ModelBase")]
    public class Model : System.Waf.Foundation.Model
    {
        /// <summary>
        ///     Set the property with the specified value. If the value is not equal with the field then the field is
        ///     set, a PropertyChanged event is raised and it returns true.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="field">Reference to the backing field of the property.</param>
        /// <param name="value">The new value for the property.</param>
        /// <param name="propertyName">
        ///     The property name. This optional parameter can be skipped
        ///     because the compiler is able to create it automatically.
        /// </param>
        /// <returns>True if the value has changed, false if the old and new value were equal.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => base.OnPropertyChanged(e));
        }
    }
}