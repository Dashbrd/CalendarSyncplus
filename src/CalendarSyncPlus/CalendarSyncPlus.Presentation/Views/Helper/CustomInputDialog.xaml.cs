using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace CalendarSyncPlus.Presentation.Views.Helper
{
    /// <summary>
    ///     Interaction logic for CustomInputDialog.xaml
    /// </summary>
    public partial class CustomInputDialog : BaseMetroDialog
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message",
            typeof (string), typeof (CustomInputDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof (string),
            typeof (CustomInputDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty AffirmativeButtonTextProperty =
            DependencyProperty.Register("AffirmativeButtonText", typeof (string), typeof (CustomInputDialog),
                new PropertyMetadata("OK"));

        public static readonly DependencyProperty NegativeButtonTextProperty =
            DependencyProperty.Register("NegativeButtonText", typeof (string), typeof (CustomInputDialog),
                new PropertyMetadata("Cancel"));

        public static readonly DependencyProperty MaxInputLengthProperty = DependencyProperty.Register(
            "MaxInputLength", typeof (int), typeof (CustomInputDialog), new PropertyMetadata(15));

        internal CustomInputDialog(MetroWindow parentWindow)
            : this(parentWindow, null)
        {
        }

        internal CustomInputDialog(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
        }

        public string Message
        {
            get { return (string) GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string Input
        {
            get { return (string) GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public string AffirmativeButtonText
        {
            get { return (string) GetValue(AffirmativeButtonTextProperty); }
            set { SetValue(AffirmativeButtonTextProperty, value); }
        }

        public string NegativeButtonText
        {
            get { return (string) GetValue(NegativeButtonTextProperty); }
            set { SetValue(NegativeButtonTextProperty, value); }
        }

        public int MaxInputLength
        {
            get { return (int) GetValue(MaxInputLengthProperty); }
            set { SetValue(MaxInputLengthProperty, value); }
        }

        internal Task<string> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Focus();
                PART_TextBox.Focus();
            }));

            var tcs = new TaskCompletionSource<string>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            KeyEventHandler escapeKeyHandler = null;

            Action cleanUpHandlers = () =>
            {
                PART_TextBox.KeyDown -= affirmativeKeyHandler;

                KeyDown -= escapeKeyHandler;

                PART_NegativeButton.Click -= negativeHandler;
                PART_AffirmativeButton.Click -= affirmativeHandler;

                PART_NegativeButton.KeyDown -= negativeKeyHandler;
                PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;
            };

            escapeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            negativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(Input);
                }
            };

            negativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(null);

                e.Handled = true;
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(Input);

                e.Handled = true;
            };

            PART_NegativeButton.KeyDown += negativeKeyHandler;
            PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            PART_TextBox.KeyDown += affirmativeKeyHandler;

            KeyDown += escapeKeyHandler;

            PART_NegativeButton.Click += negativeHandler;
            PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            AffirmativeButtonText = DialogSettings.AffirmativeButtonText;
            NegativeButtonText = DialogSettings.NegativeButtonText;

            switch (DialogSettings.ColorScheme)
            {
                case MetroDialogColorScheme.Accented:
                    PART_NegativeButton.Style = FindResource("HighlightedSquareButtonStyle") as Style;
                    PART_TextBox.SetResourceReference(ForegroundProperty, "BlackColorBrush");
                    break;
            }
        }
    }
}