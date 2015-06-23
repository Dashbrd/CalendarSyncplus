using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using CalendarSyncPlus.Presentation.Controls.HtmlXamlConversion;

namespace CalendarSyncPlus.Presentation.Controls.RichTextBox.Formatters
{
    /// <summary>
    ///     Formats the <see cref="BindableHtmlRichTextBox" /> text as Html
    /// </summary>
    public class HtmlFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            var tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (var ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                return HtmlFromXamlConverter.ConvertXamlToHtml(Encoding.Default.GetString(ms.ToArray()));
            }
        }

        /// <exception cref="InvalidDataException">
        ///     data provided is not in the correct Html format.
        /// </exception>
        public void SetText(FlowDocument document, string text)
        {
            text = HtmlToXamlConverter.ConvertHtmlToXaml(text, false);
            try
            {
                var tr = new TextRange(document.ContentStart, document.ContentEnd);
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(text)))
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