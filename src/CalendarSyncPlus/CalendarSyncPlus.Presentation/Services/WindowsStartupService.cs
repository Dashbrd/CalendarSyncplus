using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Security.Principal;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Services.Interfaces;
using CalendarSyncPlus.Services.Utilities;
using log4net;
using Microsoft.Win32;

namespace CalendarSyncPlus.Presentation.Services
{
    [Export(typeof(IWindowsStartupService))]
    public class WindowsStartupService : IWindowsStartupService
    {
        [ImportingConstructor]
        public WindowsStartupService(ApplicationLogger applicationLogger)
        {
            ApplicationLogger = applicationLogger.GetLogger(GetType());
        }

        public ILog ApplicationLogger { get; set; }

        #region IWindowsStartupService Members

        public void RunAtWindowsStartup()
        {
            try
            {
                AddApplicationToCurrentUserStartup();
            }
            catch (Exception ex)
            {
                ApplicationLogger.Error(ex);
            }
        }

        public void RemoveFromWindowsStartup()
        {
            try
            {
                RemoveApplicationFromCurrentUserStartup();
            }
            catch (Exception ex)
            {
                ApplicationLogger.Error(ex);
            }
        }

        #endregion

        public void AddApplicationToCurrentUserStartup()
        {
            using (
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                    true))
            {
                key.SetValue("CalendarSyncPlusStartup",
                    "\"" + Assembly.GetExecutingAssembly().Location + "\" " + Constants.Minimized);
            }
        }

        public void AddApplicationToAllUserStartup()
        {
            using (
                var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                    true))
            {
                key.SetValue("CalendarSyncPlusStartup",
                    "\"" + Assembly.GetExecutingAssembly().Location + "\" " + Constants.Minimized);
            }
        }

        public void RemoveApplicationFromCurrentUserStartup()
        {
            using (
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                    true))
            {
                key.DeleteValue("CalendarSyncPlusStartup", false);
            }
        }

        public void RemoveApplicationFromAllUserStartup()
        {
            using (
                var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",
                    true))
            {
                key.DeleteValue("CalendarSyncPlusStartup", false);
            }
        }

        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                var user = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
                ApplicationLogger.Error(ex);
            }
            catch (Exception ex)
            {
                isAdmin = false;
                ApplicationLogger.Error(ex);
            }
            return isAdmin;
        }
    }
}