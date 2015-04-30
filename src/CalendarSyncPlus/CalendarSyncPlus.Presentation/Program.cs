#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus
//  *      Author:         Dave, Ankesh
//  *      Created On:     02-02-2015 11:31 AM
//  *      Modified On:    02-02-2015 11:31 AM
//  *      FileName:       Program.cs
//  * 
//  *****************************************************************************/

#endregion

using System;
using System.Linq;
using CalendarSyncPlus.Presentation.Services.SingleInstance;
using CalendarSyncPlus.Services.Utilities;

namespace CalendarSyncPlus.Presentation
{
    internal sealed class Program
    {
        private const string Unique = "6DBA7828-5EDA-4215-9B75-0A7C2E6C9FD9-CalendarSyncPlus";

        [STAThread]
        //[DebuggerNonUserCode]
        public static void Main(string[] args)
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                try
                {
                    bool startMinimized = false;
                    if (args != null)
                    {
                        string minimized = args.FirstOrDefault();
                        if (minimized != null && minimized.Equals(Constants.Minimized))
                        {
                            startMinimized = true;
                        }
                    }
                    var application = new App(startMinimized);
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