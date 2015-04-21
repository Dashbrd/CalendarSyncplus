#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Presentation
//  *      Author:         Dave, Ankesh
//  *      Created On:     19-02-2015 3:57 PM
//  *      Modified On:    19-02-2015 3:57 PM
//  *      FileName:       MessageService.cs
//  * 
//  *****************************************************************************/

#endregion

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows;
using CalendarSyncPlus.Common;
using CalendarSyncPlus.GoogleServices.Google;
using CalendarSyncPlus.Presentation.Views;
using CalendarSyncPlus.Services.Interfaces;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.CSharp.RuntimeBinder;

namespace CalendarSyncPlus.Presentation.Services
{
    [Export(typeof(IMessageService))]
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
                AffirmativeButtonText = "OK",
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
                AffirmativeButtonText = "OK",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };
            return await InvokeOnCurrentDispatcher(async () =>
            {
                MessageDialogResult taskResult =
                    await View.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, metroDialogSettings);
                return taskResult;
            });
        }

        public Task<MessageDialogResult> ShowMessage(string message)
        {
            return ShowMessage(message, ApplicationInfo.ProductName);
        }

        public void ShowConfirmMessageAsync(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "YES",
                NegativeButtonText = "NO",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented,
            };

            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                    View.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, metroDialogSettings));
        }

        public void ShowConfirmMessageAsync(string message)
        {
            ShowConfirmMessageAsync(message, ApplicationInfo.ProductName);
        }

        public async Task<MessageDialogResult> ShowConfirmMessage(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "YES",
                NegativeButtonText = "NO",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            return await InvokeOnCurrentDispatcher(async () =>
            {
                MessageDialogResult taskResult =
                    await View.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative,
                        metroDialogSettings);
                return taskResult;
            });
        }

        public Task<MessageDialogResult> ShowConfirmMessage(string message)
        {
            return ShowConfirmMessage(message, ApplicationInfo.ProductName);
        }


        public Task<string> ShowInput(string message)
        {
            return ShowInput(message, ApplicationInfo.ProductName);
        }


        public async Task<string> ShowInput(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                NegativeButtonText = "CANCEL",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented,
            };

            return await InvokeOnCurrentDispatcher(async () =>
            {
                string result = await View.ShowInputAsync(title, message, metroDialogSettings);
                return result;
            });
        }


        public void ShowProgressAsync(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            DispatcherHelper.CheckBeginInvokeOnUI(
                () => View.ShowProgressAsync(title, message, false, metroDialogSettings));
        }

        public void ShowProgressAsync(string message)
        {
            ShowProgressAsync(message, ApplicationInfo.ProductName);
        }

        public async Task<ProgressDialogController> ShowProgress(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            return await InvokeOnCurrentDispatcher(async () =>
            {
                ProgressDialogController controller =
                await View.ShowProgressAsync(title, message, false, metroDialogSettings);
                return controller;
            });
        }

        public Task<ProgressDialogController> ShowProgress(string message)
        {
            return ShowProgress(message, ApplicationInfo.ProductName);
        }

        public async Task<string> ShowCustomDialog(string message, string title,int maxLength=15)
        {
            var metroDialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "OK",
                NegativeButtonText = "CANCEL",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented,
            };

            var dialog = new CustomInputDialog(View, metroDialogSettings)
            {
                Message = message,
                Title = title,
                Input = metroDialogSettings.DefaultText,
                MaxInputLength=maxLength
            };

            return await InvokeOnCurrentDispatcher(async () =>
            {
                await View.ShowMetroDialogAsync(dialog, metroDialogSettings);

                return await dialog.WaitForButtonPressAsync().ContinueWith(m =>
                    {
                        InvokeOnCurrentDispatcher(() => View.HideMetroDialogAsync(dialog));
                        return m.Result;
                    });
            });
        }

        public Task<string> ShowCustomDialog(string message)
        {
            return ShowCustomDialog(message, ApplicationInfo.ProductName);
        }

        #endregion

        private T InvokeOnCurrentDispatcher<T>(Func<T> action)
        {
            return DispatcherHelper.CheckInvokeOnUI(action);
        }
    }
}