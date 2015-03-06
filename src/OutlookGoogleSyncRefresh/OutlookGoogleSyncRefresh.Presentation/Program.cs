#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh
//  *      Author:         Dave, Ankesh
//  *      Created On:     02-02-2015 11:31 AM
//  *      Modified On:    02-02-2015 11:31 AM
//  *      FileName:       Program.cs
//  * 
//  *****************************************************************************/

#endregion

using System;
using System.Diagnostics;

using OutlookGoogleSyncRefresh.Presentation.Services.SingleInstance;

namespace OutlookGoogleSyncRefresh.Presentation
{
    internal sealed class Program
    {
        private const string Unique = "6DBA7828-5EDA-4215-9B75-0A7C2E6C9FD9-OutlookGoogleSyncRefresh";

        [STAThread]
        //[DebuggerNonUserCode]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                try
                {
                    var application = new App();
                    application.InitializeComponent();
                    application.Run();
                }
                finally
                {
                    // Allow single instance code to perform cleanup operations
                    SingleInstance<App>.Cleanup();
                }
            }
        }
    }
}