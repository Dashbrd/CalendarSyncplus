using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    /// Interaction logic for WhatsNewView.xaml
    /// </summary>
    [Export(typeof(IWhatsNewView))]
    public partial class WhatsNewView : UserControl,IWhatsNewView
    {
        public WhatsNewView()
        {
            InitializeComponent();
        }
    }
}
