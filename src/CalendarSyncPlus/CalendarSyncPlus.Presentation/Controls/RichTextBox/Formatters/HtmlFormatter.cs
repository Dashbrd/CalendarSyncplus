using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using CalendarSyncPlus.Presentation.Controls.HtmlXamlConversion;

namespace CalendarSyncPlus.Presentation.Controls.RichTextBox.Formatters
{
    /// <summary>
    /// Formats the BindableHtmlRichTextBox text as Html
    /// </summary>
    public class HtmlFormatter : ITextFormatter
    {
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return HtmlFromXamlConverter.ConvertXamlToHtml(Encoding.Default.GetString(ms.ToArray()));
            }
        }

        /// <exception cref="InvalidDataException">data provided is not in the correct Html format.</exception>
        public void SetText(System.Windows.Documents.FlowDocument document, string text)
        {
            text = HtmlToXamlConverter.ConvertHtmlToXaml(text, false);
            try
            {
                TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                {
                    tr.Load(ms, DataFormats.Xaml);
                }
            }
            catch (Exception exception)
            {
                throw new InvalidDataException("data provided is not in the correct Html format.", exception);
            }
        }
    }
}
