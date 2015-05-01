using System.Windows.Documents;

namespace CalendarSyncPlus.Presentation.Controls.RichTextBox.Formatters
{
    public interface ITextFormatter
    {
        string GetText(FlowDocument document);
        void SetText(FlowDocument document, string text);
    }
}
