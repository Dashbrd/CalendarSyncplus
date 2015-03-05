#region File Header
// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     Test
//  *      Author:         Dave, Ankesh
//  *      Created On:     10-02-2015 3:58 PM
//  *      Modified On:    10-02-2015 3:58 PM
//  *      FileName:       Program.cs
//  * 
//  *****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Outlook;
using Microsoft.Win32;

using OutlookGoogleSyncRefresh.Application.Services.ExchangeWeb;
using OutlookGoogleSyncRefresh.Domain.Models;

using Test.Model;
using Test.Services;

namespace Test
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main()
        {
            //var application = new App();

            //application.InitializeComponent();
            //application.Run();

            var service = new ExchangeWebCalendarService();

            var calendars = service.GetCalendarsAsync();
            service.GetAppointmentsAsync(7, 7, "", calendars[0]);


            //var list = GetOutlookProfileList();


        }

       

        public static List<string> GetOutlookProfileList()
        {
            var profileList = new List<string>();
            const string defaultProfilePath = @"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles";
            const string newProfilePath = @"Software\Microsoft\Office\15.0\Outlook\Profiles";

            var defaultRegKey = Registry.CurrentUser.OpenSubKey(defaultProfilePath);

            if (defaultRegKey != null)
            {
                var list = defaultRegKey.GetSubKeyNames();

                if (list.Any())
                {
                    profileList.AddRange(list);
                }
            }

            var newregKey = Registry.CurrentUser.OpenSubKey(newProfilePath, RegistryKeyPermissionCheck.Default);

            if (newregKey != null)
            {
                var list = newregKey.GetSubKeyNames();

                if (list.Any())
                {
                    foreach (string name in list.Where(name => !profileList.Contains(name)))
                    {
                        profileList.Add(name);
                    }
                }
            }

            return profileList;
        }
        
    }
}
