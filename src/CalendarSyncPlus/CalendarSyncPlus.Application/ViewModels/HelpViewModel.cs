using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Waf.Applications;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common.Log;
using log4net;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export]
    public class HelpViewModel : ViewModel<IHelpView>
    {
        [ImportingConstructor]
        public HelpViewModel(IHelpView helpView, ApplicationLogger applicationLogger)
            : base(helpView)
        {
            Logger = applicationLogger.GetLogger(GetType());
            LoadHelpFile();
        }

        public ILog Logger { get; set; }

        public FixedDocumentSequence FixedDocument { get; set; }

        private void LoadHelpFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                if (directory != null)
                {
                    directory = Path.Combine(directory, "UserGuide");
                    var fileName = Path.Combine(directory, "HowToUseGuide.xps");
                    var doc = new XpsDocument(fileName, FileAccess.Read);

                    FixedDocument = doc.GetFixedDocumentSequence();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }
    }
}