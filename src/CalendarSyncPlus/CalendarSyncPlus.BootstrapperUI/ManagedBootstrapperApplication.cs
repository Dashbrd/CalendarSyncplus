using System;
using System.Windows;
using System.Windows.Threading;
using CalendarSyncPlus.BootstrapperUI.Views;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace CalendarSyncPlus.BootstrapperUI
{
    public class ManagedBootstrapperApplication : BootstrapperApplication
    {
        // global dispatcher
        public static Dispatcher BootstrapperDispatcher { get; private set; }
        // entry point for our custom UI
        protected override void Run()
        {
            try
            {
                Engine.Log(LogLevel.Verbose, "Launching custom ManagedBootstrapperApplication UX");
                BootstrapperDispatcher = Dispatcher.CurrentDispatcher;

                var viewModel = new MainViewModel(this);
                Engine.Detect();

                var view = new MainView();
                view.DataContext = viewModel;
                view.Closed += (sender, e) => BootstrapperDispatcher.InvokeShutdown();
                view.Show();
                Dispatcher.Run();

                Engine.Quit(0);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                Engine.Quit(0);
            }
        }
    }
}