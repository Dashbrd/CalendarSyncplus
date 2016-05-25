using System;
using System.Waf.Applications;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace CalendarSyncPlus.BootstrapperUI
{
    public class MainViewModel : DataModel
    {
        //constructor
        public MainViewModel(BootstrapperApplication bootstrapper)
        {
            IsThinking = false;
            Bootstrapper = bootstrapper;
            Bootstrapper.ApplyComplete += OnApplyComplete;
            Bootstrapper.DetectPackageComplete += OnDetectPackageComplete;
            Bootstrapper.PlanComplete += OnPlanComplete;
        }

        #region Properties

        private bool installEnabled;

        public bool InstallEnabled
        {
            get { return installEnabled; }
            set
            {
                installEnabled = value;
                RaisePropertyChanged("InstallEnabled");
            }
        }

        private bool uninstallEnabled;

        public bool UninstallEnabled
        {
            get { return uninstallEnabled; }
            set
            {
                uninstallEnabled = value;
                RaisePropertyChanged("UninstallEnabled");
            }
        }

        private bool isThinking;

        public bool IsThinking
        {
            get { return isThinking; }
            set
            {
                isThinking = value;
                RaisePropertyChanged("IsThinking");
            }
        }

        public BootstrapperApplication Bootstrapper { get; }

        #endregion //Properties

        #region Methods

        private void InstallExecute()
        {
            IsThinking = true;
            Bootstrapper.Engine.Plan(LaunchAction.Install);
        }

        private void UninstallExecute()
        {
            IsThinking = true;
            Bootstrapper.Engine.Plan(LaunchAction.Uninstall);
        }

        private void ExitExecute()
        {
            ManagedBootstrapperApplication.BootstrapperDispatcher.InvokeShutdown();
        }

        /// <summary>
        ///     Method that gets invoked when the <see cref="Bootstrapper" />
        ///     ApplyComplete event is fired. This is called after a bundle
        ///     installation has completed. Make sure we updated the view.
        /// </summary>
        private void OnApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            IsThinking = false;
            InstallEnabled = false;
            UninstallEnabled = false;
        }

        /// <summary>
        ///     Method that gets invoked when the <see cref="Bootstrapper" />
        ///     DetectPackageComplete event is fired. Checks the PackageId and sets
        ///     the installation scenario. The PackageId is the ID specified in one
        ///     of the package elements (msipackage, exepackage, msppackage,
        ///     msupackage) in the WiX bundle.
        /// </summary>
        private void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            if (e.PackageId == "DummyInstallationPackageId")
            {
                if (e.State == PackageState.Absent)
                    InstallEnabled = true;
                else if (e.State == PackageState.Present)
                    UninstallEnabled = true;
            }
        }

        /// <summary>
        ///     Method that gets invoked when the <see cref="Bootstrapper" />
        ///     PlanComplete event is fired. If the planning was successful, it
        ///     instructs the <see cref="Bootstrapper" /> <see cref="Engine" /> to
        ///     install the packages.
        /// </summary>
        private void OnPlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (e.Status >= 0)
                Bootstrapper.Engine.Apply(IntPtr.Zero);
        }

        #endregion //Methods

        #region DelegateCommands

        private DelegateCommand installCommand;

        public DelegateCommand InstallCommand
        {
            get
            {
                if (installCommand == null)
                    installCommand = new DelegateCommand(() => InstallExecute(), () => InstallEnabled);
                return installCommand;
            }
        }

        private DelegateCommand uninstallCommand;

        public DelegateCommand UninstallCommand
        {
            get
            {
                if (uninstallCommand == null)
                    uninstallCommand = new DelegateCommand(() => UninstallExecute(), () => UninstallEnabled);
                return uninstallCommand;
            }
        }

        private DelegateCommand exitCommand;

        public DelegateCommand ExitCommand
        {
            get
            {
                if (exitCommand == null)
                    exitCommand = new DelegateCommand(() => ExitExecute());
                return exitCommand;
            }
        }

        #endregion //DelegateCommands
    }
}