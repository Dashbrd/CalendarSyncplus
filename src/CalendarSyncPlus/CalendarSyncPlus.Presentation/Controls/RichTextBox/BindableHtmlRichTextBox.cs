using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using CalendarSyncPlus.Presentation.Controls.RichTextBox.Formatters;

namespace CalendarSyncPlus.Presentation.Controls.RichTextBox
{
    public class BindableHtmlRichTextBox : System.Windows.Controls.RichTextBox
    {
        private bool isInvokePending;

        #region Event Hanlders

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var binding = BindingOperations.GetBinding(this, TextProperty);

            if (binding != null)
            {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default ||
                    binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
                {
                    LostFocus += (o, ea) => UpdateText(); //do this synchronously
                }
                else
                {
                    TextChanged += (o, ea) => InvokeUpdateText(); //do this async
                }
            }
        }

        #endregion //Event Hanlders

        #region Constructors

        public BindableHtmlRichTextBox()
        {
            Loaded += RichTextBox_Loaded;

            //Added
            IsReadOnly = true;
            Focusable = false;
        }

        public BindableHtmlRichTextBox(FlowDocument document)
            : base(document)
        {
        }

        #endregion //Constructors

        #region Properties

        private ITextFormatter _textFormatter;

        /// <summary>
        ///     The <see cref="ITextFormatter" /> the is used to format the text of
        ///     the BindableHtmlRichTextBox. Deafult formatter is the
        ///     <see cref="RtfFormatter" />
        /// </summary>
        public ITextFormatter TextFormatter
        {
            get { return _textFormatter ?? (_textFormatter = new RtfFormatter()); }
            set { _textFormatter = value; }
        }

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(BindableHtmlRichTextBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextPropertyChanged,
                CoerceTextProperty,
                true,
                UpdateSourceTrigger.LostFocus));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rtb = (BindableHtmlRichTextBox) d;

            rtb.TextFormatter.SetText(rtb.Document, (string) e.NewValue);
        }

        private static object CoerceTextProperty(DependencyObject d, object value)
        {
            return value ?? "";
        }

        #endregion //Text

        public TextFormatterType TextFormatterType
        {
            get { return (TextFormatterType) GetValue(TextFormatterTypeProperty); }
            set { SetValue(TextFormatterTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextFormatterType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextFormatterTypeProperty =
            DependencyProperty.Register("TextFormatterType", typeof(TextFormatterType),
                typeof(BindableHtmlRichTextBox),
                new FrameworkPropertyMetadata(TextFormatterType.Default,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextFormatterTypeChanged));

        private static void OnTextFormatterTypeChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textBox = (BindableHtmlRichTextBox) dependencyObject;

            var formatterType = (TextFormatterType) dependencyPropertyChangedEventArgs.NewValue;
            ITextFormatter formatter = new RtfFormatter();
            switch (formatterType)
            {
                case TextFormatterType.Html:
                    formatter = new HtmlFormatter();
                    break;
                case TextFormatterType.Rtf:
                    formatter = new RtfFormatter();
                    break;
                case TextFormatterType.PlainText:
                    formatter = new PlainTextFormatter();
                    break;
                case TextFormatterType.Xaml:
                    formatter = new XamlFormatter();
                    break;
                case TextFormatterType.Default:
                    break;
            }
            textBox.TextFormatter = formatter;
        }

        #endregion //Properties

        #region Methods

        private void InvokeUpdateText()
        {
            if (!isInvokePending)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(UpdateText));
                isInvokePending = true;
            }
        }

        private void UpdateText()
        {
            Text = TextFormatter.GetText(Document);

            isInvokePending = false;
        }

        #endregion //Methods
    }
}