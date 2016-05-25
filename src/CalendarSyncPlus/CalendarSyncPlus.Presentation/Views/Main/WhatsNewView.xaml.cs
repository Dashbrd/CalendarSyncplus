using System.ComponentModel.Composition;
using System.Windows.Controls;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for WhatsNewView.xaml
    /// </summary>
    [Export(typeof(IWhatsNewView))]
    public partial class WhatsNewView : UserControl, IWhatsNewView
    {
        public WhatsNewView()
        {
            InitializeComponent();
        }
    }
}