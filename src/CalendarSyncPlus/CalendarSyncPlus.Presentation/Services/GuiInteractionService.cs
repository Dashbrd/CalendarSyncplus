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
//  *      Created On:     20-02-2015 3:18 PM
//  *      Modified On:    20-02-2015 3:18 PM
//  *      FileName:       UiInteractionService.cs
//  * 
//  *****************************************************************************/

#endregion

using System;
using System.ComponentModel.Composition;
using CalendarSyncPlus.Presentation.Helpers;
using CalendarSyncPlus.Presentation.Views;
using CalendarSyncPlus.Presentation.Views.Main;
using CalendarSyncPlus.Services.Interfaces;

namespace CalendarSyncPlus.Presentation.Services
{
    [Export(typeof (IGuiInteractionService))]
    public class GuiInteractionService : IGuiInteractionService
    {
        [ImportingConstructor]
        public GuiInteractionService(ShellView shellView)
        {
            ShellView = shellView;
        }

        public ShellView ShellView { get; set; }

        #region IGuiInteractionService Members

        public void ShowApplication()
        {
            ShellView.Dispatcher.BeginInvoke(((Action) (() => Utilities.BringToForeground(ShellView))));
        }

        public void HideApplication()
        {
            ShellView.Dispatcher.BeginInvoke(((Action) (() => Utilities.HideForeground(ShellView))));
        }

        #endregion
    }
}