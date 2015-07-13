using System.ComponentModel.Composition;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views.Main
{
    /// <summary>
    ///     Interaction logic for AboutView.xaml
    /// </summary>
    [Export(typeof (IAboutView))]
    public partial class AboutView : IAboutView
    {
        public AboutView()
        {
            InitializeComponent();
        }
    }
}