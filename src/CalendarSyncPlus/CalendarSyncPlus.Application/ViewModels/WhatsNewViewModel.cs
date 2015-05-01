using System.ComponentModel.Composition;
using System.Waf.Applications;
using CalendarSyncPlus.Application.Views;

namespace CalendarSyncPlus.Application.ViewModels
{
    [Export,Export(typeof(IChildContentViewModel))]
    [ExportMetadata("ChildViewContentType", ChildViewContentType.WhatsNew)]
    public class WhatsNewViewModel : ViewModel<IWhatsNewView>, IChildContentViewModel
    {
        private string _htmlText;

        [ImportingConstructor]
        public WhatsNewViewModel(IWhatsNewView view) : base(view)
        {
            HtmlText = "<h1>Cool View</h1>";
        }

        public string HtmlText
        {
            get { return _htmlText; }
            set { SetProperty(ref _htmlText, value); }
        }
    }
}