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
//  *      Created On:     06-02-2015 1:05 PM
//  *      Modified On:    06-02-2015 1:05 PM
//  *      FileName:       CollectionChangedEventListener.cs
//  * 
//  *****************************************************************************/

#endregion

using System;
using System.Collections.Specialized;
using System.Windows;

namespace OutlookGoogleSyncRefresh.Application.Utilities
{
    internal class CollectionChangedEventListener : IWeakEventListener
    {
        private readonly NotifyCollectionChangedEventHandler handler;
        private readonly INotifyCollectionChanged source;


        public CollectionChangedEventListener(INotifyCollectionChanged source,
            NotifyCollectionChangedEventHandler handler)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            this.source = source;
            this.handler = handler;
        }


        public INotifyCollectionChanged Source
        {
            get { return source; }
        }

        public NotifyCollectionChangedEventHandler Handler
        {
            get { return handler; }
        }

        #region IWeakEventListener Members

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            handler(sender, (NotifyCollectionChangedEventArgs) e);
            return true;
        }

        #endregion
    }
}