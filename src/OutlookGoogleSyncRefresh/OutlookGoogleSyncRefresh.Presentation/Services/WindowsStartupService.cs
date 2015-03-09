using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Security.Principal;
using Microsoft.Win32;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Utilities;

namespace OutlookGoogleSyncRefresh.Presentation.Services
{
    [Export(typeof(IWindowsStartupService))]
    public class WindowsStartupService : IWindowsStartupService
    {
        public void RunAtWindowsStartup()
        {
            try
            {
                AddApplicationToCurrentUserStartup();
            }
            catch (Exception)
            {

            }
        }

        public void RemoveFromWindowsStartup()
        {
            try
            {
                RemoveApplicationFromCurrentUserStartup();
            }
            catch (Exception)
            {

            }
        }

        public void AddApplicationToCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("CalendarSyncPlusStartup", "\"" + Assembly.GetExecutingAssembly().Location + "\" " + Constants.Minimized);
            }
        }

        public void AddApplicationToAllUserStartup()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("CalendarSyncPlusStartup", "\"" + Assembly.GetExecutingAssembly().Location + "\" " + Constants.Minimized);
            }
        }

        public void RemoveApplicationFromCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue("CalendarSyncPlusStartup", false);
            }
        }

        public void RemoveApplicationFromAllUserStartup()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
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
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
