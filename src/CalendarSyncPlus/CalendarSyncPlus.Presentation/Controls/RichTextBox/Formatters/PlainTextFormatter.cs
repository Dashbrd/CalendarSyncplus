using System.Windows.Documents;

namespace CalendarSyncPlus.Presentation.Controls.RichTextBox.Formatters
{
    /// <summary>
    ///     Formats the <see cref="BindableHtmlRichTextBox" /> text as plain text
    /// </summary>
    public class PlainTextFormatter : ITextFormatter
    {
        #region ITextFormatter Members

        public string GetText(FlowDocument document)
        {
            return new TextRange(document.ContentStart, document.ContentEnd).Text;
        }

        public void SetText(FlowDocument document, string text)
        {
            new TextRange(document.ContentStart, document.ContentEnd).Text = text;
        }

        #endregion
    }
}