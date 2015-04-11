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
//  *      Created On:     11-02-2015 10:09 AM
//  *      Modified On:    11-02-2015 10:13 AM
//  *      FileName:       OutlookMailBox.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.Collections.Generic;
using System.Waf.Foundation;

#endregion

namespace CalendarSyncPlus.Domain.Models
{
    public class OutlookMailBox : Model
    {
        #region Fields

        private List<OutlookCalendar> _calendars;
        private string _entryId;
        private string _storeId;
        private string name;

        #endregion

        public OutlookMailBox()
        {
            _calendars = new List<OutlookCalendar>();
        }

        #region Properties

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public List<OutlookCalendar> Calendars
        {
            get { return _calendars; }
            set { SetProperty(ref _calendars, value); }
        }

        public string EntryId
        {
            get { return _entryId; }
            set { SetProperty(ref _entryId, value); }
        }

        public string StoreId
        {
            get { return _storeId; }
            set { SetProperty(ref _storeId, value); }
        }

        #endregion
    }
}