using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using CalendarSyncPlus.Presentation.Controls.RichTextBox.Formatters;

namespace CalendarSyncPlus.Presentation.Controls.RichTextBox
{
    public class BindableHtmlRichTextBox : System.Windows.Controls.RichTextBox
    {
        #region Private Members

        bool isInvokePending;

        #endregion //Private Members

        #region Constructors

        public BindableHtmlRichTextBox()
        {
            Loaded += RichTextBox_Loaded;

            //Added
            this.IsReadOnly = true;
            this.Focusable = false;
        }

        public BindableHtmlRichTextBox(System.Windows.Documents.FlowDocument document)
            : base(document)
        {

        }

        #endregion //Constructors

        #region Properties

        private ITextFormatter _textFormatter;
        /// <summary>
        /// The ITextFormatter the is used to format the text of the BindableHtmlRichTextBox.
        /// Deafult formatter is the RtfFormatter
        /// </summary>
        public ITextFormatter TextFormatter
        {
            get
            {
                if (_textFormatter == null)
                    _textFormatter = new HtmlFormatter(); //default is rtf

                return _textFormatter;
            }
            set
            {
                _textFormatter = value;
            }
        }

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(BindableHtmlRichTextBox),
            new FrameworkPropertyMetadata(
                String.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(OnTextPropertyChanged),
                new CoerceValueCallback(CoerceTextProperty),
                true,
                System.Windows.Data.UpdateSourceTrigger.LostFocus));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableHtmlRichTextBox rtb = (BindableHtmlRichTextBox)d;

            rtb.TextFormatter.SetText(rtb.Document, (string)e.NewValue);
        }

        private static object CoerceTextProperty(DependencyObject d, object value)
        {
            return value ?? "";
        }

        #endregion //Text

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

        #region Event Hanlders

        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding = BindingOperations.GetBinding(this, TextProperty);

            if (binding != null)
            {
                if (binding.UpdateSourceTrigger == UpdateSourceTrigger.Default || binding.UpdateSourceTrigger == UpdateSourceTrigger.LostFocus)
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
    }
}
