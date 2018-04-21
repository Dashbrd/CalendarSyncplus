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
//  *      Created On:     01-04-2015 2:16 PM
//  *      Modified On:    01-04-2015 2:16 PM
//  *      FileName:       EWSCalendar.cs
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
    public class EWSCalendar : Model
    {
        private string _entryId;
        private string _name;
        private string _storeId;
        [DataMember]
        public string EntryId
        {
            get { return _entryId; }
            set { SetProperty(ref _entryId, value); }
        }
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        [DataMember]
        public string StoreId
        {
            get { return _storeId; }
            set { SetProperty(ref _storeId, value); }
        }
    }
}