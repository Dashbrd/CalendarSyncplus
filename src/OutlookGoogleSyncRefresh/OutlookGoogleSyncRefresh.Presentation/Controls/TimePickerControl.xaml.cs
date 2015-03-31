using System;
using System.Windows;
using System.Windows.Controls;

namespace CalendarSyncPlus.Presentation.Controls
{
    /// <summary>
    ///     Interaction logic for TimePickerControl.xaml
    /// </summary>
    public partial class TimePickerControl : UserControl
    {
        // Using a DependencyProperty as the backing store for TimeValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeValueProperty =
            DependencyProperty.Register("TimeValue", typeof (DateTime), typeof (TimePickerControl),
                new FrameworkPropertyMetadata(TimeValueChangedCallback));


        // Using a DependencyProperty as the backing store for TimeFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeFormatProperty =
            DependencyProperty.Register("TimeFormat", typeof (string), typeof (TimePickerControl),
                new FrameworkPropertyMetadata(TimeFormatChangedCallback));

        private bool IsLoading;


        public TimePickerControl()
        {
            InitializeComponent();
        }

        public DateTime TimeValue
        {
            get { return (DateTime) GetValue(TimeValueProperty); }
            set { SetValue(TimeValueProperty, value); }
        }

        public string TimeFormat
        {
            get { return (string) GetValue(TimeFormatProperty); }
            set { SetValue(TimeFormatProperty, value); }
        }

        private void Hours_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            UpdateTime(DateTime.Now);
        }

        private void Minutes_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            UpdateTime(DateTime.Now);
        }

        private void Seconds_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            UpdateTime(DateTime.Now);
        }

        private static void TimeValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dateTime = (DateTime) e.NewValue;
            var timePickerControl = d as TimePickerControl;
            if (timePickerControl != null)
            {
                LoadTime(timePickerControl, dateTime);
            }
        }

        private static void TimeFormatChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timePickerControl = d as TimePickerControl;
            if (timePickerControl != null)
            {
                LoadTime(timePickerControl, timePickerControl.TimeValue);
            }
        }

        private void TtCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTime(DateTime.Now);
        }

        private void UpdateTime(DateTime today)
        {
            if (IsLoading)
            {
                return;
            }
            IsLoading = true;
            var hours = (int) HoursNumericUpDown.Value.GetValueOrDefault();
            if (TimeFormat != null && TimeFormat.StartsWith("hh"))
            {
                if (ttCombobox.SelectedIndex == 1)
                {
                    HoursNumericUpDown.Maximum = 12;
                    hours += 12;
                }
                else
                {
                    HoursNumericUpDown.Maximum = 11;
                }
            }
            var minutes = (int) MinutesNumericUpDown.Value.GetValueOrDefault();
            var seconds = (int) SecondsNumericUpDown.Value.GetValueOrDefault();
            TimeValue = today.Date.Add(new TimeSpan(hours, minutes, seconds));
            IsLoading = false;
        }

        private static void LoadTime(TimePickerControl control, DateTime dateTime)
        {
            if (control.IsLoading)
            {
                return;
            }
            control.IsLoading = true;
            if (control.TimeFormat == null)
            {
                control.HoursNumericUpDown.Maximum = 23;
                control.HoursNumericUpDown.Value = dateTime.Hour;
                control.MinutesNumericUpDown.Value = dateTime.Minute;
                control.SecondsNumericUpDown.Value = dateTime.Second;
                control.MinutesSeparator.Visibility = Visibility.Visible;
                control.SecondsNumericUpDown.Visibility = Visibility.Visible;
                control.ttCombobox.Visibility = Visibility.Collapsed;
                control.IsLoading = false;
                return;
            }

            switch (control.TimeFormat)
            {
                case "HH:mm:ss":
                    control.HoursNumericUpDown.Maximum = 23;
                    control.HoursNumericUpDown.Value = dateTime.Hour;
                    control.MinutesNumericUpDown.Value = dateTime.Minute;
                    control.SecondsNumericUpDown.Value = dateTime.Second;
                    control.MinutesSeparator.Visibility = Visibility.Visible;
                    control.SecondsNumericUpDown.Visibility = Visibility.Visible;
                    control.ttCombobox.Visibility = Visibility.Collapsed;
                    break;
                case "HH:mm":
                    control.HoursNumericUpDown.Maximum = 23;
                    control.HoursNumericUpDown.Value = dateTime.Hour;
                    control.MinutesNumericUpDown.Value = dateTime.Minute;
                    control.SecondsNumericUpDown.Value = dateTime.Second;
                    control.MinutesSeparator.Visibility = Visibility.Collapsed;
                    control.SecondsNumericUpDown.Visibility = Visibility.Collapsed;
                    control.ttCombobox.Visibility = Visibility.Collapsed;
                    break;
                case "hh:mm:ss tt":
                    control.MinutesNumericUpDown.Value = dateTime.Minute;
                    control.SecondsNumericUpDown.Value = dateTime.Second;
                    control.MinutesSeparator.Visibility = Visibility.Visible;
                    control.SecondsNumericUpDown.Visibility = Visibility.Visible;
                    control.ttCombobox.Visibility = Visibility.Visible;
                    if (dateTime.Hour > 12)
                    {
                        control.HoursNumericUpDown.Value = dateTime.Hour%12;
                        control.HoursNumericUpDown.Maximum = 12;
                        control.ttCombobox.SelectedIndex = 1;
                    }
                    else
                    {
                        control.HoursNumericUpDown.Value = dateTime.Hour;
                        control.HoursNumericUpDown.Maximum = 11;
                        control.ttCombobox.SelectedIndex = 0;
                    }
                    break;
                case "hh:mm tt":
                    control.MinutesNumericUpDown.Value = dateTime.Minute;
                    control.SecondsNumericUpDown.Value = dateTime.Second;
                    control.MinutesSeparator.Visibility = Visibility.Collapsed;
                    control.SecondsNumericUpDown.Visibility = Visibility.Collapsed;
                    control.ttCombobox.Visibility = Visibility.Visible;
                    if (dateTime.Hour > 12)
                    {
                        control.HoursNumericUpDown.Value = dateTime.Hour%12;
                        control.HoursNumericUpDown.Maximum = 12;
                        control.ttCombobox.SelectedIndex = 1;
                    }
                    else
                    {
                        control.HoursNumericUpDown.Value = dateTime.Hour;
                        control.HoursNumericUpDown.Maximum = 11;
                        control.ttCombobox.SelectedIndex = 0;
                    }
                    break;
                default:
                    throw new FormatException("Invalid time format.");
            }
            control.IsLoading = false;
        }
    }
}