using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls.Primitives;

using Hardcodet.Wpf.TaskbarNotification;

using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Presentation.Views
{
    /// <summary>
    /// Interaction logic for SystemTrayNotifierView.xaml
    /// </summary>
    [Export, Export(typeof(ISystemTrayNotifierView))]
    public partial class SystemTrayNotifierView : TaskbarIcon, ISystemTrayNotifierView
    {
        public SystemTrayNotifierView()
        {
            InitializeComponent();
        }

        public void ShowCustomBalloon()
        {
            var syncBalloon = (UIElement)this.Resources["_syncBalloon"];
            this.ShowCustomBalloon(syncBalloon, PopupAnimation.Slide, 5000);
        }
    }
}
