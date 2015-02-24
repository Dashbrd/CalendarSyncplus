using System;
using System.ComponentModel.Composition;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using OutlookGoogleSyncRefresh.Application.Views;
using OutlookGoogleSyncRefresh.Helpers;

namespace OutlookGoogleSyncRefresh.Presentation.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    [Export,Export(typeof(IShellView))]
    public partial class ShellView : IShellView
    {

        public ShellView()
        {
            InitializeComponent();
            StateChanged+=OnShellViewWindowStateChanged;
        }

        private void OnShellViewWindowStateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Minimized:
                    Utilities.HideForeground(this);
                    break;
            } 
        }

        public bool IsMaximized
        {
            get { return WindowState == WindowState.Maximized; }
            set
            {
                if (value)
                {
                    WindowState = WindowState.Maximized;
                }
                else if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
            }
        }

    }
}
