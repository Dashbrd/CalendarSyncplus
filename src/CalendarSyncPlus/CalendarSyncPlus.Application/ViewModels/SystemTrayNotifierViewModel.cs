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
//  *      Created On:     20-02-2015 2:04 PM
//  *      Modified On:    20-02-2015 2:04 PM
//  *      FileName:       SystemTrayNotifierViewModel.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.ComponentModel.Composition;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Services.Interfaces;

#endregion

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class SystemTrayNotifierViewModel : ViewModel<ISystemTrayNotifierView>
    {
        #region Constructors

        [ImportingConstructor]
        public SystemTrayNotifierViewModel(ISystemTrayNotifierView view, IGuiInteractionService guiInteractionService)
            : base(view)
        {
            GuiInteractionService = guiInteractionService;
        }

        #endregion

        public void ShowBalloon()
        {
            ViewCore.ShowCustomBalloon();
        }

        public void ShowBalloon(string tooltipText)
        {
            ToolTipText = tooltipText;
            DispatcherHelper.CheckBeginInvokeOnUI(() => ViewCore.ShowCustomBalloon());
        }

        public void UpdateBalloonText(string tooltipText)
        {
            ToolTipText = tooltipText;
        }

        /// <summary>
        /// </summary>
        /// <param name="tooltipText"></param>
        /// <param name="timeoutInMilliseconds"></param>
        public void ShowBalloon(string tooltipText, int timeoutInMilliseconds)
        {
            ToolTipText = tooltipText;
            DispatcherHelper.CheckBeginInvokeOnUI(() => ViewCore.ShowCustomBalloon(timeoutInMilliseconds));
        }

        public void HideBalloon()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => ViewCore.CloseBalloon());
        }

        public void Quit()
        {
            ViewCore.Quit();
        }

        #region Fields

        private DelegateCommand _doubleClickCommand;
        private DelegateCommand _exitApplicationCommand;
        private DelegateCommand _showApplicationCommand;
        private string _toolTipText;
        private DelegateCommand _syncNowCommand;

        #endregion

        #region Properties

        public string ToolTipText
        {
            get { return _toolTipText; }
            set { SetProperty(ref _toolTipText, value); }
        }

        public IGuiInteractionService GuiInteractionService { get; set; }

        public DelegateCommand DoubleClickCommand
        {
            get { return _doubleClickCommand ?? (_doubleClickCommand = new DelegateCommand(ShowApplication)); }
        }

        public DelegateCommand ShowApplicationCommand
        {
            get
            {
                return _showApplicationCommand ??
                       (_showApplicationCommand = new DelegateCommand(ShowApplication, CanShowApplication));
            }
        }

        public DelegateCommand ExitCommand
        {
            set { SetProperty(ref _exitApplicationCommand, value); }
            get { return _exitApplicationCommand; }
        }

        public DelegateCommand SyncNowCommand
        {
            get { return _syncNowCommand; }
            set { SetProperty(ref _syncNowCommand, value); }
        }

        #endregion

        #region Private Methods

        private void ShowApplication(object obj)
        {
            GuiInteractionService.ShowApplication();
        }

        private bool CanShowApplication(object canShowApplication)
        {
            return canShowApplication == null || (bool) canShowApplication;
        }

        #endregion
    }
}