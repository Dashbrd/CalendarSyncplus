#region File Header
// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh.Presentation
//  *      Author:         Dave, Ankesh
//  *      Created On:     20-02-2015 3:20 PM
//  *      Modified On:    20-02-2015 3:20 PM
//  *      FileName:       Utilities.cs
//  * 
//  *****************************************************************************/
#endregion

using System.Windows;

using OutlookGoogleSyncRefresh.Presentation.Views;

namespace OutlookGoogleSyncRefresh.Helpers
{
    public static class Utilities
    {
        public static void BringToForeground(Window mainWindow)
        {
            if (mainWindow.WindowState == WindowState.Minimized || mainWindow.Visibility == Visibility.Hidden)
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
            }

            // According to some sources these steps gurantee that an app will be brought to foreground.
            mainWindow.Activate();
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.Focus();
            mainWindow.ShowInTaskbar = true;
        }

        public static void HideForeground(Window mainWindow)
        {
            mainWindow.WindowState = WindowState.Minimized;
            mainWindow.ShowInTaskbar = false;
            mainWindow.Hide();
        }
    }
}