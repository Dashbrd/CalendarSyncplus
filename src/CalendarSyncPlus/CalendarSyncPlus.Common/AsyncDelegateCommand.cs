using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CalendarSyncPlus.Common
{
    /// <summary>
    /// Provides an <see cref="ICommand"/> implementation which relays the <see cref="Execute"/> and <see cref="CanExecute"/> 
    /// method to the specified delegates.
    /// This implementation disables the command during the async command execution.
    /// https://github.com/jbe2277/waf/blob/master/src/System.Waf/System.Waf/System.Waf.Core/Applications/AsyncDelegateCommand.cs
    /// </summary>
    public class AsyncDelegateCommand : ICommand
    {
        private static readonly Task<object> CompletedTask = Task.FromResult((object)null);
        private readonly Func<object, Task> _execute;
        private readonly Func<object, bool> _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">Async Delegate to execute when Execute is called on the command.</param>
        /// <exception cref="ArgumentNullException">The execute argument must not be null.</exception>
        public AsyncDelegateCommand(Func<Task> execute)
            : this(execute, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">Async Delegate to execute when Execute is called on the command.</param>
        /// <exception cref="ArgumentNullException">The execute argument must not be null.</exception>
        public AsyncDelegateCommand(Func<object, Task> execute)
            : this(execute, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">Async Delegate to execute when Execute is called on the command.</param>
        /// <param name="canExecute">Delegate to execute when CanExecute is called on the command.</param>
        /// <exception cref="ArgumentNullException">The execute argument must not be null.</exception>
        public AsyncDelegateCommand(Func<Task> execute, Func<bool> canExecute)
            : this(execute != null ? p => execute() : (Func<object, Task>)null, canExecute != null ? p => canExecute() : (Func<object, bool>)null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">Async Delegate to execute when Execute is called on the command.</param>
        /// <param name="canExecute">Delegate to execute when CanExecute is called on the command.</param>
        /// <exception cref="ArgumentNullException">The execute argument must not be null.</exception>
        public AsyncDelegateCommand(Func<object, Task> execute, Func<object, bool> canExecute)
        {
            if (execute == null) { throw new ArgumentNullException(nameof(execute)); }

            this._execute = execute;
            this._canExecute = canExecute;
        }


        /// <summary>
        /// Returns a disabled command.
        /// </summary>
        public static AsyncDelegateCommand DisabledCommand { get; } = new AsyncDelegateCommand(() => CompletedTask, () => false);

        private bool IsExecuting
        {
            get { return _isExecuting; }
            set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    RaiseCanExecuteChanged();
                }
            }
        }


        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;


        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return !IsExecuting && (_canExecute == null || _canExecute(parameter));
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            IsExecuting = true;
            try
            {
                await _execute(parameter);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }
    }
}