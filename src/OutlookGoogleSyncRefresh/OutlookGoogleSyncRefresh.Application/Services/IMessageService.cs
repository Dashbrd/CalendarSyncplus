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
//  *      Created On:     19-02-2015 3:33 PM
//  *      Modified On:    19-02-2015 3:33 PM
//  *      FileName:       IMessageService.cs
//  * 
//  *****************************************************************************/

#endregion

using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace OutlookGoogleSyncRefresh.Application.Services
{
    public interface IMessageService
    {
        void ShowMessageAsync(string message, string title);

        void ShowMessageAsync(string message);

        Task<MessageDialogResult> ShowMessage(string message, string title);

        Task<MessageDialogResult> ShowMessage(string message);

        void ShowConfirmMessageAsync(string message, string title);

        void ShowConfirmMessageAsync(string message);

        Task<MessageDialogResult> ShowConfirmMessage(string message, string title);

        Task<MessageDialogResult> ShowConfirmMessage(string message);
    }
}