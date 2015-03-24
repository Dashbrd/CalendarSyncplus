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
using OutlookGoogleSyncRefresh.Application.Views;

namespace OutlookGoogleSyncRefresh.Presentation.Views
{
    /// <summary>
    /// Interaction logic for ProfilesView.xaml
    /// </summary>
    [Export(typeof(IProfilesView))]
    public partial class ProfilesView : UserControl, IProfilesView
    {
        public ProfilesView()
        {
            InitializeComponent();
        }
    }
}
