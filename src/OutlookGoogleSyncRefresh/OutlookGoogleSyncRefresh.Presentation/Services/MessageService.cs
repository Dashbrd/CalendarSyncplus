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
//  *      Created On:     19-02-2015 3:57 PM
//  *      Modified On:    19-02-2015 3:57 PM
//  *      FileName:       MessageService.cs
//  * 
//  *****************************************************************************/
#endregion

using System.ComponentModel.Composition;
using System.Waf.Applications;
using MahApps.Metro.Controls.Dialogs;

using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Presentation.Views;

namespace OutlookGoogleSyncRefresh.Presentation.Services
{
    [Export(typeof(IMessageService))]
    public class MessageService : IMessageService
    {
        public ShellView View { get; set; }

        [ImportingConstructor]
        public MessageService(ShellView view)
        {
            View = view;
        }

        public void ShowMessageAsync(string message, string title)
        {
             
            var metroDialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText="Ok",
                AnimateHide=true,
                AnimateShow=true,
                ColorScheme=MetroDialogColorScheme.Accented
            };
            View.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, metroDialogSettings);
        }

        public void ShowMessageAsync(string message)
        {
            ShowMessageAsync(message, ApplicationInfo.ProductName);
        }
    }
}