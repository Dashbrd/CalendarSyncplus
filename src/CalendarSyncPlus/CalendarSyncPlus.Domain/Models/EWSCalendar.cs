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

using System.Waf.Foundation;

namespace CalendarSyncPlus.Domain.Models
{
    public class EWSCalendar : Model
    {
        private string entryId;
        private string name;
        private string storeId;

        public string EntryId
        {
            get { return entryId; }
            set { SetProperty(ref entryId, value); }
        }

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public string StoreId
        {
            get { return storeId; }
            set { SetProperty(ref storeId, value); }
        }
    }
}