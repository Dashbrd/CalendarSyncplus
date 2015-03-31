#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     OutlookGoogleSyncRefresh
//  *      Author:         Dave, Ankesh
//  *      Created On:     23-01-2015 1:55 PM
//  *      Modified On:    02-02-2015 2:50 PM
//  *      FileName:       App.xaml.cs
//  * 
//  *****************************************************************************/

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Threading;
using CalendarSyncPlus.Application.Controllers;
using CalendarSyncPlus.Application.Services;
using CalendarSyncPlus.Application.ViewModels;
using CalendarSyncPlus.Common;
using CalendarSyncPlus.Common.Log;
using CalendarSyncPlus.Domain.Models;
using CalendarSyncPlus.Presentation.Helpers;
using CalendarSyncPlus.Presentation.Services.SingleInstance;

#endregion

namespace CalendarSyncPlus.Presentation
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : ISingleInstanceApp
    {
        #region Fields

        private static ApplicationLogger _applicationLogger;
        private readonly bool _startMinimized;
        private AggregateCatalog catalog;
        private CompositionContainer container;
        private IApplicationController controller;

        #endregion

        static App()
        {
            DispatcherHelper.Initialize();
        }

        public App(bool startMinimized = false)
        {
            _startMinimized = startMinimized;
        }

        #region Properties

        #endregion

        #region Private Methods

        private void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception, false);
        }

        private static void HandleException(Exception exception, bool isTerminating)
        {
            if (exception == null)
            {
                return;
            }

            Trace.TraceError(exception.ToString());

            if (!isTerminating)
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture,
                    "Unknown Error Occurred:{1}{0}", exception, Environment.NewLine)
                    , ApplicationInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
                _applicationLogger.LogInfo(exception.ToString());
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

            catalog = new AggregateCatalog();
            // Add the WpfApplicationFramework assembly to the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof (ViewModel).Assembly));
            // Add the Waf.BookLibrary.Library.Presentation assembly to the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            // Add the Waf.BookLibrary.Library.Applications assembly to the catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof (ShellViewModel).Assembly));
            // Add the Common assembly to catalog
            catalog.Catalogs.Add(new AssemblyCatalog(typeof (ApplicationLogger).Assembly));

            //Composition Container
            container = new CompositionContainer(catalog, true);
            var batch = new CompositionBatch();
            batch.AddExportedValue(container);
            container.Compose(batch);

            Settings settings = container.GetExportedValue<ISettingsProvider>().GetSettings();
            container.ComposeExportedValue(settings);
            _applicationLogger = container.GetExportedValue<ApplicationLogger>();
            _applicationLogger.Setup();
            //Initialize Application Controller
            controller = container.GetExportedValue<IApplicationController>();

            controller.Initialize();
            controller.Run(_startMinimized);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //Shutdown application Controller
            controller.Shutdown();

            //Dispose All parts
            container.Dispose();
            catalog.Dispose();
        }

        #endregion

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            //Activate Hidden,Background Application
            Current.Dispatcher.BeginInvoke(((Action) (() => Utilities.BringToForeground(MainWindow))));
            return true;
        }

        #endregion
    }
}