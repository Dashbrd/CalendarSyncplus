#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Domain
//  *      Author:         Dave, Ankesh
//  *      Created On:     11-02-2015 10:12 AM
//  *      Modified On:    11-02-2015 10:36 AM
//  *      FileName:       OutlookCalendar.cs
//  * 
//  *****************************************************************************/

#endregion

using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models.Preferences
{    
    [DataContract]
    public class OutlookFolder : Model
    {
        #region Fields
        private string entryId;
        private string name;
        private string storeId;

        #endregion

        #region Properties
        [DataMember]
        public string EntryId
        {
            get { return entryId; }
            set { SetProperty(ref entryId, value); }
        }
        [DataMember]
        public string StoreId
        {
            get { return storeId; }
            set { SetProperty(ref storeId, value); }
        }
        [DataMember]
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        #endregion
    }
}