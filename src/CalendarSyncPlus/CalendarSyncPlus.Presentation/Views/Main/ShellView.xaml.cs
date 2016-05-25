using System;
using System.ComponentModel.Composition;
using System.Windows;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Presentation.Helpers;
using MahApps.Metro.SimpleChildWindow;

namespace CalendarSyncPlus.Presentation.Views.Main
{
    /// <summary>
    ///     Interaction logic for ShellView.xaml
    /// </summary>
    [Export, Export(typeof(IShellView))]
    public partial class ShellView : IShellView
    {
        public ShellView()
        {
            InitializeComponent();
            StateChanged += OnShellViewWindowStateChanged;
        }

        #region IShellView Members

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

        public async void ShowChildWindow(object childViewContent)
        {
            await
                this.ShowChildWindowAsync(new ChildView {ChildContentView = childViewContent},
                    ChildWindowManager.OverlayFillBehavior.FullWindow);
        }

        #endregion

        private void OnShellViewWindowStateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Minimized:
                    Utilities.HideForeground(this);
                    break;
            }
        }
    }
}