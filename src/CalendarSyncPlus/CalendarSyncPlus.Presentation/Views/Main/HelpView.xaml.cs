using System.ComponentModel.Composition;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views.Main
{
    /// <summary>
    ///     Interaction logic for HelpView.xaml
    /// </summary>
    [Export(typeof(IHelpView))]
    public partial class HelpView : IHelpView
    {
        public HelpView()
        {
            InitializeComponent();
        }
    }
}