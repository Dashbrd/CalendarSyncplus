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
using System.Threading.Tasks;
using System.Waf.Applications;
using MahApps.Metro.Controls.Dialogs;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Common;
using OutlookGoogleSyncRefresh.Presentation.Views;

namespace OutlookGoogleSyncRefresh.Presentation.Services
{
    [Export(typeof (IMessageService))]
    public class MessageService : IMessageService
    {
        [ImportingConstructor]
        public MessageService(ShellView view)
        {
            View = view;
        }

        public ShellView View { get; set; }

        #region IMessageService Members

        public void ShowMessageAsync(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Ok",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            DispatcherHelper.CheckBeginInvokeOnUI(
                () => View.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, metroDialogSettings));
        }

        public void ShowMessageAsync(string message)
        {
            ShowMessageAsync(message, ApplicationInfo.ProductName);
        }

        public async Task<MessageDialogResult> ShowMessage(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Ok",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };
            MessageDialogResult result =
                await View.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, metroDialogSettings);
            return result;
        }

        public Task<MessageDialogResult> ShowMessage(string message)
        {
            return ShowMessage(message, ApplicationInfo.ProductName);
        }

        #endregion
    }
}