using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace CalendarSyncPlus.Presentation.Controls.RichTextBox.Formatters
{
    /// <summary>
    ///     Formats the <see cref="BindableHtmlRichTextBox" /> text as RTF
    /// </summary>
    public class RtfFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            var tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (var ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public void SetText(FlowDocument document, string text)
        {
            var tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(text)))
            {
                tr.Load(ms, DataFormats.Rtf);
            }
        }
    }
}