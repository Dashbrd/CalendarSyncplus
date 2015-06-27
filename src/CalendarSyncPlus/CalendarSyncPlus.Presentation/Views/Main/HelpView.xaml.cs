using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Xps.Packaging;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
{
    /// <summary>
    ///     Interaction logic for HelpView.xaml
    /// </summary>
    [Export(typeof (IHelpView))]
    public partial class HelpView : IHelpView
    {
        public HelpView()
        {
            InitializeComponent();
        }
    }
}