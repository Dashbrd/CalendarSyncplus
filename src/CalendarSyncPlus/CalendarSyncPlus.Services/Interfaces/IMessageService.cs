#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     19-02-2015 3:33 PM
//  *      Modified On:    19-02-2015 3:33 PM
//  *      FileName:       IMessageService.cs
//  * 
//  *****************************************************************************/

#endregion

using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace CalendarSyncPlus.Services.Interfaces
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

        Task<string> ShowInput(string message, string title);

        Task<string> ShowInput(string message);

        void ShowProgressAsync(string message, string title);

        void ShowProgressAsync(string message);

        Task<ProgressDialogController> ShowProgress(string message, string title);

        Task<ProgressDialogController> ShowProgress(string message);


        Task<string> ShowCustomDialog(string message, string title,int maxLength);

        Task<string> ShowCustomDialog(string message);
    }
}