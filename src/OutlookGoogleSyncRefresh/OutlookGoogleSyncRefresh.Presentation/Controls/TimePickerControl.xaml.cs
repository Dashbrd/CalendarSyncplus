using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OutlookGoogleSyncRefresh.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for TimePickerControl.xaml
    /// </summary>
    public partial class TimePickerControl : UserControl
    {

        public DateTime TimeValue
        {
            get { return (DateTime)GetValue(TimeValueProperty); }
            set { SetValue(TimeValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeValueProperty =
            DependencyProperty.Register("TimeValue", typeof(DateTime), typeof(TimePickerControl), new PropertyMetadata(DateTime.Now));

        
        public string TimeFormat
        {
            get { return (string)GetValue(TimeFormatProperty); }
            set { SetValue(TimeFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeFormatProperty =
            DependencyProperty.Register("TimeFormat", typeof(string), typeof(TimePickerControl), new PropertyMetadata("HH:mm:ss"));

        
        public TimePickerControl()
        {
            InitializeComponent();
        }

        private void Hours_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            UpdateTime();
        }

        private void Minutes_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            UpdateTime();
        }

        private void Seconds_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            UpdateTime();
        }

        void UpdateTime()
        {
            var now = DateTime.Now.Date;
            int hours = (int) HoursNumericUpDown.Value.GetValueOrDefault();
            int minutes = (int) MinutesNumericUpDown.Value.GetValueOrDefault();
            int seconds = (int) SecondsNumericUpDown.Value.GetValueOrDefault();
            TimeValue = now.Add(new TimeSpan(hours, minutes, seconds));
        }
        private void TimePickerControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dateTime = DateTime.Now;
            
            switch (TimeFormat)
            {
                case "HH:mm:ss":
                    HoursNumericUpDown.Value = dateTime.Hour;
                    MinutesNumericUpDown.Value = dateTime.Minute;
                    break;
                case "HH:mm":
                    break;
                case "hh:mm:ss tt":
                    break;
                case "hh:mm tt":
                    break;
                default:
                    throw new FormatException("Invalid time format.");
            }
        }
    }
}
