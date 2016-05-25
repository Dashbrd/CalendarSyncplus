using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls.Primitives;
using CalendarSyncPlus.Application.Views;
using Hardcodet.Wpf.TaskbarNotification;

namespace CalendarSyncPlus.Presentation.Views.Notifications
{
    /// <summary>
    ///     Interaction logic for SystemTrayNotifierView.xaml
    /// </summary>
    [Export, Export(typeof(ISystemTrayNotifierView))]
    public partial class SystemTrayNotifierView : TaskbarIcon, ISystemTrayNotifierView
    {
        public SystemTrayNotifierView()
        {
            InitializeComponent();
        }

        #region ISystemTrayNotifierView Members

        public void ShowCustomBalloon()
        {
            var syncBalloon = new SyncBalloon();
            syncBalloon.DataContext = DataContext;
            ShowCustomBalloon(syncBalloon, PopupAnimation.Slide, null);
        }

        public void ShowCustomBalloon(int timeoutInMilliseconds)
        {
            var syncBalloon = new SyncBalloon();
            syncBalloon.DataContext = DataContext;
            ShowCustomBalloon(syncBalloon, PopupAnimation.Slide, timeoutInMilliseconds);
        }

        public void Quit()
        {
            Visibility = Visibility.Hidden;
        }

        #endregion
    }
}