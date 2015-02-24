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
//  *      Created On:     06-02-2015 1:06 PM
//  *      Modified On:    06-02-2015 1:06 PM
//  *      FileName:       PropertyChangedEventListener.cs
//  * 
//  *****************************************************************************/
#endregion

using System;
using System.ComponentModel;
using System.Windows;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    internal class PropertyChangedEventListener : IWeakEventListener
    {
        private readonly INotifyPropertyChanged source;
        private readonly PropertyChangedEventHandler handler;


        public PropertyChangedEventListener(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }
            this.source = source;
            this.handler = handler;
        }


        public INotifyPropertyChanged Source { get { return source; } }

        public PropertyChangedEventHandler Handler { get { return handler; } }


        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            handler(sender, (PropertyChangedEventArgs)e);
            return true;
        }
    }
}