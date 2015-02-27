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
//  *      Created On:     20-02-2015 2:04 PM
//  *      Modified On:    20-02-2015 2:04 PM
//  *      FileName:       SystemTrayNotifierViewModel.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System.ComponentModel.Composition;
using System.Waf.Applications;
using OutlookGoogleSyncRefresh.Application.Services;
using OutlookGoogleSyncRefresh.Application.Views;

#endregion

namespace OutlookGoogleSyncRefresh.Application.ViewModels
{
    [Export]
    public class SystemTrayNotifierViewModel : Utilities.ViewModel<ISystemTrayNotifierView>
    {
        #region Fields

        private DelegateCommand _doubleClickCommand;
        private DelegateCommand _exitApplicationCommand;
        private DelegateCommand _showApplicationCommand;
        private string _toolTipText;

        #endregion

        #region Constructors

        [ImportingConstructor]
        public SystemTrayNotifierViewModel(ISystemTrayNotifierView view, IGuiInteractionService guiInteractionService)
            : base(view)
        {
            GuiInteractionService = guiInteractionService;
        }

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

        #endregion

        #region Private Methods

        private void ShowApplication(object obj)
        {
            GuiInteractionService.ShowApplication();
        }

        private bool CanShowApplication(object canShowApplication)
        {
            return canShowApplication == null || (bool)canShowApplication;
        }

        #endregion

        public void ShowBalloon()
        {
            ViewCore.ShowCustomBalloon();
        }

        public void ShowBalloon(string tooltipText)
        {
            ToolTipText = tooltipText;
            ViewCore.ShowCustomBalloon();
        }

        public void HideBalloon()
        {
            ViewCore.CloseBalloon();
        }
    }
}