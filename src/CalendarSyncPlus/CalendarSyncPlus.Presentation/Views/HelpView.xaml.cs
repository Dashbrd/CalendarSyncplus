using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Xps.Packaging;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Presentation.Views
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

        private void HelpView_OnLoaded(object sender, RoutedEventArgs e)
        {
            string directory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            directory = Path.Combine(directory, "UserGuide");
            string fileName = Path.Combine(directory, "HowToUseGuide.xps");
            XpsDocument doc = new XpsDocument(fileName, FileAccess.Read);

            HelpDocumentViewer.Document = doc.GetFixedDocumentSequence();
        }
    }
}