using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CalendarSyncPlus.BootstrapperUI.Views;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using SplashScreen = CalendarSyncPlus.BootstrapperUI.Views.SplashScreen;

namespace CalendarSyncPlus.BootstrapperUI
{
    public class ManagedBootstrapperApplication : BootstrapperApplication
    {
        // global dispatcher
        static public Dispatcher BootstrapperDispatcher { get; private set; }
        // entry point for our custom UI
        protected override void Run()
        {
            try
            {
                this.Engine.Log(LogLevel.Verbose, "Launching custom ManagedBootstrapperApplication UX");
                BootstrapperDispatcher = Dispatcher.CurrentDispatcher;

                MainViewModel viewModel = new MainViewModel(this);
                this.Engine.Detect();

                MainView view = new MainView();
                view.DataContext = viewModel;
                view.Closed += (sender, e) => BootstrapperDispatcher.InvokeShutdown();
                view.Show();
                Dispatcher.Run();
                
                this.Engine.Quit(0);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                this.Engine.Quit(0);
            }
        }
    }
}
