using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;
using CalendarSyncPlus.Common.Log;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export,Export(typeof(IChildContentViewModel))]
    [ExportMetadata("ChildViewContentType", ChildViewContentType.WhatsNew)]
    public class WhatsNewViewModel : ViewModel<IWhatsNewView>, IChildContentViewModel
    {
        public ApplicationLogger Logger { get; set; }
        private string _text;

        [ImportingConstructor]
        public WhatsNewViewModel(IWhatsNewView view,ApplicationLogger logger) : base(view)
        {
            Logger = logger;
            GetReleaseNotes();
        }

        private void GetReleaseNotes()
        {
            var releaseNotesPath= ApplicationInfo.ApplicationPath + @"\ReleaseNotes\Release Notes.rtf";

            if (!File.Exists(releaseNotesPath))
            {
                Logger.LogError(String.Format("Release Notes Not Found  at the location {0}{1}", Environment.NewLine,
                    releaseNotesPath));
                Text = @"<h1>No Release Notes Found. Report Error to Developers</h1>";
            }
            var notes=File.ReadAllText(releaseNotesPath);
            Text = notes;
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
    }
}