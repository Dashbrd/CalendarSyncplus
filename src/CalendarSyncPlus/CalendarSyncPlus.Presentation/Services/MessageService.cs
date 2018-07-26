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
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Presentation.Views;
using CalendarSyncPlus.Presentation.Views.Helper;
using CalendarSyncPlus.Presentation.Views.Main;
using CalendarSyncPlus.Services.Interfaces;
using MahApps.Metro.Controls.Dialogs;

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

        public async void ShowMessage(string message, string title)
        {
            //var metroDialogSettings = new MetroDialogSettings
            //{
            //    AffirmativeButtonText = "OK",
            //    AnimateHide = true,
            //    AnimateShow = true,
            //    ColorScheme = MetroDialogColorScheme.Accented
            //};
            // await InvokeOnCurrentDispatcher(async () =>
            //{
            //    var taskResult =
            //        await View.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, metroDialogSettings);
            //    return taskResult;
            //});

            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            var dialog = new AutoCloseMessageDialog(View, metroDialogSettings)
            {
                Message = message,
                Title = title
            };

            await InvokeOnCurrentDispatcher(() =>
            {
                View.ShowMetroDialogAsync(dialog, metroDialogSettings);

                return Task.WhenAny(dialog.WaitForButtonPressAsync(), Task.Delay(5000)).ContinueWith(m =>
                {
                    InvokeOnCurrentDispatcher(() => View.HideMetroDialogAsync(dialog));
                    return m.Result;
                }); 
            });
        }

        public void ShowMessage(string message)
        {
            ShowMessage(message, ApplicationInfo.ProductName);           
        }

        public void ShowConfirmMessageAsync(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "YES",
                NegativeButtonText = "NO",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
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
                var taskResult =
                    await View.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative,
                        metroDialogSettings);
                return taskResult;
            });
        }

        public Task<MessageDialogResult> ShowConfirmMessage(string message)
        {
            return ShowConfirmMessage(message, ApplicationInfo.ProductName);
        }

        public Task<string> ShowInputAsync(string message)
        {
            return ShowInputAsync(message, ApplicationInfo.ProductName);
        }

        public Task<string> ShowInputAsync(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                NegativeButtonText = "CANCEL",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            return InvokeOnCurrentDispatcher(() => { return View.ShowInputAsync(title, message, metroDialogSettings); });
        }

        public Task<string> ShowInput(string message)
        {
            return ShowInput(message, ApplicationInfo.ProductName);
        }


        public Task<string> ShowInput(string message, string title)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                NegativeButtonText = "CANCEL",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            return InvokeOnCurrentDispatcher(() =>
            {
                return View.ShowInputAsync(title, message, metroDialogSettings);
                //return result;
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
                var controller =
                    await View.ShowProgressAsync(title, message, false, metroDialogSettings);
                return controller;
            });
        }

        public Task<ProgressDialogController> ShowProgress(string message)
        {
            return ShowProgress(message, ApplicationInfo.ProductName);
        }

        #endregion

        private T InvokeOnCurrentDispatcher<T>(Func<T> action)
        {
            return DispatcherHelper.CheckInvokeOnUI(action);
        }

        public Task<string> ShowCustomInput(string message, string title, int maxLength = 15)
        {
            var metroDialogSettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "OK",
                NegativeButtonText = "CANCEL",
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Accented
            };

            var dialog = new CustomInputDialog(View, metroDialogSettings)
            {
                Message = message,
                Title = title,
                Input = metroDialogSettings.DefaultText,
                MaxInputLength = maxLength
            };

            return InvokeOnCurrentDispatcher(() =>
            {
                View.ShowMetroDialogAsync(dialog, metroDialogSettings);

                return dialog.WaitForButtonPressAsync().ContinueWith(m =>
                {
                    InvokeOnCurrentDispatcher(() => View.HideMetroDialogAsync(dialog));
                    return m.Result;
                });
            });
        }

        public Task<string> ShowCustomInput(string message)
        {
            return ShowCustomInput(message, ApplicationInfo.ProductName);
        }
    }
}