using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using OutlookGoogleSyncRefresh.Helper;
using Test.Model;
using Test.Services;
using Calendar = Test.Model.Calendar;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_Outlook(object sender, RoutedEventArgs e)
        {
            MyGrid.IsEnabled = false;
            var outlookService = new OutlookCalendarService();
            outlookService.GetOutlookAppointmentsAsync(0, 3).ContinueWith(ContinuationAction, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ContinuationAction(Task<List<Appointment>> task)
        {
            MyGrid.IsEnabled = true;
            MyListBox.ItemsSource = task.Result;
        }

        private void Button_Click_Google(object sender, RoutedEventArgs e)
        {
            MyGrid.IsEnabled = false;

            // Get Current Sync Context
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            //Start a background Task passing SyncContext for last operation
            Task.Factory.StartNew(() => StartAuthentication(scheduler));
        }

        private void StartAuthentication(TaskScheduler taskScheduler)
        {
            //// Authenticate Oauth2
            string clientId = "68932173233-e7nf2hlqhl9o16knj0iatg82bmgdkvom.apps.googleusercontent.com";

            string clientSecret = "QRDGYelWPnCOsCiWlXVl6Cc3";
            string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
            string userName = "user"; //  A string used to identify a user (locally).

            var calenderService = (new AccountAuthentication()).AuthenticateCalenderOauth(clientId, clientSecret, userName, "OutlookGoogleSyncRefresh.Auth.Store", "OutlookGoogleSyncRefresh");
            var googleCalenderService = new GoogleCalendarService(calenderService);

            googleCalenderService.GetAvailableCalendars()
                .ContinueWith(task => ContinuationActionGoogle(task, googleCalenderService,taskScheduler));
        }

        private void ContinuationActionGoogle(Task<List<Calendar>> task, GoogleCalendarService googleCalenderService,
            TaskScheduler taskScheduler)
        {
            string calenderId = string.Empty;
            var calender = task.Result.FirstOrDefault(calendar => calendar.Name.Contains("Test"));

            if (calender != null)
            {
                calenderId = calender.Id;
            }

            

            googleCalenderService.GetCalendarEventsInRangeAsync(0, 1, calenderId)
                .ContinueWith(ContinuationActionGoogleAppointments, taskScheduler);

        }

        private void ContinuationActionGoogleAppointments(Task<List<Appointment>> task)
        {
            MyGrid.IsEnabled = true;
            MyListBox.ItemsSource = task.Result;
        }

        private async Task<List<Appointment>> GetEqualAppointments(GoogleCalendarService googleCalenderService,OutlookCalendarService outlookCalendarService,string calenderId)
        {
            List<Appointment> googleAppointments = await googleCalenderService.GetCalendarEventsInRangeAsync(0, 2,calenderId);
            List<Appointment> outLookAppointments = await outlookCalendarService.GetOutlookAppointmentsAsync(0, 2);

            return (from lookAppointment in outLookAppointments
                let isAvailable = googleAppointments.Any(appointment => appointment.Equals(lookAppointment))
                where isAvailable
                select lookAppointment).ToList();
        }

        private void Button_Click_Selected(object sender, RoutedEventArgs e)
        {
            MyGrid.IsEnabled = false;
            //// Authenticate Oauth2
            string clientId = "68932173233-e7nf2hlqhl9o16knj0iatg82bmgdkvom.apps.googleusercontent.com";

            string clientSecret = "QRDGYelWPnCOsCiWlXVl6Cc3";
            string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
            string userName = "user"; //  A string used to identify a user (locally).

            var calenderService = (new AccountAuthentication()).AuthenticateCalenderOauth(clientId, clientSecret, userName, "OutlookGoogleSyncRefresh.Auth.Store", "OutlookGoogleSyncRefresh");

            var googleCalenderService = new GoogleCalendarService(calenderService);
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            googleCalenderService.GetAvailableCalendars()
                .ContinueWith(
                    task =>
                        GetInformation(task.Result, scheduler,
                            googleCalenderService));


        }

        private void GetInformation(IEnumerable<Calendar> list, TaskScheduler currentSynchronizationContext, GoogleCalendarService googleCalenderService)
        {
            string calenderId = string.Empty;
            var calender = list.FirstOrDefault(calendar => calendar.Name.Contains("Office"));

            if (calender != null)
            {
                calenderId = calender.Id;
            }
            var outlookService = new OutlookCalendarService();

            GetEqualAppointments(googleCalenderService,outlookService,calenderId).ContinueWith(ContinuationActionGoogleAppointments, currentSynchronizationContext);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var outlookService = new OutlookCalendarService();
            outlookService.GetOutlookAppointmentsAsync(0, 2).ContinueWith(task => AddEventToTest(task.Result, scheduler));
        }

        private GoogleCalendarService GetGoogleCalendarService()
        {
            //// Authenticate Oauth2
            string clientId = "68932173233-e7nf2hlqhl9o16knj0iatg82bmgdkvom.apps.googleusercontent.com";

            string clientSecret = "QRDGYelWPnCOsCiWlXVl6Cc3";
            string redirectUri = "urn:ietf:wg:oauth:2.0:oob";
            string userName = "user"; //  A string used to identify a user (locally).

            var calenderService =(new AccountAuthentication()).AuthenticateCalenderOauth(clientId, clientSecret, userName, "OutlookGoogleSyncRefresh.Auth.Store", "OutlookGoogleSyncRefresh");

            return new GoogleCalendarService(calenderService);
        }

        private void AddEventToTest(List<Appointment> result, TaskScheduler scheduler)
        {
            var calendarService = GetGoogleCalendarService();
            var calenderId = ConfigurationManager.AppSettings["Test"];

            calendarService.AddCalendarEvent(result[0], calenderId, true, true, true).ContinueWith(ContinuationAction, scheduler);
        }

        private void ContinuationAction(Task<bool> task)
        {
            
        }
    }
}
