using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CalendarSyncPlus.Presentation.Views.Helper
{
    /// <summary>
    /// Interaction logic for AutoCloseMessageDialog.xaml
    /// </summary>
    public partial class AutoCloseMessageDialog : BaseMetroDialog
    {       
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message",
            typeof(string), typeof(AutoCloseMessageDialog), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty AffirmativeButtonTextProperty =
            DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(AutoCloseMessageDialog),
                new PropertyMetadata("OK"));

        internal AutoCloseMessageDialog()
            : this(null, null)
        {
        }
        internal AutoCloseMessageDialog(MetroWindow parentWindow)
            : this(parentWindow, null)
        {
        }

        internal AutoCloseMessageDialog(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
            HandleVisibility(settings);
        }

        private void HandleVisibility(MetroDialogSettings settings)
        {
            this.Loaded += this.Dialog_Loaded;
            this.Unloaded += this.Dialog_Loaded;
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string AffirmativeButtonText
        {
            get { return (string)GetValue(AffirmativeButtonTextProperty); }
            set { SetValue(AffirmativeButtonTextProperty, value); }
        }

        internal Task<string> WaitForButtonPressAsync()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Focus();
            }));

            var tcs = new TaskCompletionSource<string>();

            RoutedEventHandler negativeHandler = null;
            KeyEventHandler negativeKeyHandler = null;

            RoutedEventHandler affirmativeHandler = null;
            KeyEventHandler affirmativeKeyHandler = null;

            KeyEventHandler escapeKeyHandler = null;

            Action cleanUpHandlers = () =>
            {
                KeyDown -= escapeKeyHandler;

                PART_AffirmativeButton.Click -= affirmativeHandler;

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

            affirmativeKeyHandler = (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    cleanUpHandlers();

                    tcs.TrySetResult(null);
                }
            };

            affirmativeHandler = (sender, e) =>
            {
                cleanUpHandlers();

                tcs.TrySetResult(null);

                e.Handled = true;
            };

            PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

            KeyDown += escapeKeyHandler;

            PART_AffirmativeButton.Click += affirmativeHandler;

            return tcs.Task;
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            AffirmativeButtonText = DialogSettings.AffirmativeButtonText;
        }
    }
}
